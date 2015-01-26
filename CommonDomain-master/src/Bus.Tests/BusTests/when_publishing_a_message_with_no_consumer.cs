using System;
using System.Collections.Generic;
using CommonDomainLibrary.Common;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using Machine.Specifications;
using NLog;

namespace Bus.Tests.BusTests
{
    public class when_publishing_a_message_with_no_consumer
    {
        private static readonly IHandlerResolver _handlerResolver = new TestHandlerResolver(new Dictionary<Type, object>());
        private static Bus _bus;
        private static Exception _exception;

        private Establish context = () =>
            {
                _bus = new Bus(AssemblyContext.ServiceBusConnectionString, _handlerResolver, new BusSerializer(new Serializer()));
            };

        private Because of = () => _exception = Catch.Exception(() => _bus.Publish(new BeCool(Guid.NewGuid(), Guid.NewGuid())).Await());

        private It no_exception_should_be_thrown = () => _exception.ShouldBeNull();

        Cleanup clean = () => LogManager.Flush();
    }
}
