using System;
using System.Collections.Generic;
using CommonDomainLibrary;
using CommonTestingFramework;
using Machine.Specifications;
using NLog;
using NodaTime;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_subscribing_a_consumer_to_a_family_of_messages
    {
        private static InMemoryBus _bus;
        private static IMessage _received1;
        private static TestConsumer<IMessage> _archiveConsumer;
        private static IMessage _received2;
        private static BeCool _message;
        private static BeCool2 _message2;

        private Establish context = () =>
        {
            _archiveConsumer = new TestConsumer<IMessage>();
            _bus = new InMemoryBus(new TestHandlerResolver(new Dictionary<Type, object>()
                {
                    {_archiveConsumer.GetType(), _archiveConsumer}
                }));

            _bus.Subscribe(typeof (IMessage), typeof (TestConsumer<IMessage>)).Await();
        };

        private Because of = () =>
        {
            _message = new BeCool(Guid.NewGuid(), Guid.NewGuid());
            _message2 = new BeCool2(Guid.NewGuid(), Guid.NewGuid());

            var task1 = _archiveConsumer.WaitForMessage(Duration.FromSeconds(10), _message.CorrelationId);
            var task2 = _archiveConsumer.WaitForMessage(Duration.FromSeconds(10), _message2.CorrelationId);
            _bus.Publish(_message).Await();
            _bus.Publish(_message2).Await();
            _received1 = task1.Result;
            _received2 = task2.Result;
        };

        private It the_message_should_be_delivered_to_both_consumers = () =>
            {
                _received1.ShouldNotBeNull();
                _received2.ShouldNotBeNull();
            };

        private It the_two_messages_should_be_identical = () =>
            {
                if (_received1 is BeCool) (_received1 as BeCool).CompareTo(_message).ShouldEqual(0);
                else (_received1 as BeCool2).CompareTo(_message2).ShouldEqual(0);

                if (_received2 is BeCool) (_received2 as BeCool).CompareTo(_message).ShouldEqual(0);
                else (_received2 as BeCool2).CompareTo(_message2).ShouldEqual(0);
            };

        private Cleanup clean = () => LogManager.Flush();
    }
}
