using System.Collections.Generic;
using System.Threading;
using CommonDomainLibrary.Security;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using CommonTestingFramework;
using Machine.Specifications;
using Microsoft.ServiceBus;
using System;
using NLog;

namespace Bus.Tests.BusTests
{
    public class when_publishing_a_message_with_an_identity
    {
        private static Bus _bus;
        private static TestConsumer<BeCool> _consumer;
        private static CommonIdentity _handledIdentity;
        private static CommonIdentity _publishedIdentity;

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

            _publishedIdentity = new CommonIdentity(Guid.NewGuid(), "test", "test", Guid.NewGuid());
            _bus.Subscribe(typeof(BeCool), typeof(TestConsumer<BeCool>)).Await();
        };

        private Because of = () =>
            {
                var message = new BeCool(Guid.NewGuid(), Guid.NewGuid());

                var task = _consumer.WaitForMessage(message.CorrelationId, m => _handledIdentity = Thread.CurrentPrincipal.Identity as CommonIdentity);
                _bus.Publish(message, _publishedIdentity).Await();
                task.Await();
            };

        private It the_identity_should_be_impersonated = () =>
            {
                _handledIdentity.ShouldNotBeNull();
                _handledIdentity.Id.ShouldEqual(_publishedIdentity.Id);
                _handledIdentity.Name.ShouldEqual(_publishedIdentity.Name);
                _handledIdentity.OwnerId.ShouldEqual(_publishedIdentity.OwnerId);
                _handledIdentity.AuthenticationType.ShouldEqual(_publishedIdentity.AuthenticationType);
            };

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
