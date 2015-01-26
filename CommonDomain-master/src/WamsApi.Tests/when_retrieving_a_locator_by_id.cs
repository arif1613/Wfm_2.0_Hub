using System;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_retrieving_a_locator_by_id : TestBase
    {
        private static IAccessPolicy _policy;
        private static IAsset _asset;
        private static ILocator _originalLocator;
        private static ILocator _locator;

        private Establish context = () =>
        {
            _policy = CloudMediaContext.AccessPolicies.CreateAsync(Guid.NewGuid().ToString("N"), TimeSpan.FromDays(30), AccessPermissions.Read).Result;
            _asset = CloudMediaContext.Assets.CreateAsync(Guid.NewGuid().ToString("N"),
                                                               AssetCreationOptions.None, new CancellationToken()).Result;
            _originalLocator = CloudMediaContext.Locators.CreateLocatorAsync(LocatorType.OnDemandOrigin, _asset,
                                                                          _policy).Result;
        };

        private Because of = () => _locator = MediaApi.GetLocatorById(_originalLocator.Id, ConnectionString).Result;

        private It the_right_locator_should_be_returned = () => _locator.Id.ShouldEqual(_originalLocator.Id);

        private Cleanup cleanup = () =>
            {
                if (_originalLocator != null) _originalLocator.DeleteAsync().Await();
                if (_policy != null) _policy.DeleteAsync().Await();
                if (_asset != null) _asset.DeleteAsync().Await();
            };
    }
}
