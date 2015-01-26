using CommonReadModelLibrary.Tests.Repository.Fakes;
using Machine.Fakes;
using Machine.Specifications;
using Raven.Client;
using System;
using System.Threading.Tasks;

namespace CommonReadModelLibrary.Tests.Repository
{
    public class when_applying_the_same_change_consecutively : WithFakes
    {
        private static IAsyncDocumentSession _session;
        private static FakeMessage _message;
        private static Action<FakeViewDocument, FakeMessage> _applyer;
        private static FakeViewDocument _viewDocument;
        private static bool _applied;

        private Establish context = () =>
        {
            _message = new FakeMessage();
            _viewDocument = new FakeViewDocument();
            _viewDocument.HandledMessages.Add(_message.MessageId);
            _session = An<IAsyncDocumentSession>();
            _session.WhenToldTo(r => r.StoreAsync(Param<FakeViewDocument>.IsAnything)).Return(Task.Factory.StartNew(() => { }));
            _session.WhenToldTo(r => r.SaveChangesAsync()).Return(Task.Factory.StartNew(() => { }));
            _session.WhenToldTo(r => r.LoadAsync<FakeViewDocument>(Param<string>.IsAnything)).Return(Task.FromResult(_viewDocument));
            _applyer = (v, m) => _applied = true;
        };

        private Because of = () =>
        {
            _session.ApplyOnce(Guid.NewGuid(), _message, _applyer, _viewDocument.GetType()).Wait();
        };

        private It does_not_apply_the_action = () =>
        {
            _applied.ShouldBeFalse();
        };

        private It does_not_save_the_changes = () =>
        {
            _session.WasNotToldTo(r => r.StoreAsync(_viewDocument));
            _session.WasNotToldTo(r => r.SaveChangesAsync());
        };

        private It does_not_register_the_message_as_handled = () =>
        {
            _viewDocument.HandledMessages.Count.ShouldEqual(1);
        };
    }
}