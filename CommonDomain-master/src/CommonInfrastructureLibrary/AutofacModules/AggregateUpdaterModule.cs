using Autofac;
using CommonDomainLibrary.Common;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class AggregateUpdaterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<EventStoreModule>();

            builder.RegisterType<DefaultAggregateUpdater>().As<IAggregateUpdater>();
        }
    }
}
