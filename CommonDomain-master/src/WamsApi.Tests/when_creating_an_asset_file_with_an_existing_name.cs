using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_an_asset_file_with_an_existing_name : TestBase
    {
        private static string _name;
        private static string _assetId;
        private static string _assetFileName;
        private static IAssetFile _assetFile;
        private static IAsset _asset;
        private static IAssetFile _originalAssetFile;

        private Establish context = () =>
            {
                _name = Guid.NewGuid().ToString("N");
                _assetFileName = Guid.NewGuid().ToString("N");
                _assetId = MediaApi.CreateAsset(_name, ConnectionString).Result;
                _asset = MediaApi.GetAssetById(_assetId, ConnectionString).Result;
                _originalAssetFile = _asset.AssetFiles.Create(_assetFileName);
            };

        private Because of = () =>
            {
                _assetFile = MediaApi.CreateAssetFile(_assetId, _assetFileName, false, ConnectionString).Result;
            };

        private It the_existing_file_is_returned = () => _assetFile.Id.ShouldEqual(_originalAssetFile.Id);

        private It no_new_file_is_created = () => _asset.AssetFiles.Count().ShouldEqual(1);

        private Cleanup cleanup =
            () =>
                {
                    _asset.Delete();
                };
    }
}
