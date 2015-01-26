using Autofac;
using CommonDomainLibrary.Common;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class EventStoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<StreamStoreModule>();
            builder.RegisterType<EventStore.EventStore>().As<IAggregateRepository>().SingleInstance();
        }
    }
}
