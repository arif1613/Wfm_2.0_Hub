using System;
using CommonDomainLibrary;
using Machine.Specifications;
using NodaTime;

namespace CommonTestingFramework
{
    [Behaviors]
    public class A_proper_event_is_generated
    {
        protected static IEvent _event;
        protected static Guid _correlationId;
        protected static Guid _causationId;
        protected static Guid _ownerId;

        private It The_event_contains_a_message_id = () => _event.MessageId.ShouldNotEqual(Guid.Empty);
        private It The_event_contains_the_correlation_id = () => _event.CorrelationId.ShouldEqual(_correlationId);
        private It The_event_contains_the_causation_id = () => _event.CausationId.ShouldEqual(_causationId);
        private It The_event_timestamp_is_set = () => _event.Timestamp.ShouldNotEqual(new Instant());
        private It The_event_contains_the_aggregate_owner_id = () => _event.OwnerId.ShouldEqual(_ownerId);        
    }
}
