using System;
using System.Collections.Generic;
using CommonDomainLibrary.Common;
using CommonTestingFramework;
using Machine.Specifications;
using NLog;
using NodaTime;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_publishing_a_message_with_multiple_consumers_on_different_bus_instances_per_subscription
    {
        private static InMemoryBus _bus;
        private static TestConsumer<BeCool> _consumer;
        private static TestConsumer<BeCool> _consumer2;
        private static Exception _ex1;
        private static Exception _ex2;
        private static InMemoryBus _bus2;
        private static IHandlerResolver _handlerResolver1;
        private static IHandlerResolver _handlerResolver2;

        private Establish context = () =>
            {
                _consumer = new TestConsumer<BeCool>();
                _consumer2 = new TestConsumer<BeCool>();
                _handlerResolver1 = new TestHandlerResolver(new Dictionary<Type, object>(){
                    {_consumer.GetType(), _consumer},
                });
                _handlerResolver2 = new TestHandlerResolver(new Dictionary<Type, object>(){
                    {_consumer2.GetType(), _consumer2}
                });

                _bus = new InMemoryBus(_handlerResolver1);
                _bus2 = new InMemoryBus(_handlerResolver2);
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

        private Cleanup clean = () => LogManager.Flush();
    }
}
