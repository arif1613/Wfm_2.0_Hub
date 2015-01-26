using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_an_origin_with_a_name_that_already_exists : TestBase
    {
        private static string _name;
        private static int _units;
        private static string _originId;
        private static IOrigin _origin;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");            
            _units = 2;
            _origin = CloudMediaContext.Origins.CreateAsync(_name, _units).Result;
        };

        private Because of = () =>
        {
            _originId = MediaApi.CreateOrigin(_name, _units, ConnectionString).Await();
        };

        private It the_id_of_the_existent_origin_should_be_returned = () => _originId.ShouldEqual(_origin.Id);

        private Cleanup cleanup = () =>
            {
                if (_origin != null) _origin.DeleteAsync().Await();
            };
    }
}
