using CommonReadModelLibrary.Tests.Repository.Fakes;
using Machine.Fakes;
using Machine.Specifications;
using Raven.Client;
using System;
using System.Threading.Tasks;

namespace CommonReadModelLibrary.Tests.Repository
{
    public class when_applying_a_change_for_the_first_time : WithFakes
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

        private It applies_the_action = () =>
            {
                _applied.ShouldBeTrue();
            };

        private It saves_the_changes = () =>
            {
                _session.WasToldTo(r => r.StoreAsync(_viewDocument));
                _session.WasToldTo(r => r.SaveChangesAsync());
            };

        private It registers_the_message_as_handled = () =>
            {
                _viewDocument.HandledMessages.ShouldContain(_message.MessageId);
            };
    }
}