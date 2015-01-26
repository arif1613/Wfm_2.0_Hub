using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_starting_an_origin_that_is_already_started : TestBase
    {
        private static IOrigin _origin;

        private Establish context = () =>
        {
            _origin = CloudMediaContext.Origins.Create(Guid.NewGuid().ToString("N"), 2);
            _origin.StartAsync().Await();
        };

        private Because of = () => MediaApi.StartOrigin(_origin.Id, ConnectionString).Await();

        private It no_exception_should_be_thrown = () => true.ShouldBeTrue();

        private Cleanup cleanup = () =>
        {
            if (_origin != null)
            {
                _origin.StopAsync().Await();
                _origin.DeleteAsync().Await();
            }
        };
    }
}
