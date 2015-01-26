using System;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_retrieving_a_read_policy_by_id : TestBase
    {
        private static IAccessPolicy _originalPolicy;
        private static IAccessPolicy _policy;

        private Establish context = () =>
            {
                _originalPolicy = CloudMediaContext.AccessPolicies.CreateAsync(Guid.NewGuid().ToString("N"), TimeSpan.FromDays(30),
                                                                  AccessPermissions.Read).Result;
            };

        private Because of = () => _policy = MediaApi.GetAccessPolicyById(_originalPolicy.Id, ConnectionString).Result;

        private It the_right_policy_should_be_returned = () => _policy.Id.ShouldEqual(_originalPolicy.Id);

        private Cleanup cleanup = () =>
            {
                if (_originalPolicy != null)
                {
                    _originalPolicy.DeleteAsync().Await();
                }
            };
    }
}
