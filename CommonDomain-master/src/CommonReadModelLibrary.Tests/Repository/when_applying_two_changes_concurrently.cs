using CommonReadModelLibrary.Tests.Repository.Fakes;
using Machine.Specifications;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using Raven.Client.Embedded;
using System;

namespace CommonReadModelLibrary.Tests.Repository
{
    public class when_applying_two_changes_concurrently
    {
        private Establish context = () =>
            {
                _documentStore = new EmbeddableDocumentStore
                    {
                        RunInMemory = true,
                        EnlistInDistributedTransactions = false
                    };
                _documentStore.Initialize();
                _session1 = _documentStore.OpenAsyncSession();
                _session1.Advanced.UseOptimisticConcurrency = true;
                _session2 = _documentStore.OpenAsyncSession();
                _session2.Advanced.UseOptimisticConcurrency = true;
                _id = Guid.NewGuid();
                _session1.StoreAsync(new FakeViewDocument
                    {
                        Id = _id.AsId(typeof(FakeViewDocument))
                    }).Await();
                _session1.SaveChangesAsync().Await();
            };

        private Because of = () =>
            {
                FakeViewDocument doc1 = _session1.LoadAsync<FakeViewDocument>(_id.AsId(typeof(FakeViewDocument))).Await();
                FakeViewDocument doc2 = _session2.LoadAsync<FakeViewDocument>(_id.AsId(typeof(FakeViewDocument))).Await();

                doc1.Deleted = true;
                doc2.Deleted = true;

                _session1.SaveChangesAsync().Await();
                _exception = Catch.Exception(() => _session2.SaveChangesAsync().Await());

                // This will cause concurrency exception because we have already staged
                // an old copy of the document within the session
                _session2.ApplyOnce<FakeViewDocument, FakeMessage>(_id, new FakeMessage(), (d, m) =>
                    {
                        d.Set(m, s => s.Deleted, false);
                    }, doc1.GetType()).Await();
            };

        private It throws_concurrency_exception = () =>
            {
                _exception.ShouldBeOfType<ConcurrencyException>();
            };

        private It retries_when_concurrency_exception_is_thrown = () =>
            {
                _session1.Advanced.Clear();
                var doc = _session1.LoadAsync<FakeViewDocument>(_id.AsId(typeof(FakeViewDocument))).Await().AsTask.Result;
                doc.Deleted.ShouldBeFalse();
            };

        private static EmbeddableDocumentStore _documentStore;
        private static IAsyncDocumentSession _session1;
        private static IAsyncDocumentSession _session2;
        private static Guid _id;
        private static Exception _exception;
    }
}