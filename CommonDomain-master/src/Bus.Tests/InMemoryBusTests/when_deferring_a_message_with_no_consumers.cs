using System;
using System.Collections.Generic;
using CommonDomainLibrary.Common;
using Machine.Specifications;
using NLog;
using NodaTime;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_deferring_a_message_with_no_consumers
    {
        private static readonly IHandlerResolver HandlerResolver = new TestHandlerResolver(new Dictionary<Type, object>());
        private static InMemoryBus _bus;

        private Establish context = () =>
        {
            _bus = new InMemoryBus(HandlerResolver);
        };

        private Because of = () => _bus.Defer(new BeCool(Guid.NewGuid(), Guid.NewGuid()), Instant.FromDateTimeUtc(DateTime.UtcNow.AddMinutes(1))).Await();

        private It no_exception_should_be_thrown = () => true.ShouldBeTrue();

        private Cleanup clean = () => LogManager.Flush();
    }
}
