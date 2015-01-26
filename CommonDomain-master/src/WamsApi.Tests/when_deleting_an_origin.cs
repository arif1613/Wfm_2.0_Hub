using System;
using System.Linq;
using Machine.Specifications;

namespace WamsApi.Tests
{
    public class when_deleting_an_origin : TestBase
    {
        private static string _originId;

        private Establish context = () =>
        {
            _originId = CloudMediaContext.Origins.Create(Guid.NewGuid().ToString("N"), 2).Id;
        };

        private Because of = () => MediaApi.DeleteOrigin(_originId, ConnectionString).Await();

        private It the_origin_should_be_removed = () => CloudMediaContext.Origins.Where(o => o.Id == _originId).Count().ShouldEqual(0);

        private Cleanup cleanup = () =>
            {
                if (CloudMediaContext.Origins.Where(o => o.Id == _originId).Count() != 0)
                {
                    CloudMediaContext.Origins.Where(o => o.Id == _originId).First().DeleteAsync().Await();
                }
            };
    }
}
