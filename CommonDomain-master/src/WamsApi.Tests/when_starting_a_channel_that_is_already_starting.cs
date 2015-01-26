using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_starting_a_channel_that_is_already_starting : TestBase
    {
        private static string _name;
        private static IChannel _channel;
        private static Task _task;

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
            _task = _channel.StartAsync();
            Thread.Sleep(2000);
        };

        private Because of = () =>
            {
                MediaApi.StartChannel(_channel.Id, ConnectionString).Await();
                _channel = CloudMediaContext.Channels.Where(c => c.Id == _channel.Id).First();
            };

        private It the_method_should_return_when_the_channel_has_finished_starting =
            () => _channel.State.ShouldEqual(ChannelState.Running);

        private Cleanup cleanup = () =>
            {
                _task.Await();
                _channel.StopAsync().Await();
                _channel.DeleteAsync().Await();
            };
    }
}
