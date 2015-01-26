using System;
using System.Collections.Generic;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using NodaTime;

namespace Bus.Tests
{
    public class TestHandlerResolver : IHandlerResolver
    {
        private readonly Dictionary<Type, object> _handlers;

        public TestHandlerResolver(Dictionary<Type,object> handlers)
        {
            _handlers = handlers;
        }

        public virtual object Resolve(Type handlerType)
        {
            return _handlers[handlerType];
        }

        public object Resolve(Type handlerType, Dictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }
    }

    public class BeCool : IMessage, IComparable<BeCool>
    {
        public Guid Id { get; set; }
        public Guid CausationId { get; set; }
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }

        public BeCool(Guid id, Guid causationId)
        {
            Id = id;
            CausationId = causationId;
            MessageId = Guid.NewGuid();
            CorrelationId = Guid.NewGuid();
            Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
        }

        public int CompareTo(BeCool other)
        {
            if ( Id == other.Id && CausationId == other.CausationId && MessageId == other.MessageId &&
                CorrelationId == other.CorrelationId) return 0;
            return -1;
        }
    }

    public class BeCool2 : IMessage, IComparable<BeCool2>
    {
        public Guid Id { get; set; }
        public Guid CausationId { get; set; }
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }

        public BeCool2(Guid id, Guid causationId)
        {
            Id = id;
            CausationId = causationId;
            MessageId = Guid.NewGuid();
            CorrelationId = Guid.NewGuid();
            Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
        }

        public int CompareTo(BeCool2 other)
        {
            if (Id == other.Id && CausationId == other.CausationId && MessageId == other.MessageId &&
                CorrelationId == other.CorrelationId) return 0;
            return -1;
        }
    }
}