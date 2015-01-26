using System.Collections.Generic;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using CommonTestingFramework;
using Machine.Specifications;
using Microsoft.ServiceBus;
using System;
using NLog;
using NodaTime;

namespace Bus.Tests.BusTests
{
    public class when_publishing_a_message_with_a_consumer
    {
        private static Bus _bus;
        private static TestConsumer<BeCool> _consumer;
        private static BeCool _message;
        private static BeCool _received;

        private Establish context = () =>
        {
            _consumer = new TestConsumer<BeCool>();
            _bus = new Bus(AssemblyContext.ServiceBusConnectionString, new TestHandlerResolver(new Dictionary<Type, object>()
                {
                    {_consumer.GetType(), _consumer}
                }), new BusSerializer(new Serializer()));
            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
            if (manager.TopicExists(typeof(BeCool).ToString()))
            {
                manager.DeleteTopic(typeof(BeCool).ToString());
            }

            _bus.Subscribe(typeof(BeCool), typeof (TestConsumer<BeCool>)).Await();
        };

        private Because of = () =>
            {
                _message = new BeCool(Guid.NewGuid(), Guid.NewGuid());

                var task = _consumer.WaitForMessage(Duration.FromSeconds(20), _message.CorrelationId);
                _bus.Publish(_message).Await();
                _received = task.Await();
            };

        private It the_message_should_be_delivered = () => _received.ShouldNotBeNull();
        private It the_received_message_should_be_the_same_as_the_sent_one = () => _received.CompareTo(_message).ShouldEqual(0);

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
