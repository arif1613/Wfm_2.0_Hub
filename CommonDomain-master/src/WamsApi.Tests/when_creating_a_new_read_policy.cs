using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_creating_a_new_read_policy : TestBase
    {
        private static TimeSpan _duration;
        private static AccessPermissions _permissions;
        private static string _name;
        private static string _policyId;
        private static IAccessPolicy _policy;

        private Establish contest = () =>
            {
                _name = Guid.NewGuid().ToString("N");
                _duration = TimeSpan.FromDays(30);
                _permissions = AccessPermissions.Read;
            };
        private Because of = () =>
            {
                _policyId = MediaApi.CreateAccessPolicy(_name, _duration, _permissions, ConnectionString).Await();
                _policy = CloudMediaContext.AccessPolicies.Where(p => p.Id == _policyId).First();
            };

        private It a_new_policy_should_be_created = () => _policy.ShouldNotBeNull();
        private It the_policy_should_have_the_desired_name = () => _policy.Name.ShouldEqual(_name);
        private It the_policy_should_have_the_desired_duration = () => _policy.Duration.ShouldEqual(_duration);
        private It the_policy_should_have_the_desired_permissions = () => _policy.Permissions.ShouldEqual(_permissions);

        private Cleanup cleanup = () =>
            {
                if (_policy != null) _policy.DeleteAsync().Await();
            };
    }
}
