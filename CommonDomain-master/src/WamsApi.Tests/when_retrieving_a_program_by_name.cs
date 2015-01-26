using System;
using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_retrieving_a_program_by_name : TestBase
    {
        private static IChannel _channel;
        private static IProgram _originalProgram;
        private static IAsset _asset;
        private static IProgram _program;

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
            _originalProgram = _channel.Programs.CreateAsync(Guid.NewGuid().ToString("N"), "", true, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), _asset.Id).Result;
        };

        private Because of = () => _program = MediaApi.GetProgramByName(_originalProgram.Name, ConnectionString).Result;

        private It the_right_program_should_be_returned = () => _program.Id.ShouldEqual(_originalProgram.Id);

        private Cleanup cleanup = () =>
        {
            if (_channel != null) _channel.DeleteAsync().Await();
            if (_originalProgram != null) _originalProgram.DeleteAsync().Await();
            if (_asset != null) _asset.DeleteAsync().Await();
        };
    }
}
