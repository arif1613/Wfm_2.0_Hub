using System;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_a_locator : TestBase
    {
        private static IAccessPolicy _policy;
        private static IAsset _asset;
        private static ILocator _locator;

        private Establish context = () =>
            {
                _policy = CloudMediaContext.AccessPolicies.CreateAsync(Guid.NewGuid().ToString("N"), TimeSpan.FromDays(30), AccessPermissions.Read).Result;
                _asset = CloudMediaContext.Assets.CreateAsync(Guid.NewGuid().ToString("N"),
                                                                   AssetCreationOptions.None, new CancellationToken()).Result;
            };

        private Because of = () =>
            {
                MediaApi.CreateLocator(_asset.Id, _policy.Id, ConnectionString).Await();
                _locator = CloudMediaContext.Locators.Where(l => l.AssetId == _asset.Id).First();
            };

        private It a_new_locator_should_be_created = () => _locator.ShouldNotBeNull();
        private It the_new_locator_should_have_the_right_asset_it = () => _locator.AssetId.ShouldEqual(_asset.Id);

        private It the_new_locator_should_have_the_right_policy_id =
            () => _locator.AccessPolicyId.ShouldEqual(_policy.Id);

        private Cleanup cleanup = () =>
            {
                if (_locator != null) _locator.DeleteAsync().Await();
                if (_policy != null) _policy.DeleteAsync().Await();
                if (_asset != null) _asset.DeleteAsync().Await();
            };
    }
}
