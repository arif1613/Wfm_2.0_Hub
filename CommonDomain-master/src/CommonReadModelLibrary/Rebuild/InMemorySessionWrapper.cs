using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommonReadModelLibrary.Models;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using Raven.Json.Linq;

namespace CommonReadModelLibrary.Rebuild
{
    public class InMemorySessionWrapper : IAsyncDocumentSession
    {
        private sealed class DocumentInfo
        {
            public DocumentInfo(IViewDocument document, Guid eTag, string id)
            {
                Document = document;
                ETag = ETag;
                ID = id;
                MetaData = new RavenJObject();
            }

            public bool HasID
            {
                get { return !string.IsNullOrEmpty(ID); }
            }

            public bool HasETag
            {
                get { return !ETag.Equals(default(Guid)); }
            }

            public IViewDocument Document { get; set; }
            public Guid ETag { get; set; }
            public string ID { get; set; }
            public RavenJObject MetaData { get; set; }
        }

        private readonly IAsyncDocumentSession _session;
        private readonly List<DocumentInfo> _documents;
        private readonly InMemoryAdvancedSessionOperationsWrapper _advanced;

        public InMemorySessionWrapper(IAsyncDocumentSession session)
        {
            _session = session;
            _advanced = new InMemoryAdvancedSessionOperationsWrapper(session.Advanced, this);
            _documents = new List<DocumentInfo>();
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        public IAsyncLoaderWithInclude<object> Include(string path)
        {
            return _session.Include(path);
        }

        public IAsyncLoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return _session.Include(path);
        }

        public IAsyncLoaderWithInclude<T> Include<T, TInclude>(Expression<Func<T, object>> path)
        {
            return _session.Include(path);
        }

        public Task StoreAsync(object entity, Guid etag)
        {
            return StoreAsync(entity, etag, null);
        }

        public Task StoreAsync(object entity)
        {
            return StoreAsync(entity, default(Guid), null);
        }

        public Task StoreAsync(object entity, Guid etag, string id)
        {
            return Task.Run(() => _documents.Add(new DocumentInfo(entity as IViewDocument, etag, id)));
        }

        public Task StoreAsync(object entity, string id)
        {
            return StoreAsync(entity, default(Guid), id);
        }

        public void Delete<T>(T entity)
        {
            _session.Delete(entity);
        }

        public Task<T> LoadAsync<T>(string id)
        {
            return Task.FromResult((T)_documents.Select(di => di.Document).FirstOrDefault(d => d.Id.Equals(id) && d is T));
        }

        public Task<T[]> LoadAsync<T>(params string[] ids)
        {
            return _session.LoadAsync<T>(ids);
        }

        public Task<T[]> LoadAsync<T>(IEnumerable<string> ids)
        {
            return _session.LoadAsync<T>(ids);
        }

        public Task<T> LoadAsync<T>(ValueType id)
        {
            return _session.LoadAsync<T>(id);
        }

        public Task<T[]> LoadAsync<T>(params ValueType[] ids)
        {
            return _session.LoadAsync<T>(ids);
        }

        public Task<T[]> LoadAsync<T>(IEnumerable<ValueType> ids)
        {
            return _session.LoadAsync<T>(ids);
        }

        public Task SaveChangesAsync()
        {
            return Task.Run(() => { });
        }

        public IRavenQueryable<T> Query<T>(string indexName, bool isMapReduce = false)
        {
            return _session.Query<T>(indexName, isMapReduce);
        }

        public IRavenQueryable<T> Query<T>()
        {
            return _session.Query<T>();
        }

        public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
        {
            return _session.Query<T, TIndexCreator>();
        }

        public IAsyncAdvancedSessionOperations Advanced
        {
            get { return _advanced; }
        }

        public async Task SaveAllDocuments()
        {
            foreach (var documentInfo in _documents)
            {
                if (!documentInfo.HasETag && !documentInfo.HasID)
                    await _session.StoreAsync(documentInfo.Document);

                else if (documentInfo.HasETag && !documentInfo.HasID)
                    await _session.StoreAsync(documentInfo.Document, documentInfo.ETag);

                else if (!documentInfo.HasETag)
                    await _session.StoreAsync(documentInfo.Document, documentInfo.ID);

                else 
                    await _session.StoreAsync(documentInfo.Document, documentInfo.ETag, documentInfo.ID);

                var metadata = _session.Advanced.GetMetadataFor(documentInfo.Document);
                foreach (var value in documentInfo.MetaData)
                {
                    metadata[value.Key] = value.Value;
                }
            }

            await _session.SaveChangesAsync();
        }

        public RavenJObject GetMetadataFor(object instance)
        {
            return _documents.First(di => di.Document == instance).MetaData;
        }
    }
}
