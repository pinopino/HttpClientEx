# HttpClientEx
这个小项目解决如下两个问题：
- httpclient的生命周期管理，以及附带的DNS状态刷新问题
- 提供一些扩展方法，方便上层调用

第一个问题参考[这里](https://gist.github.com/odyth/3a5d3d72cc98f280f213005ec9a08de9)解决，感谢作者。

扩展方法设计上浏览了好些其它类似项目（感谢作者的辛苦劳动）：
- [FastHttpRequest](https://github.com/vla/FastHttpRequest)
- [AgileHttp](https://github.com/kklldog/AgileHttp)
- [HttpClient.Extensions](https://github.com/olivierl/HttpClient.Extensions)

重点参考的是第三个项目，在其基础之上提供了更多的扩展方法；

### 使用
通过`HttpClientManager`正确的拿到`HttpClient`对象之后借助扩展方法愉快的编码即可。

### 示例
#### 实例化
```csharp
// 使用DI
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<HttpClientManager>();
}

// 或者普通实例化
private static HttpClientManager httpClientManager = new HttpClientManager();
```

#### 使用不同的HttpMessageHandler
默认情况下HttpClientManager内部使用一个默认的HttpMessageHandler即`HttpClientHandler`。所有在此默认情况下创建的`HttpClient`会使用同一套设置（诸如超时时间等）。

如果你的不同场景有不用的设置需求，那么你可以注册命名HttpMessageHandler：
```csharp
var handler = new MyHandler { InnerHandler = new HttpClientHandler() };
var httpClientManager = new HttpClientManager().AddHttpHandler("bing", handler);
```

#### 超时
在两个粒度上提供了不同的超时设置：
- HttpClient可以设置超时（注意，HttpClientManager本身可以设置一个超时。但该值只会影响到上面提到的那个默认HttpClient）：
  ```csharp
  public HttpClient CreateClient(string name, TimeSpan timeout, int retry = 0);
  ```
  如上，通过指定timeout参数我们便能拿到一个配置了自定义超时的httpclient对象

- 单个请求可以设置超时。当调用具体的扩展方法进行http请求时，入参也可以指定一个timeout参数：
  ```csharp
  using (var client  = _httpClientManager.CreateClient())
  {
      client.GetStringAsync("http://www.baidu.com", timeout: TimeSpan.FromSeconds(100));
  }
  ```
  如上，虽然默认的httpclient会使用HttpClientManager中默认的超时设置，但因为GetStringAsync方法调用时指定了新的timeout，则请求的超时时间以该值为准

### 参考链接
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0
https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient
