using System;
using Newtonsoft.Json.Linq;

namespace CommonInfrastructureLibrary.Serialization
{
    public interface IContractVersionMigrator
    {
        int FromVersion { get; }
        int ToVersion { get; }
        Type Type { get; }
        JObject Migrate(JObject obj);
    }
}