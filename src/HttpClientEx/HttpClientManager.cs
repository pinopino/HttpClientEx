using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientEx.Management
{
    /// <summary>
    /// 负责创建可复用的HttpClient对象
    /// </summary>
    /// <remarks>
    /// 取HttpClientManager是为了区别微软官方提供的HttpClientFactory类型，尽量避免混淆。
    /// 创建的HttpClient对象是可复用的，正常的使用using块包裹也不会有任何问题；同时解决了
    /// DNS变动时状态更新的问题。
    /// </remarks>
    public class HttpClientManager
    {
        /// <summary>
        /// 默认请求超时时间
        /// </summary>
        public readonly static TimeSpan DefaultTimeout;

        public HttpClientHandler HttpHandler { private set; get; }

        static HttpClientManager()
        {
            //The number of connections per end point. default is 2-10 depending on environment.
            //Connections start to queue once this number is hit. The timeout clock starts
            //at send, so if you are stuck queuing your timeout clock is running, which leads to timeouts.
            ServicePointManager.DefaultConnectionLimit = 10;

            //This setting was put in place to save bandwidth before sending huge object
            //for post and put request to ensure remote end point is up and running.
            ServicePointManager.Expect100Continue = false;

            //Nagle’s algorithm is a means of improving the efficiency of TCP/IP networks
            //by reducing the number of packets that need to be sent over the network.
            //This can decrease the overall transmission overhead but can cause delay in data packet arrival.
            ServicePointManager.UseNagleAlgorithm = false;

            DefaultTimeout = TimeSpan.FromSeconds(5);
        }

        public HttpClientManager()
        { }

        public HttpClientManager(HttpClientHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            HttpHandler = handler;
            TimeoutHandler.SetShardHandler(HttpHandler);
        }

        public void InitHandler(HttpClientHandler handler)
        {
            if (HttpHandler != null)
                throw new InvalidOperationException("already bind a HttpClientHandler");

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            HttpHandler = handler;
            TimeoutHandler.SetShardHandler(HttpHandler);
        }

        /// <summary>
        /// 创建一个HttpClient对象
        /// </summary>
        public HttpClient CreateClient(int retry = 3)
        {
            if (HttpHandler == null)
                throw new InvalidOperationException("please init HttpClientHandler first");

            return CreateClient(DefaultTimeout, retry);
        }

        /// <summary>
        /// 创建一个HttpClient对象
        /// </summary>
        public HttpClient CreateClient(TimeSpan timeout, int retry = 3)
        {
            if (HttpHandler == null)
                throw new InvalidOperationException("please init HttpClientHandler first");

            return new HttpClient(new TimeoutHandler(timeout, retry), disposeHandler: false);
        }

        private class TimeoutHandler : DelegatingHandler
        {
            private static HttpClientHandler SharedHandler;
            private static readonly HashSet<string> Endpoints = new HashSet<string>();
            private static readonly TimeSpan ConnectionCloseTimeoutPeriod = TimeSpan.FromMinutes(2);

            private readonly int _retry;
            private readonly TimeSpan _timeout;

            public TimeoutHandler(TimeSpan timeout, int retry)
                : base(SharedHandler)
            {
                _timeout = timeout;
                _retry = retry;
            }

            public static void SetShardHandler(HttpClientHandler handler)
            {
                SharedHandler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // 通过设置Endpoint的LeaseTimeout防止DNS状态更新问题
                AddConnectionLeaseTimeout(request.RequestUri);

                return Execute.WithRetryAsync(async () =>
                {
                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        try
                        {
                            cts.CancelAfter(_timeout);
                            return await base.SendAsync(request, cts.Token);
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested == false)
                        {
                            throw new TimeoutException($"Request {request.RequestUri} timed out after {_timeout}");
                        }
                    }
                }, _retry);
            }

            private void AddConnectionLeaseTimeout(Uri endpoint)
            {
                // 无关url的查询参数，Endpoint只区分scheme，host以及port
                var hash = $"{endpoint.Scheme}{endpoint.Host}{endpoint.Port}";
                lock (Endpoints)
                {
                    if (Endpoints.Contains(hash))
                        return;

                    var sp = ServicePointManager.FindServicePoint(endpoint);
                    sp.ConnectionLeaseTimeout = (int)ConnectionCloseTimeoutPeriod.TotalMilliseconds;
                    Endpoints.Add(hash);
                }
            }
        }

        private static class RandomUtil
        {
            private static int _seedCounter;
            private static readonly ThreadLocal<Random> ThreadRandom;

            static RandomUtil()
            {
                ThreadRandom = new ThreadLocal<Random>(
                    () => new Random((int)DateTime.UtcNow.Ticks + Interlocked.Increment(ref _seedCounter)));
            }

            public static Random Random
            {
                get { return ThreadRandom.Value; }
            }
        }

        private static class Execute
        {
            public static async Task<T> WithRetryAsync<T>(Func<Task<T>> action, int maxRetry, int backoffMs = 100,
                Predicate<Exception> shouldRetry = null, Predicate<T> retryOnResult = null)
            {
                int retryCount = 0;
                while (true)
                {
                    try
                    {
                        var result = await action();
                        if (retryCount != maxRetry && retryOnResult?.Invoke(result) == true)
                            throw new Exception("Forcing retry");

                        return result;
                    }
                    catch (Exception ex)
                    {
                        if (retryCount == maxRetry || shouldRetry?.Invoke(ex) == false)
                            throw;

                        // 间隔一段时间+随机扰乱
                        var jitter = RandomUtil.Random.Next(0, 100);
                        var backoff = (int)Math.Pow(2, retryCount) * backoffMs;
                        await Task.Delay(backoff + jitter);

                        retryCount++;
                        backoffMs *= 2;
                    }
                }
            }
        }
    }
}
