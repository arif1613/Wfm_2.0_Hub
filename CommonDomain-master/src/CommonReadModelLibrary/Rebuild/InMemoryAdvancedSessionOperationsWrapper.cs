using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Abstractions.Commands;
using Raven.Client;
using Raven.Json.Linq;

namespace CommonReadModelLibrary.Rebuild
{
    class InMemoryAdvancedSessionOperationsWrapper : IAsyncAdvancedSessionOperations
    {
        private readonly IAsyncAdvancedSessionOperations _advanced;
        private readonly InMemorySessionWrapper _owner;

        public InMemoryAdvancedSessionOperationsWrapper(IAsyncAdvancedSessionOperations advanced,
            InMemorySessionWrapper owner)
        {
            _advanced = advanced;
            _owner = owner;
        }

        public bool IsLoaded(string id)
        {
            return _advanced.IsLoaded(id);
        }

        public void Evict<T>(T entity)
        {
            _advanced.Evict(entity);
        }

        public void Clear()
        {
            _advanced.Clear();
        }

        public void MarkReadOnly(object entity)
        {
            _advanced.MarkReadOnly(entity);
        }

        public RavenJObject GetMetadataFor<T>(T instance)
        {
            return _owner.GetMetadataFor(instance);
        }

        public Guid? GetEtagFor<T>(T instance)
        {
            return _advanced.GetEtagFor(instance);
        }

        public string GetDocumentId(object entity)
        {
            return _advanced.GetDocumentId(entity);
        }

        public bool HasChanged(object entity)
        {
            return _advanced.HasChanged(entity);
        }

        public void Defer(params ICommandData[] commands)
        {
            _advanced.Defer(commands);
        }

        public IDocumentStore DocumentStore
        {
            get { return _advanced.DocumentStore; }
        }

        public string StoreIdentifier
        {
            get { return _advanced.StoreIdentifier; }
        }

        public bool UseOptimisticConcurrency
        {
            get { return _advanced.UseOptimisticConcurrency; }
            set { _advanced.UseOptimisticConcurrency = value; }
        }

        public IDictionary<string, object> ExternalState
        {
            get { return _advanced.ExternalState; }
        }

        public bool AllowNonAuthoritativeInformation
        {
            get { return _advanced.AllowNonAuthoritativeInformation; }
            set { _advanced.AllowNonAuthoritativeInformation = value; }
        }

        public TimeSpan NonAuthoritativeInformationTimeout
        {
            get { return _advanced.NonAuthoritativeInformationTimeout; }
            set { _advanced.NonAuthoritativeInformationTimeout = value; }
        }

        public int MaxNumberOfRequestsPerSession
        {
            get { return _advanced.MaxNumberOfRequestsPerSession; }
            set { _advanced.MaxNumberOfRequestsPerSession = value; }
        }

        public int NumberOfRequests
        {
            get { return _advanced.NumberOfRequests; }
        }

        public bool HasChanges
        {
            get { return _advanced.HasChanges; }
        }

        public Task<IEnumerable<T>> LoadStartingWithAsync<T>(string keyPrefix, int start = 0, int pageSize = 25)
        {
            return _advanced.LoadStartingWithAsync<T>(keyPrefix, start, pageSize);
        }

        public IAsyncDocumentQuery<T> AsyncLuceneQuery<T>(string index, bool isMapReduce = false)
        {
            return _advanced.AsyncLuceneQuery<T>(index, isMapReduce);
        }

        public IAsyncDocumentQuery<T> AsyncLuceneQuery<T>()
        {
            return _advanced.AsyncLuceneQuery<T>();
        }

        public void Store(object entity, Guid etag)
        {
            _advanced.Store(entity, etag);
        }

        public void Store(object entity)
        {
            _advanced.Store(entity);
        }

        public void Store(object entity, Guid etag, string id)
        {
            _advanced.Store(entity, etag, id);
        }

        public void Store(object entity, string id)
        {
            _advanced.Store(entity, id);
        }
    }
}
