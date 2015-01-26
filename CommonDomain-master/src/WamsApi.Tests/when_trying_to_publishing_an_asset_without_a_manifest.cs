using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    class when_trying_to_publishing_an_asset_without_a_manifest : TestBase
    {
        private static string _assetName;
        private static string _assetId;
        private static IAsset _asset;
        private static ArgumentException _ex;
        private static string _expectedErrorMessage; 

        private Establish context = () =>
            {
                _assetName = Guid.NewGuid().ToString("N");
                _assetId = MediaApi.CreateAsset(_assetName, ConnectionString).Result;
                _asset = MediaApi.GetAssetById(_assetId, ConnectionString).Result;
                _expectedErrorMessage = "No manifest file found for asset " + _assetName;
            };

        private Because of = () =>
            {
                _ex = Catch.Exception(() => MediaApi.PublishVodAsset(_asset, null, ConnectionString).Await()) as ArgumentException;
            };

        private Cleanup clean = () =>
            {
                if (_asset != null)
                {
                    _asset.Delete();
                }
            };

        private It an_argument_exception_should_be_thrown = () => _ex.ShouldNotBeNull();
        private It the_exception_contains_the_correct_message = () => _ex.Message.ShouldEqual(_expectedErrorMessage);

    }
}
