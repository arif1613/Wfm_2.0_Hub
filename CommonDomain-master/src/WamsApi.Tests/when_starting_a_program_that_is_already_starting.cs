using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_starting_a_program_that_is_already_starting : TestBase
    {
        private static IProgram _program;
        private static IAsset _asset;
        private static IChannel _channel;
        private static Task _task;

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
            _task = _program.StartAsync();
            Thread.Sleep(2000);
        };

        private Because of = () =>
            {
                MediaApi.StartProgram(_program.Id, ConnectionString).Await();
                _program = _channel.Programs.Where(p => p.Id == _program.Id).First();
            };

        private It the_method_should_return_when_the_program_has_finished_starting =
            () => _program.State.ShouldEqual(ProgramState.Running);

        private Cleanup cleanup =
            () =>
            {
                _task.Await();
                if (_program != null)
                {
                    _program.StopAsync().Await();
                    _program.DeleteAsync().Await();
                }
                if (_asset != null) _asset.DeleteAsync().Await();
                if (_channel != null)
                {
                    _channel.StopAsync().Await();
                    _channel.DeleteAsync().Await();
                }
            };
    }
}
