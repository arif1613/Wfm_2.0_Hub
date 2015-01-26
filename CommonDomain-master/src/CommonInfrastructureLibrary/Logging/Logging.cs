using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using NLog;
using NLog.Config;

namespace CommonInfrastructureLibrary.Logging
{
    public static class Logging
    {
        public static void Configure()
        {
            if (RoleEnvironment.IsAvailable)
            {
                var environment = CloudConfigurationManager.GetSetting("Environment");
                var role = RoleEnvironment.CurrentRoleInstance.Role.Name.Replace(".", "-").ToLower();
                var machine = string.Concat(Dns.GetHostName(), "-", environment).ToLower();

                var target = new SyslogTarget
                    {
                        Name = "Syslog",
                        Sender = role,
                        Machine = machine,
                        SyslogServer = CloudConfigurationManager.GetSetting("SyslogServer"),
                        Port = int.Parse(CloudConfigurationManager.GetSetting("SyslogPort")),
                        Layout = CloudConfigurationManager.GetSetting("LogLayout")
                    };

                var config = new LoggingConfiguration();
                config.AddTarget(target.Name, target);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, target));

                LogManager.Configuration = config;
            }
            else
            {
                SimpleConfigurator.ConfigureForConsoleLogging();
            }
        }
    }
}