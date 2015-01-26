using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_deleting_an_access_policy : TestBase
    {
        private static IAccessPolicy _policy;

        private Establish context = () =>
            {
                _policy = CloudMediaContext.AccessPolicies.CreateAsync(Guid.NewGuid().ToString("N"), TimeSpan.FromDays(30),
                                                                  AccessPermissions.Read).Result;
            };

        private Because of = () => MediaApi.DeleteAccessPolicy(_policy.Id, ConnectionString).Await();

        private It the_policy_should_be_removed =
            () => CloudMediaContext.AccessPolicies.Where(p => p.Id == _policy.Id).Count().ShouldEqual(0);

        private Cleanup cleanup = () =>
            {
                if (_policy != null)
                {
                    if (CloudMediaContext.AccessPolicies.Where(p => p.Id == _policy.Id).Count() == 1)
                    {
                        CloudMediaContext.AccessPolicies.Where(p => p.Id == _policy.Id)
                                              .First()
                                              .DeleteAsync()
                                              .Await();
                    }
                }
            };
    }
}
