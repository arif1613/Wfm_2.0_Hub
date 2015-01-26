using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_an_asset_write_location : TestBase
    {
        private Establish context = () =>
            {
                _assetId = MediaApi.CreateAsset(Guid.NewGuid().ToString("N"), ConnectionString).Result;
                _asset = MediaApi.GetAssetById(_assetId, ConnectionString).Result;
                _expectedDuration = TimeSpan.FromHours(10);
                var policyId = MediaApi.CreateAccessPolicy("WritePolicy", _expectedDuration, AccessPermissions.Write,
                                                           ConnectionString).Result;
                _writePolicy = MediaApi.GetAccessPolicyById(policyId, ConnectionString).Result;
            };

        private Because of = () => _location = MediaApi.CreateAssetWriteLocation(_asset, _writePolicy, ConnectionString).Result;

        private It a_location_is_returned = () => _location.ShouldNotBeNull();

        private It the_location_is_connected_to_the_asset = () =>  _location.AssetId.ShouldEqual(_assetId);

        private It the_policy_is_set_to_write =
            () => _location.AccessPolicy.Permissions.ShouldEqual(AccessPermissions.Write);

        private It the_policy_duration_set_to_10_hours =
            () => _location.AccessPolicy.Duration.ShouldEqual(_expectedDuration);

        private Cleanup cleanup =
            () =>
                {
                    if (_location != null)
                    {
                        _location.Delete();
                    }
                    if (_asset != null)
                    {
                        _asset.Delete();
                    }
                    if (_writePolicy != null)
                    {
                        _writePolicy.Delete();
                    }
                };

        private static string _assetId;
        private static ILocator _location;
        private static IAccessPolicy _writePolicy;
        private static IAsset _asset;
        private static TimeSpan _expectedDuration;
    }
}
