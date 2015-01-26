using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_deleting_a_program : TestBase
    {
        private static IChannel _channel;
        private static IProgram _program;
        private static IAsset _asset;

        private Establish context = () =>
        {
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
            _program = _channel.Programs.CreateAsync(Guid.NewGuid().ToString("N"), "", true, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), _asset.Id).Result;
        };

        private Because of = () =>
            {
                MediaApi.DeleteProgram(_program.Id, ConnectionString).Await();
                _channel = CloudMediaContext.Channels.Where(c => c.Id == _channel.Id).First();
            };

        private It the_program_should_be_removed =
            () => _channel.Programs.Where(p => p.Id == _program.Id).Count().ShouldEqual(0);

        private Cleanup cleanup =
            () =>
                {
                    if (_program != null) _program.DeleteAsync().Await();
                if (_channel != null) _channel.DeleteAsync().Await();
                if (_asset != null) _asset.DeleteAsync().Await();
            };
    }
}
