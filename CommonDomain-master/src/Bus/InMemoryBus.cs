using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using CommonDomainLibrary.Security;
using NLog;
using NodaTime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bus
{
    public class InMemoryBus : IBus, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IHandlerResolver _resolver;
        private ConcurrentDictionary<Type, ConcurrentDictionary<string, Type>> _subscriptions;
        private ConcurrentDictionary<Guid, int> _messageRetries;
        private ConcurrentQueue<IMessage> _queue;
        private ConcurrentQueue<Tuple<IMessage, Instant>> _deferredQueue;
        private readonly IList<Task> _workerTasks;
        private readonly CancellationTokenSource _cancellationToken;
        private readonly int _retryCount;

        public InMemoryBus(IHandlerResolver resolver)
        {
            if (resolver == null) throw new SystemException("Handler resolver cannot be null");

            _resolver = resolver;
            _retryCount = 3;
            _subscriptions = new ConcurrentDictionary<Type, ConcurrentDictionary<string, Type>>();
            _messageRetries = new ConcurrentDictionary<Guid, int>();

            _queue = new ConcurrentQueue<IMessage>();
            _deferredQueue = new ConcurrentQueue<Tuple<IMessage, Instant>>();
            _cancellationToken = new CancellationTokenSource();
            _workerTasks = new List<Task>
                {
                    Task.Factory.StartNew(async () =>
                        {
                            while (!_cancellationToken.IsCancellationRequested)
                            {
                                IMessage msg;
                                while (_queue.TryDequeue(out msg))
                                {
                                    if (!_messageRetries.ContainsKey(msg.MessageId)) _messageRetries[msg.MessageId] = 0;

                                    try
                                    {
                                        Logger.Debug("({0})Trying to find handlers for message '{1}'",
                                                           msg.CorrelationId, msg.GetType());
                                        if (!_subscriptions.ContainsKey(msg.GetType()))
                                        {
                                            Logger.Debug("({0})No handlers found", msg.CorrelationId);
                                            continue;
                                        }
                                        Logger.Debug("({0})Begin: Passing message '{1}' to {2} handlers",
                                                           msg.CorrelationId, msg.GetType(),
                                                           _subscriptions[msg.GetType()].Count);

                                        var error = false;

                                        foreach (var sub in _subscriptions[msg.GetType()])
                                        {
                                            var handlerType = typeof (IHandle<>).MakeGenericType(msg.GetType());
                                            var handler = _resolver.Resolve(sub.Value);

                                            Logger.Debug("({0})Executing handler: '{0}'", msg.CorrelationId,
                                                               handler.GetType());
                                            try
                                            {
                                                var method = handlerType.GetMethod("Handle", new[] { msg.GetType(), typeof(bool) });
                                                await (Task)method.Invoke(handler, new object[] { msg, _messageRetries[msg.MessageId] == _retryCount });
                                            }
                                            catch (Exception e)
                                            {
                                                Logger.ErrorException(string.Format("({0})Handler threw exception:", msg.CorrelationId), e);
                                                error = true;
                                            }
                                        }

                                        if (error)
                                        {
                                            if (_messageRetries[msg.MessageId] < _retryCount)
                                            {
                                                Logger.Debug("({0})Requing message '{1}'", msg.CorrelationId,
                                                                   msg.GetType());
                                                _queue.Enqueue(msg);
                                                _messageRetries[msg.MessageId]++;
                                            }
                                            else
                                                Logger.Debug("({0})Deadlettering message '{1}'", msg.CorrelationId,
                                                                   msg.GetType());
                                        }
                                        Logger.Debug("({0})END: Passing message '{1}' to handlers",
                                                           msg.CorrelationId, msg.GetType());
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.ErrorException(string.Format("({0})WEIRD BUS EXCEPTION. CHECK IT OUT !!:", msg.CorrelationId), ex);
                                        _queue.Enqueue(msg);
                                    }
                                }
                                await Task.Delay(10);
                            }
                        }, _cancellationToken.Token),
                    Task.Factory.StartNew(async () =>
                        {
                            while (!_cancellationToken.IsCancellationRequested)
                            {
                                Tuple<IMessage, Instant> ev;
                                while (_deferredQueue.TryDequeue(out ev))
                                {
                                    if (ev.Item2 < Instant.FromDateTimeUtc(DateTime.UtcNow)) _queue.Enqueue(ev.Item1);
                                    else _deferredQueue.Enqueue(ev);
                                }
                                await Task.Delay(10);
                            }
                        }, _cancellationToken.Token)
                };
        }

        public async Task Publish(IMessage message, ICommonIdentity identity = null)
        {
            Logger.Debug("({0})Publishing message of type '{1}'", message.CorrelationId, message.GetType().Name);
            _messageRetries[(message).MessageId] = 0;
            _queue.Enqueue(message);
        }

        public async Task Defer(IMessage message, Instant instant, ICommonIdentity identity = null)
        {
            Logger.Debug("({0})Deferring message of type '{1}'", message.CorrelationId, message.GetType().Name);

            if (instant > Instant.FromDateTimeUtc(DateTime.UtcNow))
            {
                Logger.Debug("({0})Bus deferring message to '{1}'", message.CorrelationId, instant);
                _deferredQueue.Enqueue(new Tuple<IMessage, Instant>(message, instant));
            }
            else
            {
                await Publish(message);
            }
        }

        public async Task Subscribe(Type messageType, Type handler)
        {
            Logger.Debug("BEGIN: Subscribing handler '{0}' for message type '{1}'", handler, messageType.Name);

            var md5 = MD5.Create();
            var data = md5.ComputeHash(new MemoryStream(Encoding.UTF8.GetBytes(handler.FullName)));
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }
            var handlerNameHash = stringBuilder.ToString();

            if (!typeof(IMessage).IsAssignableFrom(messageType)) throw new InvalidOperationException("The message type is not an IMessage");
            if (!typeof(IHandler).IsAssignableFrom(handler)) throw new InvalidOperationException("Handler is not an IHandler");
            var handlerInterface = typeof(IHandle<>).MakeGenericType(messageType);
            if (!handlerInterface.IsAssignableFrom(handler)) throw new InvalidOperationException("Handler does not handle this message type (doesn't implement IHandle<mesageType>)");

            var associatedMessageTypes = Directory.GetFiles(Path.GetDirectoryName((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath), "*.dll")
                                                      .Select(f => AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(f)))
                                                      .SelectMany(a =>
                                                      {
                                                          try
                                                          {
                                                              return a.GetTypes();
                                                          }
                                                          catch (ReflectionTypeLoadException)
                                                          {
                                                              return new Type[0];
                                                          }
                                                      })
                                                      .Where(t => messageType.IsAssignableFrom(t) && !t.IsInterface)
                                                      .ToList();

            foreach (var associatedMessageType in associatedMessageTypes)
            {
                if (!_subscriptions.ContainsKey(associatedMessageType))
                {
                    _subscriptions[associatedMessageType] = new ConcurrentDictionary<string, Type>();
                }

                if (_subscriptions[associatedMessageType].ContainsKey(handlerNameHash)) throw new SystemException("An instance of this handler is already subscribed");

                _subscriptions[associatedMessageType][handlerNameHash] = handler;
            }
            
            Logger.Debug("END: Subscribing handler '{0}' for message type '{1}'", handler, messageType.Name);
        }

        public async Task Clear()
        {
            _queue = new ConcurrentQueue<IMessage>();
            _deferredQueue = new ConcurrentQueue<Tuple<IMessage, Instant>>();
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
            Task.WaitAll(_workerTasks.ToArray());
        }
    }
}
