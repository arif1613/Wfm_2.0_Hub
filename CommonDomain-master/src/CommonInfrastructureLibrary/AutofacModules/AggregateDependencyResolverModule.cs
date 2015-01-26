using Autofac;
using EventStore;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class AggregateDependencyResolverModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutofacAggregateDependencyResolver>().As<IAggregateDependencyResolver>().SingleInstance();
        }
    }
}
