using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommonDomainLibrary;
using CommonInfrastructureLibrary.Serialization;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using Machine.Specifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime.TimeZones;

namespace CommonInfrastructureLibrary.Tests.Serialization
{
    public class when_migrating_from_version_1_to_version_3_without_intermediate_migrator
    {
        private static IList<IContractVersionMigrator> _migrators;
        private static JObject _json;
        private static Serializer _serializer;
        private static Stream _stream;
        private static Exception _ex;

        [ContractVersion(3)]
        public class MockClass
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
        }

        private class Migrator : IContractVersionMigrator
        {
            public int FromVersion{ get { return 1; } }
            public int ToVersion { get { return 2; } }
            public Type Type { get { return typeof (MockClass); }  }
            
            public JObject Migrate(JObject obj)
            {
                var json = new JObject(obj);
                json.Add(new JProperty("property3", "value3"));
                return json;
            }
        }   
 
        private Establish context = () =>
        {
            _json = new JObject(
                new JProperty("property1", "value1"), 
                new JProperty("property2", "value2"), 
                new JProperty("@version", 1)
                );

            _migrators = new List<IContractVersionMigrator>{new Migrator()};
            _serializer = new Serializer(_migrators);
            _stream = new MemoryStream(Encoding.UTF8.GetBytes(_json.ToString()));
        };

        private Because of = () =>
        {    
            _ex = Catch.Exception(() => _serializer.Deserialize(_stream, typeof(MockClass)));
        };

        private It should_throw_invalid_operation_exception = () => _ex.ShouldBeOfExactType<InvalidOperationException>();
    }
}