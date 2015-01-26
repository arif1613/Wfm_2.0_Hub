using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_an_asset_write_location_that_already_exists : TestBase
    {
        private Establish context = () =>
            {
                _assetId = MediaApi.CreateAsset(Guid.NewGuid().ToString("N"), ConnectionString).Result;
                _asset = MediaApi.GetAssetById(_assetId, ConnectionString).Result;
                _expectedDuration = TimeSpan.FromHours(10);
                var policyId = MediaApi.CreateAccessPolicy("WritePolicy", _expectedDuration, AccessPermissions.Write,
                                                           ConnectionString).Result;
                _writePolicy = MediaApi.GetAccessPolicyById(policyId, ConnectionString).Result;
                _originalLocation = MediaApi.CreateAssetWriteLocation(_asset, _writePolicy, ConnectionString).Result;
            };

        private Because of = () => _location = MediaApi.CreateAssetWriteLocation(_asset, _writePolicy, ConnectionString).Result;

        private It a_location_is_returned = () => _location.ShouldNotBeNull();

        private It the_location_is_the_same_as_the_existing_one = () => _location.Id.ShouldEqual(_originalLocation.Id);

        private It no_new_location_is_created =
            () => _asset.Locators.Count.ShouldEqual(1);

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
        private static IAsset _asset;
        private static ILocator _originalLocation;
        private static TimeSpan _expectedDuration;
        public static IAccessPolicy _writePolicy;
    }
}
