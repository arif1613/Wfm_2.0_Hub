using System;
using System.Collections.Generic;
using System.Linq;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using Edit;
using EventStore.Tests.Contracts;
using Machine.Specifications;

namespace EventStore.Tests
{
    public class when_saving_a_bunch_of_events_and_commands : EventStoreTestsBase
    {
        private static Guid _aggregateId;
        private static TestAggregate _aggregate;
        private static Guid _causationId;
        private static IStoredDataVersion _version;
        private static DeferrableMessage _deferredCommand;

        private Establish context = () =>
            {
                _aggregateId = Guid.NewGuid();
                _causationId = Guid.NewGuid();

                Given(_aggregateId, new List<IEvent>()
                    {
                        new AggregateCreated(Guid.NewGuid(), Guid.NewGuid(), _aggregateId, Guid.NewGuid(), "test")
                    });

                var result = Store.GetById<TestAggregate>(_aggregateId).Result;

                _aggregate = result.Aggregate as TestAggregate;
                _version = result.Version;
                _aggregate.ChangeName(Guid.NewGuid(), _causationId, "test2");
            };

        private Because of = () =>
            {
                Store.Save(_causationId, _aggregate, _version).Wait();
                _deferredCommand = StoredDeferredCommands.SingleOrDefault();
            };

        private It the_event_should_be_stored =
            () => (StoredEvents.SingleOrDefault(e => e.GetType() == typeof(AggregateNameChanged)) as AggregateNameChanged).ShouldNotBeNull();

        private It the_command_should_be_stored = () => (StoredCommands.SingleOrDefault() as BeCool).ShouldNotBeNull();

        private It the_deferred_command_should_be_storred =
            () => StoredDeferredCommands.SingleOrDefault().ShouldNotBeNull();
    }
}
