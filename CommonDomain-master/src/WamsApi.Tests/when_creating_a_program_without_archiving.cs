using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_a_program_without_archiving : TestBase
    {
        private static string _name;
        private static string _programId;
        private static IChannel _channel;
        private static IProgram _program;
        private static IAsset _asset;
        private static TimeSpan _duration;
        private static bool _archived;
        private static int _dvrWindowInHours;

        private Establish context = () =>
            {
                _name = Guid.NewGuid().ToString("N");
                _duration = TimeSpan.FromMinutes(5);
                _archived = false;
                _dvrWindowInHours = 24;

                _channel = CloudMediaContext.Channels.CreateAsync(Guid.NewGuid().ToString("N"), ChannelSize.Large, new ChannelSettings()
                {
                    Ingest = new IngestEndpointSettings
                        {
                        Security = new IngestEndpointSecuritySettings
                            {
                            IPv4AllowList = new List<Ipv4>
                                {
                                                new Ipv4 {IP = "0.0.0.0/0", Name = "my"}
                                            }
                        }
                    }
                }).Result;
                _asset =
                    CloudMediaContext.Assets.CreateAsync(Guid.NewGuid().ToString("N"), AssetCreationOptions.None,
                                                              new CancellationToken()).Result;
            };

        private Because of = () =>
            {
                _programId = MediaApi.CreateProgram(_channel.Id, _name, _duration, _asset.Id, _archived, ConnectionString).Await();
                _program = CloudMediaContext.Channels.Where(c => c.Id == _channel.Id).First().Programs.Where(p => p.Name == _name).First();
            };

        private It the_program_should_have_the_a_24h_dvr_window =
            () => _program.DvrWindowLength.Value.Days.ShouldEqual(_dvrWindowInHours / 24);

        private Cleanup cleanup =
            () =>
            {                
                if (_program != null) _program.Delete();
                if (_asset != null) _asset.Delete();
                if (_channel != null) _channel.Delete();
            };

    }
}