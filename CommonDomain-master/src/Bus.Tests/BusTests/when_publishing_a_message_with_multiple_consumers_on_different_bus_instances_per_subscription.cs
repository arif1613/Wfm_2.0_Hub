using System;
using System.Collections.Generic;
using CommonDomainLibrary.Common;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using CommonTestingFramework;
using Machine.Specifications;
using Microsoft.ServiceBus;
using NLog;
using NodaTime;

namespace Bus.Tests.BusTests
{
    public class when_publishing_a_message_with_multiple_consumers_on_different_bus_instances_per_subscription
    {
        public static int _count;
        private static Bus _bus;
        private static TestConsumer<BeCool> _consumer;
        private static TestConsumer<BeCool> _consumer2;
        private static Exception _ex1;
        private static Exception _ex2;
        private static Bus _bus2;
        private static IHandlerResolver _handlerResolver1;
        private static IHandlerResolver _handlerResolver2;

        private Establish context = () =>
            {
                _count = 0;
                _consumer = new TestConsumer<BeCool>();
                _consumer2 = new TestConsumer<BeCool>();
                _handlerResolver1 = new TestHandlerResolver(new Dictionary<Type, object>()
                    {
                        {_consumer.GetType(), _consumer},
                    });
                _handlerResolver2 = new TestHandlerResolver(new Dictionary<Type, object>()
                    {
                        {_consumer2.GetType(), _consumer2}
                    });

                var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
                if (manager.TopicExists(typeof(BeCool).ToString()))
                {
                    manager.DeleteTopic(typeof(BeCool).ToString());
                }

                _bus = new Bus(AssemblyContext.ServiceBusConnectionString, _handlerResolver1, new BusSerializer(new Serializer()));
                _bus2 = new Bus(AssemblyContext.ServiceBusConnectionString, _handlerResolver2, new BusSerializer(new Serializer()));
                _bus.Subscribe(typeof(BeCool), _consumer.GetType()).Await();
                _bus2.Subscribe(typeof(BeCool), _consumer.GetType()).Await();
            };

        private Because of = () =>
            {
                var msg = new BeCool(Guid.NewGuid(), Guid.NewGuid());
                var task1 = _consumer.WaitForMessage(Duration.FromSeconds(20), msg.CorrelationId);
                var task2 = _consumer2.WaitForMessage(Duration.FromSeconds(20), msg.CorrelationId);
                _bus.Publish(msg).Await();
                _ex1 = Catch.Exception(() => task1.Await());
                _ex2 = Catch.Exception(() => task2.Await());
            };

        private It the_message_should_be_delivered_to_one_of_the_consumer_instances = () => (_ex1 == null || _ex2 == null).ShouldBeTrue();
        private It the_message_should_not_be_delivered_to_the_other_consumer_instance = () => (_ex1 != null || _ex2 != null).ShouldBeTrue();

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
