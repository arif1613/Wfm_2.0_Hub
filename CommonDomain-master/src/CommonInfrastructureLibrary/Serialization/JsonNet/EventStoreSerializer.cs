using System;
using System.IO;

namespace CommonInfrastructureLibrary.Serialization.JsonNet
{
    public class EventStoreSerializer : Edit.ISerializer
    {
        private readonly ISerializer _serializer;

        public EventStoreSerializer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public void Serialize<T>(T instance, Stream target) where T : class
        {
            _serializer.Serialize(instance, target);
        }

        public T Deserialize<T>(Stream source)
        {
            return (T) _serializer.Deserialize(source, typeof (T));
        }

        public object Deserialize(Type type, Stream source)
        {
            return _serializer.Deserialize(source, type);
        }
    }
}