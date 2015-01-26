using CommonInfrastructureLibrary.Serialization.JsonNet;
using Machine.Specifications;
using System;
using NLog;

namespace Bus.Tests.BusTests
{
    public class when_trying_to_instantiate_a_bus_with_a_null_resolver
    {
        private static object _bus;
        private static Exception _ex;

        Because of = () => _ex = Catch.Exception(() => _bus = new Bus(AssemblyContext.ServiceBusConnectionString, null, new BusSerializer(new Serializer())));

        private It an_exception_should_be_thrown = () => _ex.ShouldNotBeNull();

        Cleanup clean = () => LogManager.Flush();
    }
}
