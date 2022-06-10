using FastMember;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace HttpClientEx
{
    public class FormString : Collection<KeyValuePair<string, string>>
    {
        public FormString()
        { }

        public FormString(Dictionary<string, object> dict)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            foreach (var key in dict.Keys)
                Add(new KeyValuePair<string, string>(key, dict[key].ToString()));
        }

        public FormString(object obj)
        {
            var acc = ObjectAccessor.Create(obj);
            foreach (var member in acc.TypeAccessor.GetMembers())
                Add(new KeyValuePair<string, string>(member.Name, acc[member.Name].ToString()));
        }

        public FormString Append(string key, string value)
        {
            Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public override string ToString()
        {
            var keys = this.Select(kv =>
            {
                var key = WebUtility.UrlEncode(kv.Key);
                var value = WebUtility.UrlEncode(kv.Value);
                return $"{key}={value}";
            }).ToArray();

            return string.Join("&", keys);
        }
    }
}
