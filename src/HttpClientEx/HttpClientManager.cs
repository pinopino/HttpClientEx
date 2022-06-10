using System;
using System.Collections.Concurrent;
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
        private readonly static TimeSpan _defaultTimeout;
        private readonly string _defaultKey = "default";
        private static ConcurrentDictionary<string, HttpMessageHandler> _handlerDict = new ConcurrentDictionary<string, HttpMessageHandler>();

        static HttpClientManager()
        {
            //The number of connections per end point. default is 2-10 depending on environment.
            //Connections start to queue once this number is hit. The timeout clock starts
            //at send, so if you are stuck queuing your timeout clock is running, which leads to timeouts.
            ServicePointManager.DefaultConnectionLimit = 10;

            //This setting was put in place to save bandwidth before sending huge object
            //for post and put request to ensure remote end point is up and running.
            ServicePointManager.Expect100Continue = false;

            //Nagle's algorithm is a means of improving the efficiency of TCP/IP networks
            //by reducing the number of packets that need to be sent over the network.
            //This can decrease the overall transmission overhead but can cause delay in data packet arrival.
            ServicePointManager.UseNagleAlgorithm = false;

            _defaultTimeout = TimeSpan.FromSeconds(30);
        }

        public HttpClientManager()
        {
            AddDefaultHttpClient();
        }
        
        public HttpClientManager AddHttpHandler(string name, HttpMessageHandler httpMessageHandler)
        {
            if (name == _defaultKey)
                throw new ArgumentException($"`{_defaultKey}` reserve for the default HttpClientHandler key");

            _handlerDict.TryAdd(name, httpMessageHandler);

            return this;
        }

        /// <summary>
        /// 创建一个HttpClient对象
        /// </summary>
        public HttpClient CreateClient(int retry = 0)
        {
            return CreateClient(_defaultKey, _defaultTimeout, retry);
        }

        /// <summary>
        /// 创建一个HttpClient对象
        /// </summary>
        public HttpClient CreateClient(string name, int retry = 0)
        {
            return CreateClient(name, _defaultTimeout, retry);
        }

        /// <summary>
        /// 创建一个HttpClient对象
        /// </summary>
        public HttpClient CreateClient(string name, TimeSpan timeout, int retry = 0)
        {
            if (_handlerDict.TryGetValue(name, out HttpMessageHandler handler))
                return new HttpClient(new TimeoutHandler(handler, timeout, retry), disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };

            throw new Exception($"can not find corresponding HttpMessageHandler(name={name})");
        }

        private void AddDefaultHttpClient()
        {
            _handlerDict.TryAdd(_defaultKey, null);
        }

        private class TimeoutHandler : DelegatingHandler
        {
            private static readonly Lazy<HttpMessageHandler> SharedHandler = new Lazy<HttpMessageHandler>(() => new HttpClientHandler());
            private static readonly HashSet<string> Endpoints = new HashSet<string>();
            private static readonly TimeSpan ConnectionCloseTimeoutPeriod = TimeSpan.FromMinutes(2);

            private readonly int _retry;
            private readonly TimeSpan _timeout;

            public TimeoutHandler(HttpMessageHandler sharedHandler, TimeSpan timeout, int retry)
                : base(sharedHandler ?? SharedHandler.Value)
            {
                _timeout = timeout;
                _retry = retry;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // 通过设置Endpoint的LeaseTimeout防止DNS状态更新问题
                AddConnectionLeaseTimeout(request.RequestUri);

                return Execute.WithRetryAsync(async () =>
                {
                    using (var cts = GetCancellationTokenSource(request, cancellationToken))
                    {
                        try
                        {
                            return await base.SendAsync(request, cts?.Token ?? cancellationToken);
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

            private CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var timeout = request.GetTimeout() ?? _timeout;
                if (timeout == Timeout.InfiniteTimeSpan)
                {
                    // No need to create a CTS if there's no timeout
                    return null;
                }
                else
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(timeout);
                    return cts;
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
