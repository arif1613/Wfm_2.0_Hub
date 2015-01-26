using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_stopping_a_started_origin : TestBase
    {
        private static IOrigin _origin;
        private static string _originId;

        private Establish context = () =>
        {
            _originId = CloudMediaContext.Origins.Create(Guid.NewGuid().ToString("N"), 2).Id;
            var origin = CloudMediaContext.Origins.Where(o => o.Id == _originId).First();
            origin.StartAsync().Await();
        };

        private Because of = () =>
        {
            MediaApi.StopOrigin(_originId, ConnectionString).Await();
            _origin = CloudMediaContext.Origins.Where(o => o.Id == _originId).First();
        };

        private It the_origin_should_be_stopped = () => _origin.State.ShouldEqual(OriginState.Stopped);

        private Cleanup cleanup = () =>
        {
            _origin.StopAsync().Await();
            _origin.DeleteAsync().Await();
        };
    }
}
