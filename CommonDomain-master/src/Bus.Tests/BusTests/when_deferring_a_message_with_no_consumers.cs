using System;
using System.Collections.Generic;
using CommonDomainLibrary.Common;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using Machine.Specifications;
using NLog;
using NodaTime;

namespace Bus.Tests.BusTests
{
    public class when_deferring_a_message_with_no_consumers
    {
        private static readonly IHandlerResolver _handlerResolver = new TestHandlerResolver(new Dictionary<Type, object>());
        private static Bus _bus;

        private Establish context = () =>
        {
            _bus = new Bus(AssemblyContext.ServiceBusConnectionString, _handlerResolver, new BusSerializer(new Serializer()));
        };

        private Because of = () => _bus.Defer(new BeCool(Guid.NewGuid(), Guid.NewGuid()), Instant.FromDateTimeUtc(DateTime.UtcNow.AddMinutes(1))).Await();

        private It no_exception_should_be_thrown = () => true.ShouldBeTrue();

        Cleanup clean = () => LogManager.Flush();
    }
}
