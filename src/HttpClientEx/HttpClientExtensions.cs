using HttpClientEx.Common;
using HttpClientEx.Constants;
using HttpClientEx.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientEx
{
    public static class HttpClientExtensions
    {
        #region 属性设置
        /// <summary>
        /// 设置httpclient的默认requestheader
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="headers">自定义请求头</param>
        public static void SetDefaultRequestHeader(this HttpClient httpClient, IDictionary<string, string> headers = null)
        {
            foreach (var header in headers)
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        /// <summary>
        /// 设置httpclient的基本认证信息
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="username">用户信息</param>
        /// <param name="password">用户密码</param>
        public static void SetBasicAuthentication(this HttpClient httpClient, string username, string password)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        /// <summary>
        /// 设置httpclient的bearertoken
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="token">token</param>
        public static void SetBearerToken(this HttpClient httpClient, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        #endregion

        #region http get
        /// <summary>
        /// 发送一次http get请求，获取一个字符串
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="body">body参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>请求的字符串</returns>
        public static string GetString(this HttpClient httpClient, string url,
            QueryString queryString = null, object body = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Get, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);
            
            if (body != null)
                request.Content = CreateContent(body, contentType);

            using (var response = httpClient.SendAsync(request).Result)
            {
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        /// <summary>
        /// 异步发送一次http get请求，获取一个字符串
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="body">body参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>请求的字符串</returns>
        public static async Task<string> GetStringAsync(this HttpClient httpClient, string url,
            QueryString queryString = null, object body = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Get, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);

            if (body != null)
                request.Content = CreateContent(body, contentType);

            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 发送一次http get请求，获取一个byte数组
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="body">body参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>请求的byte数组</returns>
        public static byte[] GetBytes(this HttpClient httpClient, string url,
            QueryString queryString = null, object body = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Get, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);

            if (body != null)
                request.Content = CreateContent(body, contentType);

            using (var response = httpClient.SendAsync(request).Result)
            {
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsByteArrayAsync().Result;
            }
        }

        /// <summary>
        /// 异步发送一次http get请求，获取一个byte数组
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="body">body参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>请求的byte数组</returns>
        public static async Task<byte[]> GetBytesAsync(this HttpClient httpClient, string url,
            QueryString queryString = null, object body = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Get, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);

            if (body != null)
                request.Content = CreateContent(body, contentType);

            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        /// <summary>
        /// 异步发送一次http get请求
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="body">body参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>返回的HttpResponseMessage</returns>
        public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, string url,
            QueryString queryString = null, object body = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Get, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);

            if (body != null)
                request.Content = CreateContent(body, contentType);

            return httpClient.SendAsync(request);
        }
        #endregion

        #region http post
        /// <summary>
        /// 发送一次http post请求，获取一个字符串
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="body">body参数</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>请求的字符串</returns>
        public static string PostForString(this HttpClient httpClient, string url,
            object body, QueryString queryString = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Post, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);
            if (body != null)
                request.Content = CreateContent(body, contentType);

            using (var response = httpClient.SendAsync(request).Result)
            {
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        /// <summary>
        /// 异步发送一次http post请求，获取一个字符串
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="body">body参数</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>请求的字符串</returns>
        public static async Task<string> PostForStringAsync(this HttpClient httpClient, string url,
            object body, QueryString queryString = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Post, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);
            if (body != null)
                request.Content = CreateContent(body, contentType);

            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 发送一次http post请求，获取一个byte数组
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="body">body参数</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>请求的byte数组</returns>
        public static byte[] PostForBytes(this HttpClient httpClient, string url,
            object body, QueryString queryString = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Post, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);
            if (body != null)
                request.Content = CreateContent(body, contentType);

            using (var response = httpClient.SendAsync(request).Result)
            {
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsByteArrayAsync().Result;
            }
        }

        /// <summary>
        /// 异步发送一次http post请求，获取一个byte数组
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="body">body参数</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>请求的byte数组</returns>
        public static async Task<byte[]> PostForBytesAsync(this HttpClient httpClient, string url,
            object body, QueryString queryString = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Post, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);
            if (body != null)
                request.Content = CreateContent(body, contentType);

            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        /// <summary>
        /// 发送一次http post请求
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="body">body参数</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>返回的HttpResponseMessage</returns>
        public static Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, string url,
            object body, QueryString queryString = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Post, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);
            if (body != null)
                request.Content = CreateContent(body, contentType);

            return httpClient.SendAsync(request);
        }
        #endregion

        #region http patch
        /// <summary>
        /// 发送一次http patch请求
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="body">body参数</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>返回的HttpResponseMessage</returns>
        public static Task<HttpResponseMessage> PatchAsync(this HttpClient httpClient, string url,
            object body, QueryString queryString = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(new HttpMethod("PATCH"), url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);
            if (body != null)
                request.Content = CreateContent(body, contentType);

            return httpClient.SendAsync(request);
        }
        #endregion

        #region http put
        /// <summary>
        /// 发送一次http put请求
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="body">body参数</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>返回的HttpResponseMessage</returns>
        public static Task<HttpResponseMessage> PutAsync(this HttpClient httpClient, string url,
            object body, QueryString queryString = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Put, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);
            if (body != null)
                request.Content = CreateContent(body, contentType);

            return httpClient.SendAsync(request);
        }
        #endregion

        #region http delete
        /// <summary>
        /// 发送一次http delete请求
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="url">url地址</param>
        /// <param name="queryString">查询参数</param>
        /// <param name="body">body参数</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>返回的HttpResponseMessage</returns>
        public static Task<HttpResponseMessage> DeleteAsync(this HttpClient httpClient, string url,
            QueryString queryString = null, object body = null, IDictionary<string, string> headers = null, TimeSpan timeout = default, string contentType = ContentTypes.Json)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var request = httpClient.CreateRequest(HttpMethod.Delete, url, queryString, headers);
            if (timeout != TimeSpan.Zero)
                request.SetTimeout(timeout);

            if (body != null)
                request.Content = CreateContent(body, contentType);

            return httpClient.SendAsync(request);
        }
        #endregion

        #region 辅助方法
        private static string BuildFinalUrl(this HttpClient httpClient, string url, QueryString queryString = null)
        {
            var finalUrl = url;
            if (httpClient.BaseAddress != null)
                finalUrl = URLUtility.Combine(httpClient.BaseAddress.OriginalString, url);

            if (queryString == null)
                return finalUrl;

            finalUrl = URLUtility.AppendQueryString(finalUrl, queryString.ToString());

            return finalUrl;
        }

        private static HttpRequestMessage CreateRequest(this HttpClient httpClient, HttpMethod httpMethod, string url,
            QueryString queryString = null, IDictionary<string, string> headers = null)
        {
            var fullUrl = httpClient.BuildFinalUrl(url, queryString);
            var request = new HttpRequestMessage(httpMethod, fullUrl);
            if (headers == null)
                return request;

            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

            return request;
        }

        private static HttpContent CreateContent(object body, string contentType)
        {
            var content = string.Empty;
            if (body is string)
            {
                if (contentType == ContentTypes.Form)
                {
                    content = body.ToString();
                }
                else
                {
                    var rawStr = body.ToString();
                    try
                    {
                        JToken.Parse(rawStr);
                        content = rawStr;
                    }
                    catch
                    {
                        var jsonSerializer = new NewtonsoftSerializer();
                        content = jsonSerializer.Serialize(rawStr);
                    }
                    contentType = ContentTypes.Json;
                }
            }
            else if (body is FormString)
            {
                content = body.ToString();
                contentType = ContentTypes.Form;
            }
            else
            {
                var jsonSerializer = new NewtonsoftSerializer();
                content = jsonSerializer.Serialize(body);
                contentType = ContentTypes.Json;
            }

            return new StringContent(content, Encoding.UTF8, contentType);
        }
        #endregion
    }
}
