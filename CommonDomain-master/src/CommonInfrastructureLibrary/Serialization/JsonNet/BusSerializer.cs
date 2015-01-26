using System;
using System.IO;

namespace CommonInfrastructureLibrary.Serialization.JsonNet
{
    public class BusSerializer : Bus.ISerializer
    {
        private readonly ISerializer _serializer;

        public BusSerializer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public void Serialize(object instance, Stream target)
        {
            _serializer.Serialize(instance, target);
        }

        public object Deserialize(Type type, Stream source)
        {
            return _serializer.Deserialize(source, type);
        }
    }
}