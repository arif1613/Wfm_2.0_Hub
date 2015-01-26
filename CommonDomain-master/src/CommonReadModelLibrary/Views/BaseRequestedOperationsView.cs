using System;
using System.Threading.Tasks;
using CommonDomainLibrary;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Models;
using Raven.Client;

namespace CommonReadModelLibrary.Views
{
    public class BaseRequestedOperationsView
    {
        protected readonly IAsyncDocumentSession _session;

        public BaseRequestedOperationsView(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task CreateOperation<TMessage>(TMessage e, Type viewType) where TMessage : IMessage
        {
            await CreateOperation(e, RequestedOperationStatuses.Running.ToString(), viewType);
        }

        public async Task CreateOperation<TMessage>(TMessage e, string statusMessage, Type viewType)
            where TMessage : IMessage
        {
            await _session.ApplyOnce<RequestedOperation, TMessage>(e.CorrelationId, e, (doc, m) =>
            {
                doc.Set(m, d => d.Status, statusMessage);
                doc.Set(m, d => d.UserId, m.Sender().Id);
                doc.Set(m, d => d.HolderId, m.Sender().OwnerId);

                var ev = m as IEvent;
                var cmd = m as ICommand;

                if (ev != null)
                {
                    doc.Set(m, d => d.AggregateId, ev.Id);
                }
                else if (cmd != null)
                {
                    doc.Set(m, d => d.AggregateId, cmd.Id);
                }
            }, viewType);
        }

        public async Task SucceedOperation<TMessage>(TMessage e, Type viewType)
            where TMessage : IMessage
        {
            await SucceedOperation(e, RequestedOperationStatuses.Completed.ToString(), viewType);
        }

        public async Task SucceedOperation<TMessage>(TMessage e, string statusMessage, Type viewType)
            where TMessage : IMessage
        {
            await _session.ApplyOnce<RequestedOperation, TMessage>(e.CorrelationId, e, (doc, m) =>
            {
                doc.Set(m, d => d.Completed, true);
                doc.Set(m, d => d.Failed, false);
                doc.Set(m, d => d.Status, statusMessage);
            }, viewType);
        }

        public async Task FailOperation<TMessage>(TMessage e, Type viewType) where TMessage : IErrorEvent
        {
            await FailOperation(e, RequestedOperationStatuses.Failed.ToString(), viewType);
        }

        public async Task FailOperation<TMessage>(TMessage e, string statusMessage, Type viewType)
            where TMessage : IErrorEvent
        {
            await _session.ApplyOnce<RequestedOperation, TMessage>(e.CorrelationId, e, (doc, m) =>
            {
                doc.Set(m, d => d.Completed, true);
                doc.Set(m, d => d.Failed, true);
                doc.Set(m, d => d.Status, statusMessage);
                doc.Set(m, d => d.ErrorMessage, m.ErrorMessage);
            }, viewType);
        }

        public async Task UpdateOperation<TMessage>(TMessage e, string statusMessage, Type viewType)
            where TMessage : IMessage
        {
            await _session.ApplyOnce<RequestedOperation, TMessage>(e.CorrelationId, e, (doc, m) => 
                doc.Set(m, d => d.Status, statusMessage), viewType);
        }
    }
}
