using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Text;

namespace HttpClientEx.Serialization
{
    public class NewtonsoftSerializer : ISerializer
    {
        /// <summary>
        /// Encoding to use to convert string to byte[] and the other way around.
        /// </summary>
        /// <remarks>
        /// StackExchange.Redis uses Encoding.UTF8 to convert strings to bytes,
        /// hence we do same here.
        /// </remarks>
        private static readonly Encoding encoding = Encoding.UTF8;

        private readonly JsonSerializerSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftSerializer"/> class.
        /// </summary>
        public NewtonsoftSerializer() : this(null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftSerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public NewtonsoftSerializer(JsonSerializerSettings settings)
        {
            this.settings = settings ?? new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                DateFormatString = "yyyy-MM-dd HH:mm:ss.fff",
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public byte[] SerializeToBytes(object item)
        {
            var type = item?.GetType();
            var jsonString = JsonConvert.SerializeObject(item, type, settings);
            return encoding.GetBytes(jsonString);
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public string Serialize(object item)
        {
            var type = item?.GetType();
            var jsonString = JsonConvert.SerializeObject(item, type, settings);
            return jsonString;
        }

        /// <summary>
        /// Deserializes the specified serialized object.
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public object Deserialize(byte[] serializedBytes)
        {
            var jsonString = encoding.GetString(serializedBytes);
            return JsonConvert.DeserializeObject(jsonString, typeof(object));
        }

        /// <summary>
        /// Deserializes the specified serialized object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] serializedBytes)
        {
            var jsonString = encoding.GetString(serializedBytes);
            return JsonConvert.DeserializeObject<T>(jsonString, settings);
        }

        /// <summary>
        /// Deserializes the specified serialized object string.
        /// </summary>
        /// <param name="serializedStr">The serialized object string representation.</param>
        /// <returns></returns>
        public object Deserialize(string serializedStr)
        {
            return JsonConvert.DeserializeObject(serializedStr, typeof(object));
        }

        /// <summary>
        /// Deserializes the specified serialized object string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedStr">The serialized object string representation.</param>
        /// <returns></returns>
        public T Deserialize<T>(string serializedStr)
        {
            return JsonConvert.DeserializeObject<T>(serializedStr, settings);
        }
    }
}
