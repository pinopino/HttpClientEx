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

### 参考链接
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0
https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient
