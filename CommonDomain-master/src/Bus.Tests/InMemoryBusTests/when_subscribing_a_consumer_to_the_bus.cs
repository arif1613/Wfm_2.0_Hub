using System;
using System.Collections.Generic;
using CommonTestingFramework;
using Machine.Specifications;
using NLog;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_subscribing_a_consumer_to_the_bus
    {
        private static InMemoryBus _bus;

        private Establish context = () =>
            {
                _bus = new InMemoryBus(new TestHandlerResolver(new Dictionary<Type, object>()));
            };

        private Because of = () => _bus.Subscribe(typeof(BeCool), typeof (TestConsumer<BeCool>)).Await();

        private It no_exceptions_should_be_thrown = () => true.ShouldBeTrue();

        private Cleanup clean = () => LogManager.Flush();
    }
}
