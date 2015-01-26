using Autofac;
using CommonDomainLibrary;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class ConnectionClientServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConnectionClientService>().As<IConnectionClientService>();
        }
    }
}
