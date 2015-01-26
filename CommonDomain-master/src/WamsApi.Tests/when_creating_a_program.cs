using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_a_program : TestBase
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
                _archived = true;
                _dvrWindowInHours = 6;

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
            };

        private Because of = () =>
            {
                _programId = MediaApi.CreateProgram(_channel.Id, _name, _duration, _asset.Id, _archived, ConnectionString).Await();
                _program = CloudMediaContext.Channels.Where(c => c.Id == _channel.Id).First().Programs.Where(p => p.Name == _name).First();
            };

        private It a_new_program_should_be_created_with_that_name =
            () => _program.ShouldNotBeNull();

        private It the_program_should_have_the_proper_name = () => _program.Name.ShouldEqual(_name);

        private It the_program_should_have_the_proper_duration = () => _program.EstimatedDuration.ShouldEqual(_duration);

        private It the_program_should_have_the_proper_asset_id = () => _program.AssetId.ShouldEqual(_asset.Id);

        private It the_proper_program_id_should_be_returned =
            () => _programId.ShouldEqual(_program.Id);

        private It the_program_archivation_should_be_set = () => _program.EnableArchive.ShouldEqual(_archived);

        private It the_program_should_have_a_6h_dvr_window =
            () => _program.DvrWindowLength.Value.Hours.ShouldEqual(_dvrWindowInHours);

        private Cleanup cleanup =
            () =>
            {                
                if (_program != null) _program.Delete();
                if (_asset != null) _asset.Delete();
                if (_channel != null) _channel.Delete();
            };

    }
}