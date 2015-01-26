using System;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_a_locator_with_the_same_parameters_as_an_existing_one : TestBase
    {
        private static IAccessPolicy _policy;
        private static IAsset _asset;
        private static ILocator _locator;
        private static string _id;

        private Establish context = () =>
        {
            _policy = CloudMediaContext.AccessPolicies.CreateAsync(Guid.NewGuid().ToString("N"), TimeSpan.FromDays(30), AccessPermissions.Read).Result;
            _asset = CloudMediaContext.Assets.CreateAsync(Guid.NewGuid().ToString("N"),
                                                               AssetCreationOptions.None, new CancellationToken()).Result;
            _locator = CloudMediaContext.Locators.CreateLocatorAsync(LocatorType.OnDemandOrigin, _asset,
                                                                          _policy).Result;
        };

        private Because of = () => _id = MediaApi.CreateLocator(_asset.Id, _policy.Id, ConnectionString).Await();

        private It the_id_of_the_existing_one_should_be_returned = () => _id.ShouldEqual(_locator.Id);

        private Cleanup cleanup = () =>
        {
            if (_locator != null)
            {
                _locator.DeleteAsync().Await();
            }
            if (_policy != null) _policy.DeleteAsync().Await();
            if (_asset != null) _asset.DeleteAsync().Await();
        };
    }
}
