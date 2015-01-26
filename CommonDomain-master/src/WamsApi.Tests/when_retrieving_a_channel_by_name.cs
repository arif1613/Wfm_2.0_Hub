using System;
using System.Collections.Generic;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_retrieving_a_channel_by_name : TestBase
    {
        private static string _name;
        private static IChannel _originalChannel;
        private static IChannel _channel;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");

            _originalChannel = CloudMediaContext.Channels.CreateAsync(_name, ChannelSize.Large, new ChannelSettings()
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
        };

        private Because of = () =>
        {
            _channel = MediaApi.GetChannelByName(_name, ConnectionString).Result;
        };

        private It the_right_channel_should_be_returned = () => _channel.Id.ShouldEqual(_originalChannel.Id);

        private Cleanup cleanup = () => _originalChannel.DeleteAsync().Await();  
    }
}
