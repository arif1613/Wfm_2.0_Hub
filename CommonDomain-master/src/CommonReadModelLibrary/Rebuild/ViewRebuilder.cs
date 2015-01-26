using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Security.Models;
using CommonReadModelLibrary.Support;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Indexes;

namespace CommonReadModelLibrary.Rebuild
{
    public class ViewRebuilder : IViewRebuilder
    {
        private sealed class AllDocumentsByViewType : AbstractIndexCreationTask
        {
            public override IndexDefinition CreateIndexDefinition()
            {
                return
                    new IndexDefinition
                    {
                        Name = "AllDocumentsByViewType",
                        Map = "from doc in docs " +
                              "let DocId = doc[\"@metadata\"][\"@id\"] " +
                              "select new {DocId};"
                    };
            }
        }

        private readonly IAsyncDocumentSession _session;
        private readonly ISupportService _supportService;

        public ViewRebuilder(IAsyncDocumentSession session, ISupportService supportService)
        {
            _session = session;
            _supportService = supportService;
        }

        public async Task RebuildView(Type viewType, ICommonIdentity identity)
        {
            //get client ID and authorization key, needed to make request to support service
            if(identity == null)
                throw new ArgumentNullException("identity", "Identity cannot be null.");

            var ownerId = identity.OwnerId.AsId(typeof(HolderWithClients));

            var client = (await _session.Query<HolderClientCredentials>()
                                        .Where(cc => cc.OwnerDocumentId.Equals(ownerId))
                                        .ToListAsync())
                                        .Single();

            if(client == null)
                throw new SystemException("Failed to get client credentials");

            var authenticationKey = client.AuthenticationKey;
            var clientId = client.Id.AsGuid();

            //first we remove all documents from the view
            var docs = await _session.Advanced.AsyncLuceneQuery<dynamic>().WhereEquals("@metadata.ViewType", viewType.FullName).ToListAsync();
            foreach (var doc in docs.Item2)
                _session.Delete(doc);
            await _session.SaveChangesAsync();

            //we rebuild the view
            var handlerInterfaces = viewType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>)).ToList();
            var messageTypes = handlerInterfaces.Select(handlerInterface => handlerInterface.GetGenericArguments()[0]).ToList();

            var archivedMessages = _supportService.GetArchivedMessages(messageTypes, identity, clientId, authenticationKey).ToList();

            var sessionWrapper = new InMemorySessionWrapper(_session);
            var viewInstance = Activator.CreateInstance(viewType, new object[] { sessionWrapper });

            foreach (var archivedMessage in archivedMessages)
            {
                var message = archivedMessage.Message;
                
                var handleMethod =
                    viewInstance.GetType()
                        .GetTypeInfo()
                        .GetDeclaredMethods("Handle")
                        .First(m => m.GetParameters()[0].ParameterType == message.GetType());

                await (Task)handleMethod.Invoke(viewInstance, new object[] { message, true });
            }

            await sessionWrapper.SaveAllDocuments();
        }
    }
}
