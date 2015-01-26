using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.RavenDB;
using CommonReadModelLibrary.Security.Indices;
using CommonReadModelLibrary.Security.Models;
using CommonWebServiceLibrary.Routing;
using CommonWebServiceLibrary.Security;
using CommonWebServiceLibrary.Serialization;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Testing.Fakes;
using Newtonsoft.Json;
using NodaTime.TimeZones;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace CommonWebServiceLibrary.Tests.Security
{
    public class SecurityTestBootstrapper : AutofacNancyBootstrapper
    {
        public class MyCredentialsProvider : ICredentialsProvider
        {
            public async Task<IList<ClientCredentials>> GetClientCredentials(string clientId, string username)
            {
                return ClientCredentialsList;
            }
        }

        public ILifetimeScope Container { get; private set; }
        public List<INancyModule> TestModules;
        public static List<ClientCredentials> ClientCredentialsList;
        public DocumentStore DocumentStore;

        protected override IRootPathProvider RootPathProvider
        {
            get
            {
                return new FakeRootPathProvider();
            }
        }

        public IRouteConstructor RouteConstructor;        

        public SecurityTestBootstrapper()
        {
            TestModules = new List<INancyModule>();
            ClientCredentialsList = new List<ClientCredentials>();

            DocumentStore = new EmbeddableDocumentStore
            {
                RunInMemory = true,
                EnlistInDistributedTransactions = false,
                Conventions =
                {
                    CustomizeJsonSerializer =
                        serializer =>
                        serializer.ConfigureForNodaTime(new DateTimeZoneCache(new BclDateTimeZoneSource()))
                }
            };
            DocumentStore.Initialize();
            RouteConstructor = new DefaultRouteConstructor();
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope container)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<CryptoProvider>().As<ICryptoProvider>().SingleInstance();
            builder.RegisterInstance(RouteConstructor).As<IRouteConstructor>().SingleInstance();
            builder.RegisterType<MyCredentialsProvider>().As<ICredentialsProvider>();

            builder.Register(c => JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new WebServiceContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Converters = { new GuidConverter() }
            }).ConfigureForNodaTime(new DateTimeZoneCache(new BclDateTimeZoneSource())));

            builder.RegisterInstance(DocumentStore).SingleInstance().As<DocumentStore>();
            builder.Register(c =>
            {
                var session = c.Resolve<DocumentStore>().OpenAsyncSession();
                session.Advanced.UseOptimisticConcurrency = true;

                return session;
            }).As<IAsyncDocumentSession>();

            builder.Update(container.ComponentRegistry);

            Container = container;
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
        {
            pipelines.AfterRequest.AddItemToStartOfPipeline(c =>
            {
                var routeConstructor = container.Resolve<IRouteConstructor>() as DefaultRouteConstructor;
                routeConstructor._context = c;
            });

            MacAuthentication.Enable(pipelines, new MacAuthenticationConfiguration
            {
                DocumentSession = container.Resolve<IAsyncDocumentSession>(),
                CryptoProvider = container.Resolve<ICryptoProvider>(),
                CredentialsProvider = container.Resolve<ICredentialsProvider>()
            });
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            IndexCreation.CreateIndexes(typeof(FlattenedClientCredentials).Assembly,
                                        container.Resolve<DocumentStore>());

            Cors.Enable(pipelines);
        }

        protected override void RegisterRequestContainerModules(ILifetimeScope container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            base.RegisterRequestContainerModules(container, moduleRegistrationTypes);

            var builder = new ContainerBuilder();

            foreach (var module in TestModules)
            {
                builder.RegisterType(module.GetType()).As<INancyModule>();
            }

            builder.Update(container.ComponentRegistry);

            ResourceRoutes.Configure(container.Resolve<IEnumerable<INancyModule>>());
        }
    }    
}
