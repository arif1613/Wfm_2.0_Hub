//using System;
//using System.Collections.Generic;
//using CommonTestingFramework;
//using Machine.Specifications;
//using Microsoft.ServiceBus;
//using NodaTime;

//namespace Bus.Tests.BusTests
//{
//    public class when_publishing_a_deferred_message_with_a_consumer
//    {
//        private static Bus _bus;
//        private static TestConsumer<BeCool> _consumer;
//        private static Guid _correlationId;
//        private static Exception _exception;

//        private Because of = () =>
//        {
//            _consumer = new TestConsumer<BeCool>();

//            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
//            if (manager.TopicExists(typeof(BeCool).ToString()))
//            {
//                manager.DeleteTopic(typeof(BeCool).ToString());
//            }

//            _bus = new Bus(AssemblyContext.ServiceBusConnectionString, new TestHandlerResolver(new Dictionary<Type, object>()
//                {
//                    {_consumer.GetType(), _consumer}
//                }), new BusSerializer(new Serializer()));
//            _bus.Subscribe(typeof(BeCool), _consumer.GetType()).Await();

//            var msg = new BeCool(Guid.NewGuid(), Guid.NewGuid());
//            _correlationId = msg.CorrelationId;

//            var task = _consumer.WaitForMessage(Duration.FromSeconds(20), Duration.FromSeconds(50), _correlationId);

//            _bus.Defer(msg,
//                       Instant.FromDateTimeUtc(DateTime.UtcNow.AddSeconds(20))).Await();

//            _exception = Catch.Exception(() => task.Await());
//        };

//        private It the_test_message_should_not_be_delivered_to_the_consumer_before_the_timeout_passes =
//            () => _exception.ShouldBeNull();

//        private Cleanup cleanup = () =>
//        {
//            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
//            if (manager.TopicExists(typeof(BeCool).ToString()))
//            {
//                manager.DeleteTopic(typeof(BeCool).ToString());
//            }
//        };
//    }
//}
