using HttpClientEx.Constants;
using HttpClientEx.Serialization;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientEx
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T> ReadJsonAsync<T>(this HttpResponseMessage httpResponseMessage)
            where T : class
        {
            var content = httpResponseMessage.Content;
            if (content == null)
                return null;

            var contentType = content.Headers.ContentType.MediaType;
            if (!contentType.Contains(ContentTypes.Json))
                throw new HttpRequestException($"Content type \"{contentType}\" not supported");

            var json = await httpResponseMessage.Content.ReadAsStringAsync();
            var jsonSerializer = new NewtonsoftSerializer();

            return jsonSerializer.Deserialize<T>(json);
        }
    }
}
