using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_an_asset_file : TestBase
    {
        private static string _name;
        private static string _assetId;
        private static string _assetFileName;
        private static IAssetFile _assetFile;
        private static bool _isPrimary;
        private static IAsset _asset;
        private static IAssetFile _assetFileFromAsset;

        private Establish context = () =>
            {
                _name = Guid.NewGuid().ToString("N");
                _assetFileName = Guid.NewGuid().ToString("N");
                _isPrimary = true;
                _assetId = MediaApi.CreateAsset(_name, ConnectionString).Result;
                _asset = MediaApi.GetAssetById(_assetId, ConnectionString).Result;
            };

        private Because of = () =>
            {
                _assetFile = MediaApi.CreateAssetFile(_assetId, _assetFileName, _isPrimary, ConnectionString).Result;
                _assetFileFromAsset = _asset.AssetFiles.Where(f => f.Name == _assetFileName).SingleOrDefault();
            };

        private It an_new_asset_file_should_be_created = () => _assetFile.ShouldNotBeNull();

        private It the_asset_file_should_be_stored_in_the_asset = () => _assetFileFromAsset.ShouldNotBeNull();

        private It the_primary_flag_should_be_set_on_the_file = () => _assetFile.IsPrimary.ShouldEqual(_isPrimary);

        private It the_file_should_have_the_correcy_name = () => _assetFile.Name.ShouldEqual(_assetFileName);

        private Cleanup cleanup =
            () =>
                {
                    _asset.Delete();
                };
    }
}
