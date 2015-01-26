using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using NodaTime;

namespace CommonDomainLibrary.Tests
{
    public class TestAggregate : IAggregate, IMessageAccessor
    {
        private readonly TestAggregateState _state;
        private MessageRouter _messageRouter;
        public MessageRouter Messages { get { return _messageRouter; } }
        public Guid Id { get { return _state.Id; } }
        public string Version { get { return _state.Version; } }

        public TestAggregate(TestAggregateState state)
        {
            _state = state;
            _messageRouter = MessageRouter.For(_state);
        }

        public async Task Create(Guid correlationId, Guid causationId, Guid id, Guid ownerId, string name)
        {
            this.Raise(new AggregateCreated(correlationId, causationId, id, ownerId, name));
        }

        public async Task ChangeName(Guid correlationId, Guid causationId, string name)
        {
            this.Raise(new AggregateNameChanged(correlationId, causationId, _state.Id, _state.OwnerId, name));
        }

        public async Task ThrowRetriableException(Guid correlationId, Guid causationId)
        {
            throw DomainError.Retriable("Error", new ErrorEvent(correlationId, causationId, _state.Id, _state.OwnerId));
        }

        public async Task ThrowNonDomainErrorException(Guid correlationId, Guid causationId)
        {
            throw new SystemException("adasda");
        }

        public async Task ThrowNonretriableException(Guid correlationId, Guid causationId)
        {
            throw DomainError.Final("Error", new ErrorEvent(correlationId, causationId, _state.Id, _state.OwnerId));
        }
    }

    public class TestAggregateState : IState
    {
        public Guid Id { get; set; }
        public OrderedDictionary<Guid, List<dynamic>> Messages { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public Guid OwnerId { get; set; }

        public TestAggregateState(string version)
        {
            Version = version;
            Messages = new OrderedDictionary<Guid, List<dynamic>>();
        }

        public void Apply(AggregateCreated e)
        {
            Id = e.Id;
            Name = e.Name;
            OwnerId = e.OwnerId;
        }

        public void Apply(AggregateNameChanged e)
        {
            Name = e.Name;
        }
    }

    public class AggregateNameChanged : IEvent
    {
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid CausationId { get; set; }
        public string Name { get; set; }

        public AggregateNameChanged(Guid correlationId, Guid causationId, Guid id, Guid ownerId, string name)
        {
            CorrelationId = correlationId;
            CausationId = causationId;
            Id = id;
            OwnerId = ownerId;
            Name = name;
            MessageId = Guid.NewGuid();
            Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
        }
    }

    public class AggregateCreated : IEvent
    {
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid CausationId { get; set; }
        public string Name { get; set; }

        public AggregateCreated(Guid correlationId, Guid causationId, Guid id, Guid ownerId, string name)
        {
            CorrelationId = correlationId;
            CausationId = causationId;
            Id = id;
            OwnerId = ownerId;
            Name = name;
            MessageId = Guid.NewGuid();
            Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
        }
    }

    public class ErrorEvent : IEvent, IErrorEvent
    {
        public Guid CausationId { get; set; }
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }
        public Guid Id { get; set; }
        public string ErrorMessage { get; set; }
        public Guid OwnerId { get; set; }

        public ErrorEvent(Guid correlationId, Guid causationId, Guid id, Guid ownerId)
        {
            CorrelationId = correlationId;
            CausationId = causationId;
            Id = id;
            OwnerId = ownerId;
            MessageId = Guid.NewGuid();
            Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
        }
        
    }
}
