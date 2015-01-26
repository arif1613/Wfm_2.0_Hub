using Machine.Specifications;
using System;
using NLog;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_trying_to_subscribe_the_wrong_type_of_handler
    {
        private static InMemoryBus _bus;
        private static Exception _ex;

        private Establish context = () =>
        {
            _bus = new InMemoryBus(new TestHandlerResolver(null));
        };

        private Because of = () => _ex = Catch.Exception(() => _bus.Subscribe(typeof(BeCool), typeof(string)).Wait());

        private It an_Exception_should_be_thrown = () => _ex.ShouldNotBeNull();

        private Cleanup clean = () => LogManager.Flush();
    }
}
