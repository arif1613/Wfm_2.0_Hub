using Autofac;
using CommonDomainLibrary.Security;
using CommonInfrastructureLibrary.RavenDB;
using CommonReadModelLibrary;
using CommonReadModelLibrary.Http;
using CommonReadModelLibrary.Rebuild;
using CommonReadModelLibrary.Support;
using Microsoft.WindowsAzure;
using NodaTime.TimeZones;
using Raven.Client;
using Raven.Client.Document;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class ReadModelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
                {
                    var ds = new DocumentStore
                    {
                        Url = CloudConfigurationManager.GetSetting("RavenDbUrl"),
                        EnlistInDistributedTransactions = false,
                        Conventions =
                        {
                            CustomizeJsonSerializer =
                                serializer =>
                                    serializer.ConfigureForNodaTime(new DateTimeZoneCache(new BclDateTimeZoneSource()))
                        }
                    };
                    ds.Initialize();

                    return ds;
                }).SingleInstance();

            builder.Register(c =>
                {
                    var session = c.Resolve<DocumentStore>().OpenAsyncSession();
                    session.Advanced.UseOptimisticConcurrency = true;

                    return session;
                });

            builder.Register(c => new CryptoProvider()).As<ICryptoProvider>();

            builder.Register(c =>
            {
                var cryptoProvider = c.Resolve<ICryptoProvider>();

                return new RequestHelper(cryptoProvider);
            }).As<IRequestHelper>().SingleInstance();

            builder.Register(c =>
            {
                var resourceUrl = CloudConfigurationManager.GetSetting("SupportResourceUrl");
                var requestHelper = c.Resolve<IRequestHelper>();

                return new SupportService(requestHelper, resourceUrl);
            }).As<ISupportService>().SingleInstance();

            builder.Register(c =>
            {
                var session = c.Resolve<IAsyncDocumentSession>();
                var supportService = c.Resolve<ISupportService>();

                return new ViewRebuilder(session, supportService);
            }).As<IViewRebuilder>().SingleInstance();
        }
    }
}
