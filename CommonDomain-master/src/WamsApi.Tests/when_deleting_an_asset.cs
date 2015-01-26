using System;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_deleting_an_asset : TestBase
    {
        private static string _name;
        private static IAsset _asset;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");

            _asset = CloudMediaContext.Assets.CreateAsync(_name, AssetCreationOptions.None, new CancellationToken()).Result;
        };

        private Because of = () => MediaApi.DeleteAsset(_asset.Id, ConnectionString).Await();

        private It the_asset_should_be_deleted =
            () => CloudMediaContext.Assets.Where(c => c.Id == _asset.Id).Count().ShouldEqual(0);

        private Cleanup cleanup = () =>
        {
            if (CloudMediaContext.Assets.Where(c => c.Id == _asset.Id).Count() != 0)
            {
                CloudMediaContext.Assets.Where(c => c.Id == _asset.Id).First().DeleteAsync().Await();
            }
        };
    }
}
