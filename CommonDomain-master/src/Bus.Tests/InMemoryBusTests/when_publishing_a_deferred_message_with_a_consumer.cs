using System;
using System.Collections.Generic;
using CommonTestingFramework;
using Machine.Specifications;
using NLog;
using NodaTime;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_publishing_a_deferred_message_with_a_consumer
    {
        private static InMemoryBus _bus;
        private static TestConsumer<BeCool> _consumer;
        private static Guid _correlationId;

        private Because of = () =>
        {
            _consumer = new TestConsumer<BeCool>();

            _bus = new InMemoryBus(new TestHandlerResolver(new Dictionary<Type, object>()
                {
                    {_consumer.GetType(), _consumer}
                }));
            _bus.Subscribe(typeof(BeCool), _consumer.GetType()).Await();

            var msg = new BeCool(Guid.NewGuid(), Guid.NewGuid());
            _correlationId = msg.CorrelationId;

            var task = _consumer.WaitForMessage(Duration.FromSeconds(6), Duration.FromSeconds(26), _correlationId);

            _bus.Defer(msg,
                       Instant.FromDateTimeUtc(DateTime.UtcNow.AddSeconds(6))).Await();

            task.Await();
        };

        private It the_test_message_should_not_be_delivered_to_the_consumer_before_the_timeout_passes =
            () => true.ShouldBeTrue();

        private Cleanup clean = () => LogManager.Flush();
    }
}
