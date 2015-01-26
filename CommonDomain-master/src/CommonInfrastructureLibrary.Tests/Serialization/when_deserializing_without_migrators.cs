using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommonDomainLibrary;
using CommonInfrastructureLibrary.Serialization;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using Machine.Specifications;
using Newtonsoft.Json.Linq;

namespace CommonInfrastructureLibrary.Tests.Serialization
{
    public class when_deserializing_without_migrators
    {
        private static IList<IContractVersionMigrator> _migrators;
        private static JObject _json;
        private static Serializer _serializer;
        private static Stream _stream;
        private static MockClass _mockClassObj;

        public class MockClass
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
        }

        private Establish context = () =>
        {
            _json = new JObject(
                new JProperty("property1", "value1"),
                new JProperty("property2", "value2"),
                new JProperty("property3", "value3"),
                new JProperty("@version", 1)
                );

            _migrators = new List<IContractVersionMigrator>();
            _serializer = new Serializer(_migrators);
            _stream = new MemoryStream(Encoding.UTF8.GetBytes(_json.ToString()));
        };

        private Because of = () =>
        {
            _mockClassObj = (MockClass)_serializer.Deserialize(_stream, typeof(MockClass));
        };

        private It should_contain_property1 = () => _mockClassObj.Property1.ShouldEqual("value1");

        private It should_contain_property2 = () => _mockClassObj.Property2.ShouldEqual("value2");

        private It should_contain_property3 = () => _mockClassObj.Property3.ShouldEqual("value3"); 
    }
}