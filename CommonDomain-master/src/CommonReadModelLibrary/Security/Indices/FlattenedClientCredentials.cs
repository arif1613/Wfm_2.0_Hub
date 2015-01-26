using System.Collections.Generic;
using System.Linq;
using CommonReadModelLibrary.Security.Models;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace CommonReadModelLibrary.Security.Indices
{
    public class FlattenedClientCredentials : AbstractIndexCreationTask<UserWithHolder>
    {
        public FlattenedClientCredentials()
        {
            Map = users => from user in users
                           let holder = LoadDocument<HolderWithClients>(user.OwnerDocumentId)
                           from client in holder.Clients
                           from childHolder in Recurse(holder, c => c.Children.Select(s => LoadDocument<HolderWithClients>(s))).SelectMany(c => c.Children).Union(new List<string>(){user.OwnerDocumentId})
                           where user.Deleted == false && holder.Deleted == false
                           select new
                           {
                               HolderId = childHolder,
                               ClientId = client,
                               UserId = user.Id,
                               Username = user.Username,
                               OwnerId = user.OwnerDocumentId,
                               Password = user.Password,
                               UserRoles = user.Roles
                           };
            TransformResults = (db, users) => from user in users
                                              let holder = db.Load<HolderWithClients>(user.OwnerDocumentId)
                                              from client in holder.Clients
                                              from childHolder in Recurse(holder, c => c.Children.Select(s => db.Load<HolderWithClients>(s))).SelectMany(c => c.Children).Union(new List<string>() { user.OwnerDocumentId })
                                              select new ClientCredentials
                                              {
                                                  HolderId = childHolder,
                                                  ClientId = client,
                                                  UserId = user.Id,
                                                  Username = user.Username,
                                                  AuthenticationKey = db.Load<HolderClientCredentials>(client).AuthenticationKey,
                                                  OwnerId = user.OwnerDocumentId,
                                                  Roles = user.Roles,
                                                  Password = user.Password
                                              };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}