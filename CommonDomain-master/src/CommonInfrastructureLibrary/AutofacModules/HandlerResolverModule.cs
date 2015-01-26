using Autofac;
using CommonDomainLibrary.Common;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class HandlerResolverModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutofacHandlerResolver>().As<IHandlerResolver>();
        }
    }
}
