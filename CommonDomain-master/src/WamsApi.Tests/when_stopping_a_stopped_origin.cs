using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_stopping_a_stopped_origin : TestBase
    {
        private static IOrigin _origin;

        private Establish context = () =>
        {
            _origin = CloudMediaContext.Origins.Create(Guid.NewGuid().ToString("N"), 2);
        };

        private Because of = () =>
        {
            MediaApi.StopOrigin(_origin.Id, ConnectionString).Await();
        };

        private It no_exception_should_be_thrown = () => true.ShouldBeTrue();

        private Cleanup cleanup = () =>
        {
            if (_origin != null)
            {
                _origin.DeleteAsync().Await();
            }
        };
    }
}
