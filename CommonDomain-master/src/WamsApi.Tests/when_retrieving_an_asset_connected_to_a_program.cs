using System;
using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_retrieving_an_asset_connected_to_a_program : TestBase
    {
        private static string _name;
        protected static IAsset _originalAsset;
        protected static IAsset _asset;
        protected static List<IAssetFile> _assetFiles;
        private static IProgram _program;
        private static IChannel _channel;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");
            _assetFiles = new List<IAssetFile>();
            _originalAsset = CloudMediaContext.Assets.CreateAsync(_name, AssetCreationOptions.None, new CancellationToken()).Result;
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
            _program = _channel.Programs.CreateAsync(_name, false, TimeSpan.FromMinutes(1),
                                                               TimeSpan.FromHours(1), _originalAsset.Id).Result;
            foreach (var file in _originalAsset.AssetFiles)
            {
                _assetFiles.Add(file);
            }
        };

        private Because of = () =>
        {
            _asset = MediaApi.GetAssetById(_originalAsset.Id, ConnectionString).Result;
        };

        private Behaves_like<behaves_like_an_asset> check_asset_properties;

        private Cleanup cleanup = () =>
            {
                if (_program != null) _program.DeleteAsync().Await();
                if (_originalAsset != null)
                    _originalAsset.DeleteAsync().Await();
                if (_channel != null) _channel.DeleteAsync().Await();
            };
    }
}
