using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using CommonTestingFramework;
using Machine.Specifications;
using Microsoft.ServiceBus;
using NLog;
using NodaTime;

namespace Bus.Tests.BusTests
{
    public class when_publishing_a_deferred_message_with_multiple_consumers
    {
        internal class TestConsumer2 : TestConsumer<BeCool>
        {}

        private static Bus _bus;
        private static TestConsumer<BeCool> _consumer;
        private static TestConsumer2 _consumer2; 
        private static Guid _correlationId;
        private static Exception _exception;

        private Because of = () =>
        {
            _consumer = new TestConsumer<BeCool>();
            _consumer2 = new TestConsumer2();

            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
            if (manager.TopicExists(typeof(BeCool).ToString()))
            {
                manager.DeleteTopic(typeof(BeCool).ToString());
            }

            _bus = new Bus(AssemblyContext.ServiceBusConnectionString, new TestHandlerResolver(new Dictionary<Type, object>()
                {
                    {_consumer.GetType(), _consumer},
                    {_consumer2.GetType(), _consumer2}
                }), new BusSerializer(new Serializer()));
            _bus.Subscribe(typeof(BeCool), _consumer.GetType()).Await();
            _bus.Subscribe(typeof(BeCool), _consumer2.GetType()).Await();

            var msg = new BeCool(Guid.NewGuid(), Guid.NewGuid());
            _correlationId = msg.CorrelationId;

            var task1 = _consumer.WaitForMessage(Duration.FromSeconds(10), Duration.FromSeconds(50), _correlationId);
            var task2 = _consumer2.WaitForMessage(Duration.FromSeconds(10), Duration.FromSeconds(50), _correlationId);

            _bus.Defer(msg,
                       Instant.FromDateTimeUtc(DateTime.UtcNow.AddSeconds(10))).Await();

            _exception = Catch.Exception(() => Task.WhenAll(task1, task2));
        };

        private It the_test_message_should_not_be_delivered_to_the_consumers_before_the_timeout_passes_and_both_consumers_should_get_it = () => _exception.ShouldBeNull();

        private Cleanup cleanup = () =>
        {
            LogManager.Flush();
            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
            if (manager.TopicExists(typeof(BeCool).ToString()))
            {
                manager.DeleteTopic(typeof(BeCool).ToString());
            }
        };        
    }
}
