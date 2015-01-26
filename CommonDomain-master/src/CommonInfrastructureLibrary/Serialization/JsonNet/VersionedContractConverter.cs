using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonDomainLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommonInfrastructureLibrary.Serialization.JsonNet
{
    public class VersionedContractConverter : JsonConverter
    {
        private readonly IDictionary<Type, IOrderedEnumerable<IContractVersionMigrator>> _migrators;
        private readonly Dictionary<Type, ContractVersion> _versions;
        private readonly Serializer _serializer;

        public VersionedContractConverter(IList<IContractVersionMigrator> migrators)
        {
            _migrators = migrators.GroupBy(m => m.Type).ToDictionary(m => m.Key, m => m.OrderBy(o => o.FromVersion));
            _versions = migrators.GroupBy(m => m.Type).ToDictionary(m => m.Key, m => (ContractVersion)m.Key.GetCustomAttributes(typeof(ContractVersion), false).FirstOrDefault() ?? new ContractVersion(1));
            _serializer = new Serializer();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(value, stream);
                stream.Seek(0, SeekOrigin.Begin);

                var json = JObject.Load(new JsonTextReader(new StreamReader(stream)));
                var version = _versions[value.GetType()];

                json["@version"] = new JValue(version.Version);
                json.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = JObject.Load(reader);
            var version = Math.Max(json.Value<int>("@version"), 1);
            var originalVersion = version;
            var targetVersion = _versions[objectType];

            foreach (var migrator in _migrators[objectType])
            {
                if (migrator.FromVersion == version)
                {
                    version = migrator.ToVersion;
                    json = migrator.Migrate(json);
                }
            }

            if (targetVersion.Version != version)
            {
                throw new InvalidOperationException(
                    string.Format("Could not migrate {0} version {1} to version {2} because of limited migrators.",
                        objectType.Name, originalVersion, targetVersion.Version));
            }

            using (var stream = new MemoryStream())
            {
                using (var writer = new JsonTextWriter(new StreamWriter(stream)))
                {
                    json.WriteTo(writer);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);

                    return _serializer.Deserialize(stream, objectType);
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return _migrators.ContainsKey(objectType);
        }
    }
}