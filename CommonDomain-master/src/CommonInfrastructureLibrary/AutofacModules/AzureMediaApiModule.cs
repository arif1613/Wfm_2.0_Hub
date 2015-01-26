using Autofac;
using WamsApi;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class AzureMediaApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AzureMediaApi>().As<IAzureMediaApi>();
        }
    }
}