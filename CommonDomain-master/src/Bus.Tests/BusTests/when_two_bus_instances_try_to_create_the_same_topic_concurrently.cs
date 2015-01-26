using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using CommonTestingFramework;
using Machine.Specifications;
using NLog;

namespace Bus.Tests.BusTests
{
    public class when_two_bus_instances_try_to_create_the_same_topic_concurrently
    {
        private static readonly IHandlerResolver _handlerResolver = new TestHandlerResolver(new Dictionary<Type, object>());
        private static Bus _bus1;
        private static Bus _bus2;

        private Establish context = () =>
            {
                _bus1 = new Bus(AssemblyContext.ServiceBusConnectionString, _handlerResolver, new BusSerializer(new Serializer()));
                _bus2 = new Bus(AssemblyContext.ServiceBusConnectionString, _handlerResolver, new BusSerializer(new Serializer()));
            };

        private Because of = () =>
            {
                var task1 = _bus1.Subscribe(typeof (BeCool), typeof (TestConsumer<BeCool>));
                var task2 = _bus2.Subscribe(typeof (BeCool), typeof (TestConsumer<BeCool>));
                Task.WaitAll(new [] {task1, task2});
            };

        private It no_exception_should_be_thrown = () => true.ShouldBeTrue();

        Cleanup clean = () => LogManager.Flush();
    }
}
