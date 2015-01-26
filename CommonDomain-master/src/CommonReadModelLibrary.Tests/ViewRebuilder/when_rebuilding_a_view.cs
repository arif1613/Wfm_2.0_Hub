using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Models;
using CommonReadModelLibrary.RavenDB;
using CommonReadModelLibrary.Security.Models;
using CommonReadModelLibrary.Support;
using CommonReadModelLibrary.Tests.ViewRebuilder.Shared;
using FakeItEasy;
using Machine.Specifications;
using NodaTime;
using NodaTime.TimeZones;
using Raven.Client;
using Raven.Client.Embedded;

namespace CommonReadModelLibrary.Tests.ViewRebuilder
{
    public class when_rebuilding_a_view
    {
        public class MyMessage1 : IMessage
        {
            public Guid CausationId { get; set; }
            public Guid MessageId { get; set; }
            public Guid CorrelationId { get; set; }
            public Instant Timestamp { get; set; }

            public MyMessage1()
            {
                CausationId = Guid.NewGuid();
                MessageId = Guid.NewGuid();
            }
        };

        public class MyView : IHandle<MyMessage1>
        {
            public MyView(IAsyncDocumentSession session)
            {

            }

            public MyView() { }

            public async Task Handle(MyMessage1 e, bool lastTry)
            {
                await Task.Run(() =>
                {
                    _handledMessage = e;
                });
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

        private static IAsyncDocumentSession _session;
        private static ISupportService _supportService;
        private static Rebuild.ViewRebuilder _rebuilder;
        private static MyMessage1 _message;
        private static object _oldDocument;
        private static string _oldId;
        private static Guid _ownerId;
        private static MyMessage1 _handledMessage;

        private Establish context = () =>
        {
            _ownerId = Guid.NewGuid();

            _message = new MyMessage1();
            _session = A.Fake<IAsyncDocumentSession>();
            _oldId = Guid.NewGuid().ToString();
            _oldDocument = new
            {
                Id = _oldId,
                Name = "someName"
            };

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

            _session.StoreAsync(_oldDocument).Await();
            _session.StoreAsync(new HolderClientCredentials
            {
                AuthenticationKey = Encoding.UTF8.GetBytes("someKey"),
                OwnerDocumentId = _ownerId.AsId(typeof(HolderWithClients))
            }).Await();

            _session.Advanced.GetMetadataFor(_oldDocument)["ViewType"] = typeof(MyView).FullName;
            _session.SaveChangesAsync().Await();

            _supportService = new MockSupportService
            {
                ArchivedMessages = new List<ArchivedMessage>
                {
                    new ArchivedMessage
                    {
                        Id = _message.MessageId,
                        Message = _message,
                        MessageType = _message.GetType()
                    }
                }
            };

            _rebuilder = new Rebuild.ViewRebuilder(_session, _supportService);

        };

        private Because of = () => _rebuilder.RebuildView(typeof(MyView), new Identity { OwnerId = _ownerId }).Await();

        private It the_view_documents_should_be_cleared_before_rebuild = () =>
            _session.LoadAsync<object>(_oldId).Result.ShouldBeNull();

        private It the_handle_methods_for_the_replayed_messages_should_be_called =
            () => _handledMessage.ShouldEqual(_message);
    }
}