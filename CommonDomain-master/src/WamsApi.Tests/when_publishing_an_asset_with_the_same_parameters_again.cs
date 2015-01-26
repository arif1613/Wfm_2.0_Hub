using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    class when_publishing_an_asset_with_the_same_parameters_again : TestBase
    {
        private static string _assetName;
        private static string _assetId;
        private static string _assetFileName;
        private static IAsset _asset;
        private static bool _isPrimary;
        private static IAssetFile _assetFile;
        private static IAccessPolicy _policy;
        private static string _policyName;

        private static string _publishUrl;

        private Establish context = () =>
            {
                _assetName = Guid.NewGuid().ToString("N");
                _assetId = MediaApi.CreateAsset(_assetName, ConnectionString).Result;
                _asset = MediaApi.GetAssetById(_assetId, ConnectionString).Result;
                _policyName = Guid.NewGuid().ToString("N");
                var policyName = Guid.NewGuid().ToString("N");
                var policyId = MediaApi.CreateAccessPolicy(policyName, TimeSpan.FromHours(1), AccessPermissions.Read, ConnectionString).Result;
                _policy = MediaApi.GetAccessPolicyById(policyId, ConnectionString).Result;
                _assetFileName = "mainfile.ism";
                _isPrimary = true;
                _assetFile = MediaApi.CreateAssetFile(_assetId, _assetFileName, _isPrimary, ConnectionString).Result;
                MediaApi.PublishVodAsset(_asset, _policy, ConnectionString).Await();
            };

        private Because of = () =>
            {
                _publishUrl = MediaApi.PublishVodAsset(_asset, _policy, ConnectionString).Result;
            };

        private Cleanup clean = () =>
            {
                if (_asset != null)
                {
                    _asset.Delete();
                }
                if (_policy != null)
                {
                    _policy.Delete();
                }
            };

        private It a_publish_url_should_be_returned = () => _publishUrl.ShouldEndWith(_assetFileName + "/manifest");

        private It no_additional_access_locator_should_be_created_created = () => _asset.Locators.Count.ShouldEqual(1);
    }
}
