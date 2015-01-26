using System;
using Newtonsoft.Json;

namespace CommonInfrastructureLibrary.Serialization.JsonNet
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
            var value = reader.ReadAsString();

            if (objectType == typeof(Guid))
            {
                return Guid.Parse(value);
            }

            if (!string.IsNullOrEmpty(value))
            {
                return (Guid?)Guid.Parse(value);
            }
            else
            {
                return null;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Guid) == objectType || typeof(Guid?) == objectType;
        }
    }
}