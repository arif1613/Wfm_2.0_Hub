using System;
using System.Collections.Generic;
using CommonDomainLibrary;
using Edit;
using EventStore.Tests.Contracts;
using Machine.Specifications;

namespace EventStore.Tests
{
    public class when_retrieving_an_aggregate_from_the_store : EventStoreTestsBase
    {
        private static Guid _id;
        private static string _name;
        private static TestAggregate _aggregate;
        private static IStoredDataVersion _version;
        private static Guid _ownerId;

        private Establish context = () =>
            {
                _id = Guid.NewGuid();
                _name = "test";
                _ownerId = Guid.Empty;

                Given(_id, new List<IEvent>()
                    {
                        new AggregateCreated(Guid.NewGuid(), Guid.NewGuid(), _id, _ownerId, _name)
                    });
            };

        private Because of = () =>
            {
                var response = Store.GetById<TestAggregate>(_id).Result;

                _aggregate = response.Aggregate as TestAggregate;
                _version = response.Version;
            };

        private It The_aggregate_should_be_retrieved = () => _aggregate.ShouldNotBeNull();
        private It The_aggregate_version_should_be_set = () => _version.ShouldNotBeNull();
    }
}
