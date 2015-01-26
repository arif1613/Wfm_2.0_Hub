using System;
using System.Threading.Tasks;
using CommonDomainLibrary;
using CommonReadModelLibrary.Models;
using CommonTestingFramework;
using Machine.Fakes;
using Machine.Specifications;
using NodaTime;
using Raven.Client;

namespace CommonReadModelLibrary.Tests.BaseRequestedOperationsView
{
    class when_updating_a_requested_operation : WithFakes
    {
        private sealed class RequestedOperationsView : Views.BaseRequestedOperationsView
        {
            public RequestedOperationsView(IAsyncDocumentSession session) : base(session)
            {
            }
        }

        private sealed class Message : IMessage
        {
            public Guid CausationId { get; set; }
            public Guid MessageId { get; set; }
            public Guid CorrelationId { get; set; }
            public Instant Timestamp { get; set; }
        }

        protected static RequestedOperation _document;
        private static RequestedOperationsView _view;
        private static IAsyncDocumentSession _session;
        private static Guid _correlationId;
        private static Message _message;
        private static string _newStatus;

        private Establish context = () =>
        {
            _correlationId = Guid.NewGuid();

            _document = new RequestedOperation
            {
                Completed = false,
                Status = "Doing something",
                Id = _correlationId.AsId(typeof(RequestedOperation))
            };

            _newStatus = "Completed doing something, doing something else";

            _session = An<IAsyncDocumentSession>();
            _session = An<IAsyncDocumentSession>();
            _session.WhenToldTo(r => r.StoreAsync(Param<RequestedOperation>.IsAnything)).Return(Task.Factory.StartNew(() => { }));
            _session.WhenToldTo(r => r.SaveChangesAsync()).Return(Task.Factory.StartNew(() => { }));
            _session.WhenToldTo(r => r.LoadAsync<RequestedOperation>(_correlationId.AsId(typeof (RequestedOperation))))
                .Return(Task.FromResult(_document));

            _view = new RequestedOperationsView(_session);

            _message = new Message
            {
                CorrelationId = _correlationId
            };
        };

        private Because of = () => _view.UpdateOperation(_message, _newStatus, typeof (RequestedOperationsView)).Await();

        private Behaves_like<behaves_like_a_running_operation> operation_is_running;

        private It the_status_is_updated = () => _document.Status.ShouldEqual(_newStatus);
    }
}
