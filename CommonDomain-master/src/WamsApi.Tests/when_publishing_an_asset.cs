using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    class when_publishing_an_asset : TestBase
    {
        private static string _assetName;
        private static string _assetId;
        private static string _assetFileName;
        private static IAsset _asset;
        private static bool _isPrimary;
        private static IAssetFile _assetFile;
        private static IAccessPolicy _policy;
        private static string _expectedPolicyName;
        private static string _publishUrl;
        private static TimeSpan _expectedPolicyDuration;

        private Establish context = () =>
            {
                _assetName = Guid.NewGuid().ToString("N");
                _assetId = MediaApi.CreateAsset(_assetName, ConnectionString).Result;
                _asset = MediaApi.GetAssetById(_assetId, ConnectionString).Result;
                _assetFileName = "mainfile.ism";
                _isPrimary = true;
                _expectedPolicyName = Guid.NewGuid().ToString("N");
                _expectedPolicyDuration = TimeSpan.FromHours(1);
                var policyId = MediaApi.CreateAccessPolicy(_expectedPolicyName, _expectedPolicyDuration, AccessPermissions.Read, ConnectionString).Result;
                _policy = MediaApi.GetAccessPolicyById(policyId, ConnectionString).Result;
                _assetFile = MediaApi.CreateAssetFile(_assetId, _assetFileName, _isPrimary, ConnectionString).Result;
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

        private It a_locator_should_be_created_on_the_asset = () => _asset.Locators.Count.ShouldEqual(1);

        private It a_vod_access_policy_should_be_assured =
            () =>
            CloudMediaContext.AccessPolicies.Where(a => a.Name == _expectedPolicyName)
                             .FirstOrDefault()
                             .ShouldNotBeNull();

        private It the_vod_access_policy_should_have_the_correct_duration =
            () =>
            CloudMediaContext.AccessPolicies.Where(a => a.Name == _expectedPolicyName)
                             .FirstOrDefault().Duration.ShouldEqual(_expectedPolicyDuration);

    }
}
