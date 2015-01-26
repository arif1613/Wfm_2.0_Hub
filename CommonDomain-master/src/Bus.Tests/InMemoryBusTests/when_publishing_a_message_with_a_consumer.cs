using System;
using System.Collections.Generic;
using CommonTestingFramework;
using Machine.Specifications;
using NLog;
using NodaTime;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_publishing_a_message_with_a_consumer
    {
        private static InMemoryBus _bus;
        private static TestConsumer<BeCool> _consumer; 

        private Establish context = () =>
        {
            _consumer = new TestConsumer<BeCool>();

            _bus = new InMemoryBus(new TestHandlerResolver(new Dictionary<Type, object>()
                {
                    {_consumer.GetType(), _consumer}
                }));
            _bus.Subscribe(typeof(BeCool), typeof (TestConsumer<BeCool>)).Await();
        };

        private Because of = () =>
            {
                var message = new BeCool(Guid.NewGuid(), Guid.NewGuid());

                var task = _consumer.WaitForMessage(Duration.FromSeconds(20), message.CorrelationId);
                _bus.Publish(message).Await();
                task.Await();
            };

        private It the_message_should_be_delivered = () => true.ShouldBeTrue();

        private Cleanup clean = () => LogManager.Flush();
    }
}
