using Autofac;
using CommonInfrastructureLibrary.Configuration;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using Edit;
using Edit.AzureTableStorage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class StreamStoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventStoreSerializer>().As<ISerializer>().SingleInstance();
            builder.RegisterModule<ConfigurationModule>();
            builder.RegisterModule<SerializationModule>();

            var table = CloudConfigurationManager.GetSetting("EventStoreTable");
            if (string.IsNullOrEmpty(table))
            {
                table = "performancetests";
            }
            builder.Register(c =>
                {
                    var cloudStorageAccount = CloudStorageAccount.Parse(c.Resolve<IStorageConfiguration>().ConnectionString);
                    return AzureTableStorageAppendOnlyStore.CreateAsync(cloudStorageAccount, table, c.Resolve<ISerializer>()).Result;
                }).As<IStreamStore>().SingleInstance();
        }
    }
}
