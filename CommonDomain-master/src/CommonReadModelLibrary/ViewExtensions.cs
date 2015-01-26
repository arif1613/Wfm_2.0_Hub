using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonDomainLibrary;
using CommonReadModelLibrary.Models;
using NLog;
using Raven.Client;
using Raven.Abstractions.Exceptions;

namespace CommonReadModelLibrary
{
    public static class ViewExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string AsId(this Guid id, Type type)
        {
            var matches = Regex.Matches(type.Name, "[A-Z]").Cast<Match>().ToList();
            var count = matches.Count(m => m.Success);

            if (count > 1)
            {
                return string.Concat(string.Join(string.Empty, matches.Select(m => m.Groups[0].Value)).ToLower(),
                                        "_",
                                        id.ToString("n"));
            }

            return string.Concat(type.Name.Substring(0, 2).ToLower(), "_", id.ToString("n"));
        }

        public static Guid AsGuid(this string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var parts = id.Split('_');

                if (parts.Length == 2)
                {
                    Guid result;
                    if (Guid.TryParse(parts[1], out result))
                    {
                        return result;
                    }
                }
            }

            return Guid.Empty;
        }

        public async static Task ApplyOnce<TDocument, TMessage>(this IAsyncDocumentSession session,
                                                                 Guid id,
                                                                 TMessage message,
                                                                 Action<TDocument, TMessage> apply, Type viewType,
                                                                 bool save = true)
            where TDocument : class, IViewDocument, new()
            where TMessage : IMessage
        {
            Logger.Debug("Attempting to apply message {0} to {1} of id {2}.", message.MessageId, typeof(TDocument).Name, id);

            var newDocument = false;
            var key = id.AsId(typeof(TDocument));
            TDocument doc = await session.LoadAsync<TDocument>(key);
            if (doc == null)
            {
                doc = new TDocument
                {
                    Id = key
                };
                newDocument = true;
            }

            if (!doc.HandledMessages.ReverseContains(message.MessageId))
            {
                Logger.Debug("Message {0} has not been handled before, applying change.", message.MessageId);

                apply(doc, message);
                doc.HandledMessages.Add(message.MessageId);
                doc.LastChangeTime = message.Timestamp;

                Logger.Debug("Message {0} has been applied to read model.", message.MessageId);

                bool retry = false;

                try
                {
                    await session.StoreAsync(doc);

                    if (newDocument)
                    {
                        var metaData = session.Advanced.GetMetadataFor(doc);
                        metaData["ViewType"] = viewType.FullName;
                    }

                    if (save)
                    {
                        await session.SaveChangesAsync();
                    }
                }
                catch (AggregateException ex)
                {
                    if (ex.GetBaseException() is ConcurrencyException)
                    {

                        session.Advanced.Clear();
                        retry = true;

                        Logger.Debug(string.Format("({0}) ConcurrencyException when saving read model, retrying.", message.CorrelationId));
                    }
                    else
                    {
                        Logger.ErrorException(string.Format("({0}) Error saving the document to the read model store: ", message.CorrelationId), ex);
                        throw ex.GetBaseException();
                    }
                }
                catch (ConcurrencyException ex)
                {
                    session.Advanced.Clear();
                    retry = true;

                    Logger.Debug("ConcurrencyException when saving read model, retrying.", ex);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to save read model.", ex);
                    throw;
                }

                if (retry)
                {
                    await session.ApplyOnce(id, message, apply, viewType);
                }
            }
        }

        public static async Task ApplyBatch(this IAsyncDocumentSession session, Func<IAsyncDocumentSession, Task> apply)
        {
            bool retry = false;

            try
            {
                await apply(session);
                await session.SaveChangesAsync();
            }
            catch (AggregateException ex)
            {
                if (ex.GetBaseException() is ConcurrencyException)
                {

                    session.Advanced.Clear();
                    retry = true;

                    Logger.Debug("ConcurrencyException when saving read model, retrying.", ex);
                }
                else
                {
                    Logger.Error("Failed to save read model.", ex);
                    throw ex.GetBaseException();
                }
            }
            catch (ConcurrencyException ex)
            {
                session.Advanced.Clear();
                retry = true;

                Logger.Debug("ConcurrencyException when saving read model, retrying.", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save read model.", ex);
                throw;
            }

            if (retry)
            {
                await session.ApplyBatch(apply);
            }
        }

        public static void Set<TDocument, TMessage, TValue>(this TDocument document, TMessage message, Expression<Func<TDocument, TValue>> targetExpression, TValue value)
            where TDocument : IViewDocument
            where TMessage : IMessage
        {
            var targetMember = ((MemberExpression)targetExpression.Body).Member;
            var path = targetExpression.ToString().Split('.').Skip(1).ToList();
            var fullPropertyName = string.Join(".", path);

            if (!document.FieldChanges.ContainsKey(fullPropertyName) || document.FieldChanges[fullPropertyName] <= message.Timestamp)
            {
                if (path.Count > 1)
                {
                    object target = document;
                    PropertyInfo property = null;
                    foreach (var propertyName in path)
                    {
                        property = target.GetType().GetProperty(propertyName);
                        var temp = property.GetValue(target, null);
                        if (temp == null && property.PropertyType != typeof(System.Byte[]))
                        {
                            property.SetValue(target, Activator.CreateInstance(property.PropertyType));
                        }
                        else if (property.PropertyType == typeof(System.Byte[]))
                        {
                            property.SetValue(target, value);
                        }
                        if (propertyName != path.Last()) target = property.GetValue(target, null);
                    }

                    property.SetValue(target, value);
                    document.FieldChanges[fullPropertyName] = message.Timestamp;
                    document.LastChangeTime = message.Timestamp;
                }
                else
                {
                    ((PropertyInfo)targetMember).SetValue(document, value, null);
                    document.FieldChanges[fullPropertyName] = message.Timestamp;
                    document.LastChangeTime = message.Timestamp;
                }
            }
        }

        public static void Set<TDocument, TMessage, TKey, TValue>(this TDocument document, TMessage message, Expression<Func<TDocument, Dictionary<TKey, TValue>>> targetExpression, TKey key, TValue value)
            where TDocument : IViewDocument
            where TMessage : IMessage
        {
            var targetMember = (MemberExpression)targetExpression.Body;
            var targetProperty = (PropertyInfo)targetMember.Member;
            var dictionary = (IDictionary<TKey, TValue>)targetProperty.GetValue(document) ??
                             new Dictionary<TKey, TValue>();
            var fieldKey = string.Concat(targetProperty.Name, "[", key, "]");

            if (!document.FieldChanges.ContainsKey(fieldKey) || document.FieldChanges[fieldKey] <= message.Timestamp)
            {
                dictionary[key] = value;
                document.FieldChanges[fieldKey] = message.Timestamp;
                document.LastChangeTime = message.Timestamp;
            }
        }

        public static void Add<TDocument, TMessage, TValue>(this TDocument document, TMessage message, Expression<Func<TDocument, IList<TValue>>> targetExpression, TValue value)
            where TDocument : IViewDocument
            where TMessage : IMessage
        {
            var targetMember = (MemberExpression)targetExpression.Body;
            var targetProperty = (PropertyInfo)targetMember.Member;
            var list = (IList<TValue>)targetProperty.GetValue(document) ?? new List<TValue>();
            var fieldKey = string.Concat(targetProperty.Name, "[", value, "]");

            if (!document.FieldChanges.ContainsKey(fieldKey) ||
                document.FieldChanges[fieldKey] <= message.Timestamp)
            {
                list.Add(value);
                document.FieldChanges[fieldKey] = message.Timestamp;
                document.LastChangeTime = message.Timestamp;
            }

            targetProperty.SetValue(document, list, null);
        }

        public static void Remove<TDocument, TMessage, TValue>(this TDocument document, TMessage message, Expression<Func<TDocument, IList<TValue>>> targetExpression, TValue value)
            where TDocument : IViewDocument
            where TMessage : IMessage
        {
            var targetMember = (MemberExpression)targetExpression.Body;
            var targetProperty = (PropertyInfo)targetMember.Member;
            var list = (IList<TValue>)targetProperty.GetValue(document) ?? new List<TValue>();
            var fieldKey = string.Concat(targetProperty.Name, "[", value, "]");

            if (!document.FieldChanges.ContainsKey(fieldKey) ||
                document.FieldChanges[fieldKey] <= message.Timestamp)
            {
                list.Remove(value);
                document.FieldChanges[fieldKey] = message.Timestamp;
                document.LastChangeTime = message.Timestamp;
            }

            targetProperty.SetValue(document, list, null);
        }

        public static void Remove<TDocument, TMessage, TKey, TValue>(this TDocument document, TMessage message, Expression<Func<TDocument, IDictionary<TKey, TValue>>> targetExpression, TKey key)
            where TDocument : IViewDocument
            where TMessage : IMessage
        {
            var targetMember = (MemberExpression)targetExpression.Body;
            var targetProperty = (PropertyInfo)targetMember.Member;
            var dictionary = (IDictionary<TKey, TValue>)targetProperty.GetValue(document) ??
                             new Dictionary<TKey, TValue>();
            var fieldKey = string.Concat(targetProperty.Name, "[", key, "]");

            if (!document.FieldChanges.ContainsKey(fieldKey) || document.FieldChanges[fieldKey] <= message.Timestamp)
            {
                dictionary.Remove(key);
                document.FieldChanges[fieldKey] = message.Timestamp;
                document.LastChangeTime = message.Timestamp;
            }
        }

        public static bool ReverseContains<T>(this IList<T> list, T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (comparer.Equals(list[i], item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}