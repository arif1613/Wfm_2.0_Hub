using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_retrieving_an_origin_by_name : TestBase
    {
        private static IOrigin _origin;
        private static IOrigin _originalOrigin;

        private Establish context = () =>
        {
            _originalOrigin = CloudMediaContext.Origins.Create(Guid.NewGuid().ToString("N"), 2);
        };

        private Because of = () => _origin = MediaApi.GetOriginByName(_originalOrigin.Name, ConnectionString).Result;

        private It the_right_origin_should_be_returned = () => _origin.Id.ShouldEqual(_originalOrigin.Id);

        private Cleanup cleanup = () => _originalOrigin.DeleteAsync().Await();
    }
}
