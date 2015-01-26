using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_a_channel_with_an_existing_name : TestBase
    {
        private static string _name;
        private static string _channelId;
        private static IChannel _channel;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");
            _channel = CloudMediaContext.Channels.Create(_name, ChannelSize.Large, new ChannelSettings
                {
                    Ingest = new IngestEndpointSettings
                        {
                            Security = new IngestEndpointSecuritySettings
                                {
                                    IPv4AllowList = new List<Ipv4>()
                                            {
                                                new Ipv4 { IP = "0.0.0.0/0", Name = "my" }
                                            }
                                }
                        }
                });
        };

        private Because of = () =>
        {
            _channelId = MediaApi.CreateChannel(_name, ConnectionString).Await();
        };

        private It the_id_of_the_existing_channel_should_be_returned = () => _channelId.ShouldEqual(_channel.Id);

        private Cleanup cleanup =
            () =>
            {
                if (CloudMediaContext.Channels.Where(c => c.Name == _name).Count() > 0)
                {
                    CloudMediaContext.Channels.Where(c => c.Name == _name).First().DeleteAsync();
                }
            };        
    }
}
