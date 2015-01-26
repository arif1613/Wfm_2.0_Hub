using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_starting_a_program_with_a_dirty_asset : TestBase
    {
        private static IProgram _program;
        private static IAsset _asset;
        private static IChannel _channel;

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
            _channel.StartAsync().Await();
            _asset =
                CloudMediaContext.Assets.CreateAsync(Guid.NewGuid().ToString("N"), AssetCreationOptions.None,
                                                          new CancellationToken()).Result;
            _program = _channel.Programs.CreateAsync(Guid.NewGuid().ToString("N"), "", true, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), _asset.Id).Result;
            _program.StartAsync().Await();
            _program.StopAsync().Await();
        };

        private Because of = () => MediaApi.StartProgram(_program.Id, ConnectionString).Await();

        private It the_program_should_be_started =
            () => CloudMediaContext.Programs.Where(p => p.Id == _program.Id).First().State.ShouldEqual(ProgramState.Running);

        private Cleanup cleanup =
            () =>
            {
                if (_program != null)
                {
                    _program = CloudMediaContext.Programs.Where(p => p.Id == _program.Id).First();
                    if (_program.State == ProgramState.Running) _program.StopAsync().Await();
                    _program.DeleteAsync().Await();
                }
                
                if (_asset != null)
                {
                    _asset = CloudMediaContext.Assets.Where(a => a.Id == _asset.Id).First();
                    _asset.DeleteAsync().Await();
                }
                
                if (_channel != null)
                {
                    _channel = CloudMediaContext.Channels.Where(c => c.Id == _channel.Id).First();
                    if(_channel.State == ChannelState.Running) _channel.StopAsync().Await();
                    _channel.DeleteAsync().Await();
                }
            };
    }
}
