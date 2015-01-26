using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_an_origin : TestBase
    {
        private static string _name;
        private static int _units;
        private static string _originId;
        private static IOrigin _origin;

        private Establish context = () =>
            {
                _name = Guid.NewGuid().ToString("N");
                _units = 2;
            };

        private Because of = () =>
            {
                _originId = MediaApi.CreateOrigin(_name, _units, ConnectionString).Await();
                _origin = CloudMediaContext.Origins.Where(o => o.Id == _originId).First();
            };

        private It an_origin_should_be_created = () => true.ShouldBeTrue();
        private It the_name_should_be_correct = () => _origin.Name.ShouldEqual(_name);
        private It the_number_of_reserved_units_should_be_set = () => _origin.ReservedUnits.ShouldEqual(_units);

        private Cleanup cleanup = () => _origin.DeleteAsync().Await();
    }
}
