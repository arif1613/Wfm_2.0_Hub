using System;
using System.Collections.Generic;
using CommonTestingFramework;
using Machine.Specifications;
using NLog;
using NodaTime;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_publishing_a_deferred_message_with_multiple_consumers
    {
        internal class TestConsumer2 : TestConsumer<BeCool>
        { }

        private static InMemoryBus _bus;
        private static TestConsumer<BeCool> _consumer;
        private static TestConsumer2 _consumer2;
        private static Guid _correlationId;

        private Because of = () =>
        {
            _consumer = new TestConsumer<BeCool>();
            _consumer2 = new TestConsumer2();

            _bus = new InMemoryBus(new TestHandlerResolver(new Dictionary<Type, object>()
                {
                    {_consumer.GetType(), _consumer},
                    {_consumer2.GetType(), _consumer2}
                }));
            _bus.Subscribe(typeof(BeCool), _consumer.GetType()).Await();
            _bus.Subscribe(typeof(BeCool), _consumer2.GetType()).Await();

            var msg = new BeCool(Guid.NewGuid(), Guid.NewGuid());
            _correlationId = msg.CorrelationId;

            var task = _consumer.WaitForMessage(Duration.FromSeconds(6), Duration.FromSeconds(26), _correlationId);
            var task2 = _consumer2.WaitForMessage(Duration.FromSeconds(6), Duration.FromSeconds(26), _correlationId);

            _bus.Defer(msg,
                       Instant.FromDateTimeUtc(DateTime.UtcNow.AddSeconds(6))).Await();

            task.Await();
            task2.Await();
        };

        private It the_test_message_should_not_be_delivered_to_the_consumers_before_the_timeout_passes =
            () => true.ShouldBeTrue();

        private Cleanup clean = () => LogManager.Flush();
    }
}
