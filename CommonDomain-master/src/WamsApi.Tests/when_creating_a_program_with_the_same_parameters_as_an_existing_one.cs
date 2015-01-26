using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_a_program_with_the_same_parameters_as_an_existing_one : TestBase
    {
        private static string _name;
        private static string _programId;
        private static IChannel _channel;
        private static IProgram _program;
        private static IAsset _asset;
        private static TimeSpan _duration;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");
            _duration = TimeSpan.FromMinutes(5);

            _channel = CloudMediaContext.Channels.CreateAsync(Guid.NewGuid().ToString("N"), ChannelSize.Large, new ChannelSettings()
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
            _asset =
                CloudMediaContext.Assets.CreateAsync(Guid.NewGuid().ToString("N"), AssetCreationOptions.None,
                                                          new CancellationToken()).Result;
            _program = CloudMediaContext.Channels.Where(c => c.Id == _channel.Id).First().Programs.Create(_name, true, TimeSpan.FromMinutes(5), _duration, _asset.Id);
        };

        private Because of = () =>
        {
            _programId = MediaApi.CreateProgram(_channel.Id, _name, _duration, _asset.Id, true, ConnectionString).Await();
        };

        private It the_id_of_the_existing_program_should_be_returned = () => _programId.ShouldEqual(_program.Id);

        private Cleanup cleanup =
            () =>
            {
                if (_channel != null) _channel.DeleteAsync();
                if (_asset != null) _asset.DeleteAsync();
                if (_program != null) _program.DeleteAsync();
            };
    }
}
