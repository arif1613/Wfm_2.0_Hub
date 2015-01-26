using System;
using System.Collections.Generic;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using Edit;
using EventStore.Tests.Contracts;
using NodaTime;

namespace EventStore.Tests
{
    public class TestAggregateDependencyResolver : IAggregateDependencyResolver
    {
        public object GetDependencyInstance(Type type)
        {
            return null;
        }
    }

    public class TestAggregate : IAggregate, IMessageAccessor
    {
        public readonly TestAggregateState State;
        private readonly MessageRouter _messageRouter;

        public Guid Id
        {
            get { return State.Id; }
        }

        public MessageRouter Messages
        {
            get { return _messageRouter; }
        }

        public TestAggregate(TestAggregateState state)
        {
            State = state;
            _messageRouter = MessageRouter.For(State);
        }

        public void Create(Guid correlationId, Guid causationId, Guid id, Guid ownerId, string name)
        {
            this.Raise(new AggregateCreated(correlationId, causationId, id, ownerId, name));
        }

        public void ChangeName(Guid correlationId, Guid causationId, string name)
        {
            this.Raise(new AggregateNameChanged(correlationId, causationId, State.Id, State.OwnerId, name));
            this.Raise(new BeCool(State.Id, causationId));
            this.Raise(new DeferrableMessage(new BeCool(State.Id, causationId),
                                             Instant.FromDateTimeUtc(DateTime.UtcNow.AddMinutes(5))));
        }
    }

    public class TestStoredDataVersion : IStoredDataVersion { }

}
namespace EventStore.Tests.Contracts
{
    [Serializable]
    public class BeCool : IMessage
    {
        public Guid Id { get; set; }
        public Guid CausationId { get; set; }
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }

        private BeCool()
        {
        }

        public BeCool(Guid id, Guid causationId)
        {
            Id = id;
            CausationId = causationId;
            MessageId = Guid.NewGuid();
            CorrelationId = Guid.NewGuid();
            Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
        }        
    }

    public class TestAggregateState : IState
    {
        public Guid Id { get; set; }
        public OrderedDictionary<Guid, List<dynamic>> Messages { get; set; }
        public string Name { get; set; }
        public Guid OwnerId { get; set; }

        public TestAggregateState()
        {
            Messages = new OrderedDictionary<Guid, List<dynamic>>();
        }

        public void Apply(AggregateCreated e)
        {
            Id = e.Id;
            Name = e.Name;
        }

        public void Apply(AggregateNameChanged e)
        {
            Name = e.Name;
        }
    }

    [Serializable]
    public class AggregateNameChanged : IEvent
    {
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid CausationId { get; set; }
        public string Name { get; set; }

        public AggregateNameChanged()
        {
        }

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

    [Serializable]
    public class AggregateCreated : IEvent
    {
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid CausationId { get; set; }
        public string Name { get; set; }

        private AggregateCreated()
        {
        }

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
}

