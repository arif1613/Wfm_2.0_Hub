using System;
using System.Collections.Generic;
using CommonDomainLibrary;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using CommonTestingFramework;
using Machine.Specifications;
using Microsoft.ServiceBus;
using NLog;
using NodaTime;

namespace Bus.Tests.BusTests
{
    public class when_subscribing_a_consumer_to_a_family_of_messages
    {
        private static Bus _bus;
        private static BeCool _message1;
        private static BeCool2 _message2;
        private static TestConsumer<IMessage> _archiveConsumer;
        private static IMessage _received1;
        private static IMessage _received2;

        private Establish context = () =>
        {
            _archiveConsumer = new TestConsumer<IMessage>();
            _bus = new Bus(AssemblyContext.ServiceBusConnectionString, new TestHandlerResolver(new Dictionary<Type, object>()
                {
                    {_archiveConsumer.GetType(), _archiveConsumer}
                }), new BusSerializer(new Serializer()));
            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
            if (manager.TopicExists(typeof(BeCool).ToString()))
            {
                manager.DeleteTopic(typeof(BeCool).ToString());
            }
            if (manager.TopicExists(typeof(BeCool2).ToString()))
            {
                manager.DeleteTopic(typeof(BeCool2).ToString());
            }

            _bus.Subscribe(typeof (IMessage), typeof (TestConsumer<IMessage>)).Await();
        };

        private Because of = () =>
        {
            _message1 = new BeCool(Guid.NewGuid(), Guid.NewGuid());
            _message2 = new BeCool2(Guid.NewGuid(), Guid.NewGuid());

            var task1 = _archiveConsumer.WaitForMessage(Duration.FromSeconds(10), _message1.CorrelationId);
            var task2 = _archiveConsumer.WaitForMessage(Duration.FromSeconds(10), _message2.CorrelationId);
            _bus.Publish(_message1).Await();
            _bus.Publish(_message2).Await();
            _received1 = task1.Result;
            _received2 = task2.Result;
        };

        private It all_the_messages_that_inherit_from_the_original_should_be_delivered_to_the_consumer = () =>
            {
                _received1.ShouldNotBeNull();
                _received2.ShouldNotBeNull();
            };

        private It the_received_messages_should_be_identical_to_the_sent_messages = () =>
            {
                if (_received1 is BeCool) (_received1 as BeCool).CompareTo(_message1).ShouldEqual(0);
                else (_received1 as BeCool2).CompareTo(_message2).ShouldEqual(0);

                if (_received2 is BeCool) (_received2 as BeCool).CompareTo(_message1).ShouldEqual(0);
                else (_received2 as BeCool2).CompareTo(_message2).ShouldEqual(0);
            };

        private Cleanup cleanup = () =>
        {
            LogManager.Flush();
            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
            if (manager.TopicExists(typeof(BeCool).ToString()))
            {
                manager.DeleteTopic(typeof(BeCool).ToString());
            }
            if (manager.TopicExists(typeof(BeCool2).ToString()))
            {
                manager.DeleteTopic(typeof(BeCool2).ToString());
            }
        };        
    }
}
