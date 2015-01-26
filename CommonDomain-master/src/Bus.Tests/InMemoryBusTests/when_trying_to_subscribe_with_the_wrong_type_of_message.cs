using System;
using System.Collections.Generic;
using CommonTestingFramework;
using Machine.Specifications;
using NLog;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_trying_to_subscribe_with_the_wrong_type_of_message
    {
        private static InMemoryBus _bus;
        private static Exception _ex;

        private Establish context = () =>
            {
                _bus = new InMemoryBus(new TestHandlerResolver(new Dictionary<Type, object>()));
            };

        private Because of = () => _ex = Catch.Exception(() => _bus.Subscribe(typeof(string), typeof (TestConsumer<BeCool>)).Wait());

        private It an_Exception_should_be_thrown = () => _ex.ShouldNotBeNull();

        private Cleanup clean = () => LogManager.Flush();
    }
}
