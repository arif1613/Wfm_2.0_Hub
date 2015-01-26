using System;
using System.Collections.Generic;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using CommonInfrastructureLibrary.Serialization.JsonNet;

namespace CommonSystemTestLibrary
{
    public class BusMessageSender
    {
        private IBus _bus;

        private static BusMessageSender _instance;

        public static BusMessageSender Instance
        {
            get { return _instance ?? (_instance = new BusMessageSender()); }
        }

        private BusMessageSender()
        {
            SetupBus();
        }

        public void SendMessage(IMessage e)
        {
            _bus.Publish(e).Wait();
        }

        private void SetupBus()
        {
            var azureBusConnectionString = Environment.GetEnvironmentVariable("AzureBusConnectionString");
            _bus = new Bus.Bus(azureBusConnectionString, new FakeHandlerResolver(), new BusSerializer(new Serializer()));
        }

        private sealed class FakeHandlerResolver : IHandlerResolver
        {
            public object Resolve(Type handlerType)
            {
                throw new NotImplementedException();
            }

            public object Resolve(Type handlerType, Dictionary<string, object> parameters)
            {
                throw new NotImplementedException();
            }
        }
    }
}
