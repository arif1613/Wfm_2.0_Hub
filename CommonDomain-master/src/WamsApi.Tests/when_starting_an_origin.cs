using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_starting_an_origin : TestBase
    {
        private static IOrigin _origin;
        private static string _originId;

        private Establish context = () =>
        {
            _originId = CloudMediaContext.Origins.Create(Guid.NewGuid().ToString("N"), 2).Id;
        };

        private Because of = () =>
            {
                MediaApi.StartOrigin(_originId, ConnectionString).Await();
                _origin = CloudMediaContext.Origins.Where(o => o.Id == _originId).First();
            };

        private It the_origin_should_be_started = () => _origin.State.ShouldEqual(OriginState.Running);

        private Cleanup cleanup = () =>
            {
                _origin.StopAsync().Await();
                _origin.DeleteAsync().Await();
            };
    }
}
