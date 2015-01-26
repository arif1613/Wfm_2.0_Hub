using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NodaTime.TimeZones;
using System;
using System.IO;

namespace CommonInfrastructureLibrary.Serialization.JsonNet
{
    public class Serializer : ISerializer
    {
        private readonly JsonSerializer _serializer;

        public Serializer(IEnumerable<IContractVersionMigrator> migrators = null)
        {
            var migratorsList = migrators != null
                ? migrators.ToList()
                : new List<IContractVersionMigrator>();
            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                TypeNameHandling = TypeNameHandling.Objects
            };
            settings.ConfigureForNodaTime(new DateTimeZoneCache(new BclDateTimeZoneSource()));
            
            if (migratorsList.Count > 0)
            {
                settings.Converters.Add(new VersionedContractConverter(migratorsList));
            }

            _serializer = JsonSerializer.Create(settings);
        }

        public object Deserialize(Stream stream, Type objectType)
        {
            return _serializer.Deserialize(new StreamReader(stream), objectType);
        }

        public void Serialize(object o, Stream stream)
        {
            var writer = new StreamWriter(stream);
            _serializer.Serialize(writer, o);
            writer.Flush();
        }
    }
}