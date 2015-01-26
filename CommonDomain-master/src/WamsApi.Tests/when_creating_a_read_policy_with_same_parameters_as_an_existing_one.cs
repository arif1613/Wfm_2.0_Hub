using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_a_read_policy_with_same_parameters_as_an_existing_one : TestBase
    {
        private static string _name;
        private static TimeSpan _duration;
        private static AccessPermissions _permissions;
        private static string _policyId;
        private static IAccessPolicy _policy;

        private Establish context = () =>
        {
            _name = Guid.NewGuid().ToString("N");
            _duration = TimeSpan.FromDays(10);
            _permissions = AccessPermissions.Read;
            _policy = CloudMediaContext.AccessPolicies.Create(_name, _duration, AccessPermissions.Read);
        };

        private Because of = () =>
        {
            _policyId = MediaApi.CreateAccessPolicy(_name, _duration, _permissions, ConnectionString).Await();
        };

        private It the_id_of_the_existing_policy_should_be_returned = () => _policyId.ShouldEqual(_policy.Id);

        private Cleanup cleanup = () =>
        {
            if (_policy != null) _policy.DeleteAsync().Await();
        };
    }
}
