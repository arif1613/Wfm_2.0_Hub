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
    public class when_migrating_from_version_1_to_version_3
    {
        private static IList<IContractVersionMigrator> _migrators;
        private static JObject _json;
        private static Serializer _serializer;
        private static Stream _stream;
        private static MockClass _mockClassObj;

        [ContractVersion(3)]
        public class MockClass
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
        }

        private class Migrator1 : IContractVersionMigrator
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

        private class Migrator2 : IContractVersionMigrator
        {
            public int FromVersion { get { return 2; } }
            public int ToVersion { get { return 3; } }
            public Type Type { get { return typeof(MockClass); } }

            public JObject Migrate(JObject obj)
            {
                var json = new JObject(obj);
                json.Add(new JProperty("property4", "value4"));
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

            _migrators = new List<IContractVersionMigrator>{new Migrator1(), new Migrator2()};
            _serializer = new Serializer(_migrators);
            _stream = new MemoryStream(Encoding.UTF8.GetBytes(_json.ToString()));
        };

        private Because of = () =>
        {    
            _mockClassObj = (MockClass)_serializer.Deserialize(_stream, typeof(MockClass));
        };

        private It should_contain_version_1_property1 = () => _mockClassObj.Property1.ShouldEqual("value1");
        
        private It should_contain_version_1_property2 = () => _mockClassObj.Property2.ShouldEqual("value2");

        private It should_contain_version_2_property3 = () => _mockClassObj.Property3.ShouldEqual("value3");

        private It should_contain_version_3_property4 = () => _mockClassObj.Property4.ShouldEqual("value4");
    }
}