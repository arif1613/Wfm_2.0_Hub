using System;
using Microsoft.WindowsAzure;

namespace CommonInfrastructureLibrary.Configuration
{
    public class BusConfiguration : IBusConfiguration
    {
        public string ConnectionString
        {
            get
            {
                //var connBuilder = new ServiceBusConnectionStringBuilder
                //{
                //    ManagementPort = 9355,
                //    RuntimePort = 9354
                //};

                //connBuilder.Endpoints.Add(new UriBuilder() { Scheme = "sb", Host = "localhost", Path = "ServiceBusDefaultNamespace" }.Uri);
                //connBuilder.StsEndpoints.Add(new UriBuilder() { Scheme = "https", Host = "localhost", Port = 9355, Path = "ServiceBusDefaultNamespace" }.Uri);

                //var connString = connBuilder.ToString();

                //return connString;

                //return
                //    "Endpoint=sb://localhost/ServiceBusDefaultNamespace;StsEndpoint=https://localhost:9355/ServiceBusDefaultNamespace;RuntimePort=9354;ManagementPort=9355"; // local

                //return
                //    "Endpoint=sb://ats-test.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=CVQEW67CMQMzrIbybba1vPbLn1Ni4ZkeUG/VBuX0BAA="; //Production

                //return
                //    "Endpoint=sb://kumo2-test.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=8BgefAtjNPzLb+M28kQMmSfYqT/zE9XTN75CrWsHZqY="; //Test

                var azureBusConnectionString = CloudConfigurationManager.GetSetting("AzureBusConnectionString").Replace("localhost", Environment.MachineName);

                if (azureBusConnectionString.Equals("managed-by-environment-variable"))
                    azureBusConnectionString = Environment.GetEnvironmentVariable("AzureBusConnectionString");

                return azureBusConnectionString;
            }
        }
    }
}
