using CommonReadModelLibrary.RavenDB;
using NodaTime.TimeZones;
using Raven.Client;
using Raven.Client.Embedded;

namespace CommonReadModelLibrary.Tests
{
    public abstract class RavenTest
    {
         protected static IDocumentStore CreateDocumentStore()
         {
             var ds = new EmbeddableDocumentStore()
                 {
                     RunInMemory = true,
                     EnlistInDistributedTransactions = false
                 };
             ds.Conventions.CustomizeJsonSerializer = serializer => serializer.ConfigureForNodaTime(new DateTimeZoneCache(new BclDateTimeZoneSource()));
             ds.Initialize();

             return ds;
         }
    }
}