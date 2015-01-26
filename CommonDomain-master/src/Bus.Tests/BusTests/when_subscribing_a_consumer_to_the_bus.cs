using System;
using System.Collections.Generic;
using CommonDomainLibrary.Common;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using CommonTestingFramework;
using Machine.Specifications;
using Microsoft.ServiceBus;
using NLog;

namespace Bus.Tests.BusTests
{
    public class when_subscribing_a_consumer_to_the_bus
    {
        private static Bus _bus;
        private static Exception _exception;
        private static readonly IHandlerResolver HandlerResolver = new TestHandlerResolver(new Dictionary<Type, object>()
            {
                {typeof(TestConsumer<BeCool>), new TestConsumer<BeCool>()}
            });

        private Establish context = () =>
            {
                _bus = new Bus(AssemblyContext.ServiceBusConnectionString, HandlerResolver, new BusSerializer(new Serializer()));
                var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
                if (manager.TopicExists(typeof(BeCool).ToString()))
                {
                    manager.DeleteTopic(typeof(BeCool).ToString());
                }
            };

        private Because of = () => _exception = Catch.Exception(() => _bus.Subscribe(typeof(BeCool), typeof(TestConsumer<BeCool>)).Await());

        private It no_exceptions_should_be_thrown = () => _exception.ShouldBeNull();

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
