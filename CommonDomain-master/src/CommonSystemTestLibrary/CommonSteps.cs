using CommonSystemTestLibrary.Context;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace CommonSystemTestLibrary
{
    [Binding]
    public class CommonSteps
    {
        [Given(@"a user with the role (.*)")]
        public static void GivenAUserWithTheRole(string role)
        {
            ExtendedScenarioContext.CurrentUser = SystemTestContext.Instance.GetUserWithRole(role);
        }

        [Then(@"the requested operation is updated to ""(.*)""")]
        public void ThenTheRequestedOperationIsUpdatedTo(string status)
        {
            Assert.AreEqual(status, ExtendedScenarioContext.RequestedOperation.Status);
        }

        [Then(@"a|an ""(.*)"" exception is thrown")]
        public static void ThenAnExceptionIsThrown(string status)
        {
            var errorMessage = string.Format("Expected web exception message to contain '{0}' but was '{1}'", status,
                ExtendedScenarioContext.WebException.Message);

            Assert.IsTrue(ExtendedScenarioContext.WebException.Message.Contains(status), errorMessage);
        }

        [AfterScenario]
        public static void Cleanup()
        {
            ExtendedScenarioContext.Cleanup();
        }
    }
}
