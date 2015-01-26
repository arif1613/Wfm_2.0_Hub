using System;
using System.Collections.Generic;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using Machine.Specifications;
using NLog;

namespace Bus.Tests.BusTests
{
    public class when_trying_to_subscribe_the_wrong_type_of_handler
    {
        private static Bus _bus;
        private static Exception _ex;

        private Establish context = () =>
        {
            _bus = new Bus(AssemblyContext.ServiceBusConnectionString, new TestHandlerResolver(new Dictionary<Type, object>()), new BusSerializer(new Serializer()));
        };

        private Because of = () => _ex = Catch.Exception(() => _bus.Subscribe(typeof(BeCool), typeof(string)).Wait());

        private It an_Exception_should_be_thrown = () => _ex.ShouldNotBeNull();

        Cleanup clean = () => LogManager.Flush();
    }
}
