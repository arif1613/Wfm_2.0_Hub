using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using Edit;
using Edit.AzureTableStorage;
using Edit.JsonNet;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace EventStore.Tests
{
    public class EventStoreTestsBase
    {
        protected static EventStore Store;
        private static IStreamStore _storage;
        protected static IEnumerable<IEvent> StoredEvents
        {
            get
            {
                var result = _storage.ReadAsync(StreamId).Result;

                return new List<IEvent>(result.Chunks.Where(c => c.Instance is IEvent).Select(chunk => chunk.Instance as IEvent));
            }
        }

        protected static IEnumerable<IMessage> StoredCommands
        {
            get
            {
                var result = _storage.ReadAsync(StreamId).Result;

                return new List<IMessage>(result.Chunks.Where(c => c.Instance is IMessage && !(c.Instance is IEvent)).Select(chunk => chunk.Instance as IMessage));
            }
        }

        protected static IEnumerable<DeferrableMessage> StoredDeferredCommands
        {
            get
            {
                var result = _storage.ReadAsync(StreamId).Result;

                return new List<DeferrableMessage>(result.Chunks.Where(c => c.Instance.GetType() == typeof(DeferrableMessage)).Select(chunk => chunk.Instance as DeferrableMessage));
            }
        }

        protected static string StreamId { get; set; }

        protected EventStoreTestsBase()
        {
            _storage = WireupEventStoreAsync().Result;
            Store = new EventStore(_storage, new TestAggregateDependencyResolver());
        }

        private async Task<IStreamStore> WireupEventStoreAsync()
        {
            //return new InMemoryStreamStore();
            var cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorageAcountConnectionString"]);

            return await AzureTableStorageAppendOnlyStore.CreateAsync(cloudStorageAccount, "performancetests", new JsonNetSerializer(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects, DateFormatHandling = DateFormatHandling.IsoDateFormat }));
        }

        protected static void Given(Guid aggregateId, IEnumerable<IEvent> events)
        {
            StreamId = "TestAggregate-" + aggregateId;

            var chunkSet = new ChunkSet(events.Select(e => new Chunk()
                {
                    Instance = e
                }), null);

            _storage.WriteAsync(StreamId, chunkSet.Chunks, null).Wait();
        }
    }
}
