using System;
using System.Linq;
using Machine.Specifications;

namespace WamsApi.Tests
{
    public class when_creating_an_asset : TestBase
    {
        private static string _name;
        private static string _assetId;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");
        };

        private Because of = () =>
        {
            _assetId = MediaApi.CreateAsset(_name, ConnectionString).Await();
        };

        private It a_new_asset_should_be_created_with_that_name =
            () => CloudMediaContext.Assets.Where(c => c.Name == _name && c.Id == _assetId).Count().ShouldEqual(1);

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
