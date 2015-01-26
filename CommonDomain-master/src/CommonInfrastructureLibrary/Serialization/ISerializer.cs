using System;
using System.IO;

namespace CommonInfrastructureLibrary.Serialization
{
    public interface ISerializer
    {
        object Deserialize(Stream stream, Type objectType);
        void Serialize(object o, Stream stream);
    }
}