using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using Edit;
using NLog;

namespace EventStore
{
    public class EventStore : IAggregateRepository
    {
        private readonly IStreamStore _storage;
        private readonly IAggregateDependencyResolver _aggregateDependencyResolver;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public EventStore(IStreamStore storage, IAggregateDependencyResolver aggregateDependencyResolver)
        {
            _storage = storage;
            _aggregateDependencyResolver = aggregateDependencyResolver;
        }

        public async Task<AggregateRepositoryResponse> GetById<T>(Guid aggregateId) where T : class, IAggregate, IMessageAccessor
        {
            var stream = await _storage.ReadAsync(typeof(T).Name + "-" + aggregateId);
            IStoredDataVersion version = null;
            if(stream!=null) version = stream.Version;
            Logger.Debug("Got aggregate from Edit");

            var ctor = typeof(T).GetConstructors().First();
            var parameters = ctor.GetParameters().ToList();
            var types = new List<object>();

            Logger.Debug("Inserting dependencies");

            foreach (var parameter in parameters)
            {
                Logger.Debug("Inserting '{0}' dependency", parameter.ParameterType.Name);
                types.Add(parameter.ParameterType.IsClass
                                   ? Activator.CreateInstance(parameter.ParameterType)
                                   : _aggregateDependencyResolver.GetDependencyInstance(parameter.ParameterType));
            }

            Logger.Debug("Building instance");
            var aggregate = (T)ctor.Invoke(types.ToArray());

            Logger.Debug("Applying events");
            if (stream!=null && stream.Chunks.Any())
            {                
                foreach (var e in stream.Chunks)
                {
                    aggregate.Raise(e.Instance);
                }                
            }

            Logger.Debug("Returning aggregate");
            return new AggregateRepositoryResponse(aggregate, version);
        }

        public async Task Save<T>(Guid causationId, T aggregate, IStoredDataVersion version) where T : class, IAggregate, IMessageAccessor
        {
            await _storage.WriteAsync(typeof(T).Name + "-" + aggregate.Id, aggregate.Messages.GetMessages().Select(e => new Chunk()
            {
                Instance = e
            }), version);
        }
    }
}
