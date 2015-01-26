using System;
using System.IO;

namespace Bus
{
    public interface ISerializer
    {
        object Deserialize(Type type, Stream stream);
        void Serialize(object obj, Stream stream);

    }
}