using System;
using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_retrieving_an_asset_by_name : TestBase
    {
        private static string _name;
        protected static IAsset _originalAsset;
        protected static IAsset _asset;
        protected static List<IAssetFile> _assetFiles;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");
            _assetFiles = new List<IAssetFile>();
            _originalAsset = CloudMediaContext.Assets.CreateAsync(_name, AssetCreationOptions.None, new CancellationToken()).Result;
        };

        private Because of = () =>
        {
            _asset = MediaApi.GetAssetByName(_name, ConnectionString).Result;
        };

        private Behaves_like<behaves_like_an_asset> check_asset_properties;

        private Cleanup cleanup = () =>
            {
                if (_originalAsset != null)
                    _originalAsset.DeleteAsync().Await();
            };
    }
}
