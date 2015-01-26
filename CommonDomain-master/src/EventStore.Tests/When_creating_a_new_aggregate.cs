using System;
using System.Linq;
using CommonDomainLibrary;
using EventStore.Tests.Contracts;
using Machine.Specifications;

namespace EventStore.Tests
{
    public class when_creating_a_new_aggregate : EventStoreTestsBase
    {
        private static Guid _id;
        private static string _name;
        private static Guid _causationId;
        private static TestAggregate _storedAggregate;
        private static Guid _ownerId;

        private Establish context = () =>
            {
                _id = Guid.NewGuid();
                _name = "test";
                _causationId = Guid.NewGuid();
                StreamId = "TestAggregate-" + _id;

                Given(_id, Enumerable.Empty<IEvent>());

                var aggregate = new TestAggregate(new TestAggregateState());
                aggregate.Create(Guid.NewGuid(), _causationId, _id, _ownerId, _name);
                Store.Save(_causationId, aggregate, null).Await();
            };

        private Because of = () =>
            {
                _storedAggregate = Store.GetById<TestAggregate>(_id).Result.Aggregate as TestAggregate;
            };

        private It we_should_be_able_to_rehydrate_the_aggregate_from_the_store =
            () => _storedAggregate.ShouldNotBeNull();

        private It the_aggregate_state_should_reflect_the_generated_events = () =>
            {
                _storedAggregate.State.Id.ShouldEqual(_id);
                _storedAggregate.State.Name.ShouldEqual(_name);
                _storedAggregate.State.OwnerId.ShouldEqual(_ownerId);
            };
    }
}
