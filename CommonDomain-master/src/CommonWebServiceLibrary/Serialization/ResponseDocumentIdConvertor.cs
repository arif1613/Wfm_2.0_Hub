using System;
using System.Text.RegularExpressions;
using CommonReadModelLibrary;
using Newtonsoft.Json;

namespace CommonWebServiceLibrary.Serialization
{
    public class ResponseDocumentIdConvertor : JsonConverter
    {
        private static readonly Regex _regex = new Regex(@"([a-z]){2}_([a-z0-9]){32}", RegexOptions.Compiled);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var s = value as string;
            if (s!=null)
            {
                if(_regex.IsMatch(s)) writer.WriteValue(s.AsGuid().ToString("n"));
                else writer.WriteValue(s);
            }
            else writer.WriteNull();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof (string)) return true;
            return false;
        }
    }
}
