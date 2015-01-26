using Machine.Specifications;
using NLog.Config;

namespace CommonReadModelLibrary.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            SimpleConfigurator.ConfigureForConsoleLogging(NLog.LogLevel.Off);
        }

        public void OnAssemblyComplete()
        {
        }
    }
}