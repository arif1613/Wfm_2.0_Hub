using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using CommonDomainLibrary.Security;
using Edit;
using NodaTime;

namespace CommonTestingFramework
{
    public class TestAggregateRepository : IAggregateRepository
    {
        public IEnumerable<IEvent> GeneratedEvents { get; private set; }
        public IEnumerable<IEvent> GivenEvents { get; set; }
        private Dictionary<Type, Type> DependencyMap { get; set; }
        private Dictionary<Type, object> DependencyMapInstances { get; set; }
        public IEnumerable<DeferrableMessage> GeneratedDeferred { get; set; }
        public IEnumerable<IMessage> GeneratedCommands { get; set; }

        public TestAggregateRepository(IEnumerable<IEvent> events, Dictionary<Type, Type> dependencyMap, Dictionary<Type, object> dependencyMapInstances)
        {
            GivenEvents = events;
            DependencyMap = dependencyMap;
            DependencyMapInstances = dependencyMapInstances;
        }

        public async Task<AggregateRepositoryResponse> GetById<T>(Guid aggregateId) where T : class, IAggregate, IMessageAccessor
        {
            return new AggregateRepositoryResponse(GetAggregate<T>(GivenEvents.Where(e => e.Id == aggregateId)), null);
        }

        public async Task Save<T>(Guid causationId, T aggregate, IStoredDataVersion version) where T : class, IAggregate, IMessageAccessor
        {
            if (GeneratedEvents != null)
            {
                GeneratedEvents = GeneratedEvents.Union(aggregate.Messages.GetEvents(causationId)).ToList();
            }
            else
            {
                GeneratedEvents = aggregate.Messages.GetEvents(causationId);
            }

            if (GeneratedCommands != null)
            {
                GeneratedCommands = GeneratedCommands.Union(aggregate.Messages.GetCommands(causationId)).ToList();
            }
            else
            {
                GeneratedCommands = aggregate.Messages.GetCommands(causationId);
            }

            if (GeneratedDeferred != null)
            {
                GeneratedDeferred = GeneratedDeferred.Union(aggregate.Messages.GetDeferredCommands(causationId)).ToList();
            }
            else
            {
                GeneratedDeferred = aggregate.Messages.GetDeferredCommands(causationId);
            }
        }

        private T GetAggregate<T>(IEnumerable<IEvent> events) where T : class, IAggregate, IMessageAccessor
        {
            var ctr = typeof(T).GetConstructors().First();

            var paramList = new List<object>();

            var state = new object();

            foreach (var param in ctr.GetParameters())
            {
                var type = param.ParameterType;
                if (type.IsClass)
                {
                    var instance = Activator.CreateInstance(type);

                    if (instance is IState) state = instance;
                    paramList.Add(instance);
                }

                if (type.IsInterface)
                {
                    if(!DependencyMap.ContainsKey(type) && !DependencyMapInstances.ContainsKey(type)) throw new Exception("Helper dependency dictionary does not contain a mapping for type '"+type.Name+"'");
                    paramList.Add(DependencyMap.ContainsKey(type)
                                      ? Activator.CreateInstance(DependencyMap[type])
                                      : DependencyMapInstances[type]);
                }
            }

            var aggregate = (T)Activator.CreateInstance(typeof(T), paramList.ToArray());
            
            if (events != null)
                foreach (var @event in events)
                {
                    aggregate.Messages.RaiseMessage(@event);
                }

            return aggregate;
        }
    }

    public class TestBus : IBus
    {
        public List<IEvent> PublishedEvents { get; private set; }
        public List<IMessage> CommandsSent { get; private set; }
        public List<DeferrableMessage> CommandsDeferred { get; private set; }

        public TestBus()
        {
            PublishedEvents = new List<IEvent>();
            CommandsSent = new List<IMessage>();
            CommandsDeferred = new List<DeferrableMessage>();
        }

        public async Task Publish(IMessage message, ICommonIdentity identity = null)
        {
            if(message is IEvent) PublishedEvents.Add(message as IEvent);
            if(!(message is IEvent)) CommandsSent.Add(message);
        }

        public async Task Defer(IMessage message, Instant instant, ICommonIdentity identity = null)
        {
            CommandsDeferred.Add(new DeferrableMessage(message, instant));
        }

        public async Task Subscribe(Type messageType, Type handler)
        {
        }

        public async Task LoadAllHandlersFromContainer(object container)
        {
        }
    }

    public class ApplicationServiceTestHelper<T> where T : class, IAggregate, IMessageAccessor
    {
        public IEvent GeneratedEvent
        {
            get { return _aggregateRepository.GeneratedEvents.SingleOrDefault(); }
        }

        public IEnumerable<IEvent> GeneratedEvents
        {
            get { return _aggregateRepository.GeneratedEvents; }
        }

        public IEnumerable<IMessage> GeneratedCommands
        {
            get { return _aggregateRepository.GeneratedCommands; }
        }
        public IEnumerable<DeferrableMessage> DeferredCommands
        {
            get { return _aggregateRepository.GeneratedDeferred; }
        }

        private TestAggregateRepository _aggregateRepository;
        private readonly DefaultAggregateUpdater _aggregateUpdater;
        public Dictionary<Type, Type> DependencyMap;
        public Dictionary<Type, object> DependencyMapInstances;
        private TestBus _bus;

        public ApplicationServiceTestHelper()
        {
            DependencyMap = new Dictionary<Type, Type>();
            DependencyMapInstances = new Dictionary<Type, object>();
            _aggregateRepository = new TestAggregateRepository(Enumerable.Empty<IEvent>(), DependencyMap, DependencyMapInstances);
            _bus = new TestBus();

            _aggregateUpdater = new DefaultAggregateUpdater(_aggregateRepository, _bus);
        }

        public void Given(IEnumerable<IEvent> events)
        {
            _aggregateRepository.GivenEvents = events;
        }

        private IEnumerable<object> GetHandlers(IMessage message)
        {
            var ns = typeof(T).Namespace + ".ApplicationServices";

            var handlers = typeof(T).Assembly.GetTypes().Where(t => t.Namespace == ns && t.GetInterfaces().Contains(typeof(IHandle<>).MakeGenericType(message.GetType())));

            return handlers.Select(handler =>
                {
                    var ctr = handler.GetConstructors().First();

                    var paramList = new List<object>();

                    var state = new object();

                    foreach (var param in ctr.GetParameters())
                    {
                        var type = param.ParameterType;
                        if (type.IsClass)
                        {
                            var instance = Activator.CreateInstance(type);

                            if (instance is IState) state = instance;
                            paramList.Add(instance);
                        }

                        if (type.IsInterface)
                        {
                            if (type == typeof (IAggregateUpdater)) paramList.Add(_aggregateUpdater);
                            else
                            {
                                if (!DependencyMap.ContainsKey(type) && !DependencyMapInstances.ContainsKey(type))
                                    throw new Exception(
                                        "Helper dependency dictionary does not contain a mapping for type '" + type.Name +
                                        "'");
                                paramList.Add(DependencyMap.ContainsKey(type)
                                                  ? Activator.CreateInstance(DependencyMap[type])
                                                  : DependencyMapInstances[type]);
                            }
                        }
                    }

                    return Activator.CreateInstance(handler, paramList.ToArray());
                });
        }

        public void When<TM>(TM message) where TM : class, IMessage
        {
            var handlers = GetHandlers(message);

            foreach (var handler in handlers)
            {
                var h = handler as IHandle<TM>;
                if (h != null) h.Handle(message, false).Wait();
            }
        }
    }
}
