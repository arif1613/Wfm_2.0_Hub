using System;
using Autofac;
using CommonDomainLibrary.Security;
using CommonWebServiceLibrary.Security;
using Machine.Specifications;
using Nancy;
using Nancy.Security;
using Nancy.Testing;

namespace CommonWebServiceLibrary.Tests.Security
{
    public class when_receiving_a_request_with_the_wrong_authentication
    {
        private static Browser _browser;
        private static BrowserResponse _result;
        private static SecurityTestBootstrapper _bootstrapper;

        private class TestModule: NancyModule
        {
            public TestModule()
            {
                this.RequiresAuthentication();

                Get["/testModule", true] = async (a, token) =>
                    {
                        return 200;
                    };
            }  
        }

        private Establish context = () =>
        {
            _bootstrapper = new SecurityTestBootstrapper();
            _bootstrapper.TestModules.Add(new TestModule());

            _browser = new Browser(_bootstrapper);
        };

        private Because of = () =>
            {
                _result = _browser.Get("/testModule", ww =>
                    {
                        ww.HttpRequest();

                        var authenticationMac = new AuthenticationMac("123", Guid.NewGuid(), 123, "123", "method",
                                                                      "resource", _bootstrapper.Container.Resolve<ICryptoProvider>(), new byte[]{});

                        ww.Header("Authorization",string.Concat("MAC ", authenticationMac.ToString()));
                    });
            };

        private It the_result_should_be_unauthorized = () => _result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
    }
}
