using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_stopping_a_channel : TestBase
    {
        private static string _name;
        private static IChannel _channel;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");

            _channel = CloudMediaContext.Channels.CreateAsync(_name, ChannelSize.Large, new ChannelSettings()
            {
                Ingest = new IngestEndpointSettings()
                {
                    Security = new IngestEndpointSecuritySettings()
                    {
                        IPv4AllowList = new List<Ipv4>()
                                            {
                                                new Ipv4(){IP = "0.0.0.0/0", Name = "my"}
                                            }
                    }
                }
            }).Result;
            _channel.StartAsync().Await();
        };

        private Because of = () => MediaApi.StopChannel(_channel.Id, ConnectionString).Await();

        private It the_channel_should_be_stopped =
            () =>
            {
                _channel = CloudMediaContext.Channels.Where(c => c.Id == _channel.Id).First();
                _channel.State.ShouldEqual(ChannelState.Stopped);
            };

        private Cleanup cleanup = () =>
        {
            if (_channel != null)
            {
                _channel = CloudMediaContext.Channels.SingleOrDefault(c => c.Id == _channel.Id);
                if (_channel.State == ChannelState.Running) _channel.StopAsync().Await();
                _channel.DeleteAsync().Await();
            }
        };
    }
}
