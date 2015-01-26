using System;
using Machine.Specifications;
using NLog;

namespace Bus.Tests.InMemoryBusTests
{
    public class when_trying_to_instantiate_a_bus_with_a_null_resolver
    {
        private static object _bus;
        private static Exception _ex;

        Because of = () => _ex = Catch.Exception(() => _bus = new InMemoryBus(null));

        private It an_exception_should_be_thrown = () => _ex.ShouldNotBeNull();

        private Cleanup clean = () => LogManager.Flush();
    }
}
