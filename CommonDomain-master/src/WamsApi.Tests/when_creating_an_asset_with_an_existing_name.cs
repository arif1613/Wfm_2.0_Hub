using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_an_asset_with_an_existing_name : TestBase
    {
        private static string _name;
        private static string _assetId;
        private static IAsset _asset;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");
            _asset = CloudMediaContext.Assets.Create(_name, AssetCreationOptions.None);
        };

        private Because of = () =>
        {
            _assetId = MediaApi.CreateAsset(_name, ConnectionString).Await();
        };

        private It the_id_of_the_existing_asset_should_be_returned = () => _assetId.ShouldEqual(_asset.Id);

        private Cleanup cleanup =
            () =>
            {
                if (CloudMediaContext.Assets.Where(c => c.Name == _name).Count() > 0)
                {
                    CloudMediaContext.Assets.Where(c => c.Name == _name).First().DeleteAsync();
                }
            };        
    }
}
