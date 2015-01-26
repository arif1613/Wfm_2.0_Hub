using System;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using Edit;
using FakeItEasy;
using Machine.Specifications;

namespace CommonDomainLibrary.Tests
{
    public class When_the_default_updater_receives_a_concurrency_exception_from_the_store
    {
        private static DefaultAggregateUpdater _updater;
        private static DomainError _ex;
        private static IAggregateRepository _store;
        private static Guid _causationId;

        public class TestRepository : IAggregateRepository
        {
            public async Task<AggregateRepositoryResponse> GetById<T>(Guid aggregateId) where T : class, IAggregate, IMessageAccessor
            {
                var existingAggregate = new TestAggregate(new TestAggregateState("someVersion"));
                existingAggregate.Raise(new AggregateCreated(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "name"));

                return new AggregateRepositoryResponse(existingAggregate, null);
            }

            public async Task Save<T>(Guid causationId, T aggregate, IStoredDataVersion version) where T : class, IAggregate, IMessageAccessor
            {
                throw new ConcurrencyException("", null);
            }
        }

        private Establish context = () =>
            {
                var bus = A.Fake<IBus>();
                _causationId = Guid.NewGuid();
                _store = new TestRepository();

                _updater = new DefaultAggregateUpdater(_store, bus);
            };

        private Because of =
            () => _ex = Catch.Exception(() => _updater.Update<TestAggregate>(Guid.NewGuid(), _causationId,
                                                                             ag =>
                                                                             ag.ChangeName(Guid.NewGuid(),
                                                                                           _causationId, "test"), false)
                                                      .Await()) as DomainError;

        private It A_DomainError_exception_should_be_generated = () => _ex.ShouldNotBeNull();
        private It The_exception_should_have_the_right_name = () => _ex.Name.ShouldEqual("domain-concurrency-exception");
    }
}
