using System;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using CommonTestingFramework;
using Machine.Specifications;
using NLog;

namespace Bus.Tests.BusTests
{
    public class when_trying_to_subscribe_with_the_wrong_type_of_message
    {
        private static Bus _bus;
        private static Exception _ex;

        private Establish context = () =>
            {
                _bus = new Bus(AssemblyContext.ServiceBusConnectionString, new TestHandlerResolver(null), new BusSerializer(new Serializer()));
            };

        private Because of = () => _ex = Catch.Exception(() => _bus.Subscribe(typeof(string), typeof (TestConsumer<BeCool>)).Wait());

        private It an_Exception_should_be_thrown = () => _ex.ShouldNotBeNull();

        Cleanup clean = () => LogManager.Flush();
    }
}
