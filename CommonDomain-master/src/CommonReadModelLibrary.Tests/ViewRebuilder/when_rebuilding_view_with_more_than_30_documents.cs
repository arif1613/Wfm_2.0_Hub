using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Models;
using CommonReadModelLibrary.RavenDB;
using CommonReadModelLibrary.Rebuild;
using CommonReadModelLibrary.Security.Models;
using CommonReadModelLibrary.Tests.ViewRebuilder.Shared;
using Machine.Specifications;
using NodaTime;
using NodaTime.TimeZones;
using Raven.Client;
using Raven.Client.Embedded;

namespace CommonReadModelLibrary.Tests.ViewRebuilder
{
    public class when_rebuilding_view_with_more_than_30_documents
    {
        private sealed class CreateTestDocument : IMessage
        {
            public Guid CausationId { get; set; }
            public Guid MessageId { get; set; }
            public Guid CorrelationId { get; set; }
            public Instant Timestamp { get; set; }
            public Guid Id { get; set; }
            public string Message { get; set; }
        }

        private sealed class EditTestDocument : IMessage
        {
            public Guid CausationId { get; set; }
            public Guid MessageId { get; set; }
            public Guid CorrelationId { get; set; }
            public Instant Timestamp { get; set; }
            public Guid Id { get; set; }
            public string Message { get; set; }
        }

        private sealed class TestDocument : IViewDocument
        {
            public TestDocument()
            {
                HandledMessages = new List<Guid>();
                FieldChanges = new Dictionary<string, Instant>();
            }

            public string Id { get; set; }
            public bool Deleted { get; set; }
            public Guid HolderId { get; set; }
            public IList<Guid> HandledMessages { get; set; }
            public IDictionary<string, Instant> FieldChanges { get; set; }
            public Instant LastChangeTime { get; set; }
            public string Message { get; set; }
            public Instant CreatedDate { get; set; }
        }

        private sealed class TestView : IHandle<CreateTestDocument>,
                                        IHandle<EditTestDocument>
        {
            private readonly IAsyncDocumentSession _internalSession;

            public TestView(IAsyncDocumentSession session)
            {
                _internalSession = session;
            }

            public async Task Handle(CreateTestDocument e, bool lastTry)
            {
                await _internalSession.ApplyOnce<TestDocument, CreateTestDocument>(e.Id, e, (doc, m) =>
                {
                    doc.Set(m, d => d.Message, m.Message);
                    doc.Set(m, d => d.CreatedDate, m.Timestamp);
                }, typeof (TestView));
            }

            public async Task Handle(EditTestDocument e, bool lastTry)
            {
                await _internalSession.ApplyOnce<TestDocument, EditTestDocument>(e.Id, e, (doc, m) => 
                    doc.Set(m, d => d.Message, m.Message), typeof (TestView));
            }
        }

        private sealed class Identity : ICommonIdentity
        {
            public string Name { get; private set; }
            public string AuthenticationType { get; private set; }
            public bool IsAuthenticated { get; private set; }
            public Guid Id { get; private set; }
            public Guid OwnerId { get; set; }
            public Guid ClientId { get; set; }
        }

        private static MockSupportService _supportService;
        private static IAsyncDocumentSession _session;
        private static IViewRebuilder _rebuilder;
        private static TestView _view;

        private static Guid _ownerId;

        private static IList<TestDocument> _documents; 

        private Establish context = () =>
        {
            _ownerId = Guid.NewGuid();

            _supportService = new MockSupportService();
            for (var i = 0; i < 40; i++)
            {
                var documentId = Guid.NewGuid();

                _supportService.ArchivedMessages.Add(new ArchivedMessage
                {
                    Id = Guid.NewGuid(),
                    Message = new CreateTestDocument
                    {
                        CausationId = Guid.NewGuid(),
                        CorrelationId = Guid.NewGuid(),
                        Id = documentId,
                        Message = string.Format("This is document nr {0}", i),
                        MessageId = Guid.NewGuid(),
                        Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow)
                    },
                    MessageType = typeof(CreateTestDocument)
                });

                _supportService.ArchivedMessages.Add(new ArchivedMessage
                {
                    Id = Guid.NewGuid(),
                    Message = new EditTestDocument
                    {
                        CausationId = Guid.NewGuid(),
                        CorrelationId = Guid.NewGuid(),
                        Id = documentId,
                        Message = string.Format("Document {0} has now been edited", i),
                        MessageId = Guid.NewGuid(),
                        Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow)
                    },
                    MessageType = typeof(EditTestDocument)
                });
            }

            var ds = new EmbeddableDocumentStore
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
            ds.Initialize();

            _session = ds.OpenAsyncSession();

            _session.StoreAsync(new HolderClientCredentials
            {
                AuthenticationKey = Encoding.UTF8.GetBytes("someKey"),
                OwnerDocumentId = _ownerId.AsId(typeof(HolderWithClients))
            }).Await();

            _session.SaveChangesAsync().Await();

            _rebuilder = new Rebuild.ViewRebuilder(_session, _supportService);

            _view = new TestView(new InMemorySessionWrapper(_session));
        };

        private Because of = () =>
        {
            _rebuilder.RebuildView(_view.GetType(), new Identity {OwnerId = _ownerId}).Await();
            _documents = _session.Query<TestDocument>().ToListAsync().Result;
        };
            

        private It all_documents_are_added_to_the_view = () => _documents.Count.ShouldEqual(40);

        private It the_documents_have_their_messages_updated = () =>
        {
            var docs = _session.Query<TestDocument>().ToListAsync().Result;
            for (var i = 0; i < docs.Count; i++)
                docs[i].Message.ShouldEqual(string.Format("Document {0} has now been edited", i));
        };

        private It the_documents_have_the_correct_metadata = () =>
        {
            foreach (var metaData in _documents.Select(document => _session.Advanced.GetMetadataFor(document)))
            {
                metaData["ViewType"].ShouldEqual(typeof (TestView).FullName);
                metaData["Raven-Entity-Name"].ShouldEqual("TestDocuments");
                metaData["Raven-Clr-Type"].ShouldEqual(typeof (TestDocument).FullName + ", " +
                                                       typeof (TestDocument).Assembly.GetName().Name);
            }
        };
    }
}
