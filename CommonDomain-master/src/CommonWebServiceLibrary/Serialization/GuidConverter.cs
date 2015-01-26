using System;
using Newtonsoft.Json;

namespace CommonWebServiceLibrary.Serialization
{
    public class GuidConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Guid)
            {
                writer.WriteValue(((Guid)value).ToString("n"));
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof (Guid) && reader.TokenType == JsonToken.String)
            {
                return Guid.Parse(reader.Value.ToString());
            }

            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof (Guid) == objectType || typeof(Guid?) == objectType;
        }
    }
}