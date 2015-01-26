using System;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_deleting_a_locator : TestBase
    {
        private static IAccessPolicy _policy;
        private static IAsset _asset;
        private static ILocator _locator;

        private Establish context = () =>
            {
                _policy = CloudMediaContext.AccessPolicies.CreateAsync(Guid.NewGuid().ToString("N"), TimeSpan.FromDays(30), AccessPermissions.Read).Result;
                _asset = CloudMediaContext.Assets.CreateAsync(Guid.NewGuid().ToString("N"),
                                                                   AssetCreationOptions.None, new CancellationToken()).Result;
                _locator = CloudMediaContext.Locators.CreateLocatorAsync(LocatorType.OnDemandOrigin, _asset,
                                                                              _policy).Result;
            };

        private Because of = () => MediaApi.DeleteLocator(_locator.Id, ConnectionString).Await();

        private It the_locator_should_be_removed =
            () => CloudMediaContext.Locators.Where(l => l.Id == _locator.Id).Count().ShouldEqual(0);

        private Cleanup cleanup = () =>
            {
                if (CloudMediaContext.Locators.Where(l => l.Id == _locator.Id).Count() != 0)
                {
                    _locator.DeleteAsync().Await();
                }
                if (_policy != null) _policy.DeleteAsync().Await();
                if (_asset != null) _asset.DeleteAsync().Await();
            };
    }
}
