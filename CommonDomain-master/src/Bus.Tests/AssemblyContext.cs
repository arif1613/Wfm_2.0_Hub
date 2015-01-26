using System;
using System.Configuration;
using Machine.Specifications;
using NLog.Config;

namespace Bus.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public static string ServiceBusConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["ServiceBusConnectionString"]
                                             .Replace("localhost", Environment.MachineName);
            }
        }

        public void OnAssemblyStart()
        {
            SimpleConfigurator.ConfigureForConsoleLogging(NLog.LogLevel.Off);
        }

        public void OnAssemblyComplete()
        {
        }
    }
}