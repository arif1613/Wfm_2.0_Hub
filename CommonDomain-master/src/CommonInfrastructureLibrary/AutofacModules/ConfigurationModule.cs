using Autofac;
using CommonInfrastructureLibrary.Configuration;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class ConfigurationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(GetType().Assembly)
                   .InNamespaceOf<IStorageConfiguration>()
                   .AsImplementedInterfaces();
        }
    }
}