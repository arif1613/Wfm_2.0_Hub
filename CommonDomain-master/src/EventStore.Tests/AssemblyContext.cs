using Machine.Specifications;
using NLog.Config;

namespace EventStore.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            SimpleConfigurator.ConfigureForConsoleLogging(NLog.LogLevel.Debug);
        }

        public void OnAssemblyComplete()
        {
        }
    }
}