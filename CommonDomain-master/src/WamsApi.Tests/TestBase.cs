using System;
using System.Text;
using CommonDomainLibrary;
using FakeItEasy;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class TestBase
    {
        protected static AzureMediaApi MediaApi;
        private static string AccountName;
        private static string AccountKey;
        protected static string ConnectionString;
        protected static CloudMediaContext CloudMediaContext;

        protected static string ShortName(string name)
        {
            string encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(name));
            encoded = encoded
              .Replace("/", "_")
              .Replace("+", "-");
            return encoded.Substring(0, 22);
        }

        public TestBase()
        {
            AccountName = "kumo2stage";
            AccountKey = "3h992idjHSsaGymTq1Sh73hcco/fc/TOdjy9+BlQ9sM=";
            CloudMediaContext = new CloudMediaContext(AccountName, AccountKey);
            ConnectionString = "{'account':'" + AccountName + "','key':'" + AccountKey + "'}";

            var connectionClientService = A.Fake<IConnectionClientService>();
            A.CallTo(
                () => connectionClientService.WamsClient(ConnectionString))
             .Returns(new CloudMediaContext(AccountName, AccountKey));

            MediaApi = new AzureMediaApi(connectionClientService);
        }
    }
}
