using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_stopping_an_origin_that_is_already_stopping : TestBase
    {
        private static IOrigin _origin;
        private static Task _task;

        private Establish context = () =>
        {
            _origin = CloudMediaContext.Origins.Create(Guid.NewGuid().ToString("N"), 2);
            _origin.StartAsync().Await();
            _task = _origin.StopAsync();
            Thread.Sleep(2000);
        };

        private Because of = () =>
        {
            MediaApi.StopOrigin(_origin.Id, ConnectionString).Await();
            _origin = CloudMediaContext.Origins.Where(o => o.Id == _origin.Id).First();
        };

        private It no_exception_should_be_thrown = () => true.ShouldBeTrue();

        private It the_method_should_return_when_the_origin_is_running =
            () => _origin.State.ShouldEqual(OriginState.Stopped);

        private Cleanup cleanup = () =>
        {
            _task.Await();
            if (_origin != null)
            {
                _origin.DeleteAsync().Await();
            }
        };
    }
}
