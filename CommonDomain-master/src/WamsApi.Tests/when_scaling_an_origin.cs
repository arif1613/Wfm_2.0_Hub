using System;
using System.Linq;
using Machine.Specifications;

namespace WamsApi.Tests
{
    public class when_scaling_an_origin : TestBase
    {
        private static string _originId;
        private static int _size;

        private Establish context = () =>
        {
            _originId = CloudMediaContext.Origins.Create(Guid.NewGuid().ToString("N"), 2).Id;
            _size = 4;
        };

        private Because of = () => MediaApi.ScaleOrigin(_originId, _size, ConnectionString).Await();

        private It the_reserved_units_number_should_change_should_be_removed = () => CloudMediaContext.Origins.Where(o => o.Id == _originId).First().ReservedUnits.ShouldEqual(_size);

        private Cleanup cleanup =
            () => CloudMediaContext.Origins.Where(o => o.Id == _originId).First().DeleteAsync().Await();
    }
}
