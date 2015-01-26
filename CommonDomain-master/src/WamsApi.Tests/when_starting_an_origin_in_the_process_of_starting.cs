using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_starting_an_origin_in_the_process_of_starting : TestBase
    {
        private static IOrigin _origin;
        private static Task _task;

        private Establish context = () =>
        {
            _origin = CloudMediaContext.Origins.Create(Guid.NewGuid().ToString("N"), 2);
            _task = _origin.StartAsync();
            Thread.Sleep(2000);
        };

        private Because of = () =>
            {
                MediaApi.StartOrigin(_origin.Id, ConnectionString).Await();
                _origin = CloudMediaContext.Origins.Where(o => o.Id == _origin.Id).First();
            };

        private It no_exception_should_be_thrown = () => true.ShouldBeTrue();

        private It the_method_should_return_when_the_origin_is_running =
            () => _origin.State.ShouldEqual(OriginState.Running);

        private Cleanup cleanup = () =>
            {
                _task.Await();
                if (_origin != null)
                {
                    _origin.StopAsync().Await();
                    _origin.DeleteAsync().Await();
                }
            };
    }
}
