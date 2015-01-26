using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using Edit;
using FakeItEasy;
using Machine.Specifications;

namespace CommonDomainLibrary.Tests
{
    public class When_the_updater_fails_the_last_try_with_an_exception_different_from_DomainError
    {
        private static DefaultAggregateUpdater _updater;
        private static IAggregateRepository _store;
        private static TestAggregate _existingAggregate;
        private static Guid _causationId;
        private static Guid _correlationId;
        private static IEnumerable<IEvent> _savedEvents;
        private static Exception _ex;

        public class TestRepository : IAggregateRepository
        {
            public async Task<AggregateRepositoryResponse> GetById<T>(Guid aggregateId) where T : class, IAggregate, IMessageAccessor
            {
                _existingAggregate = new TestAggregate(new TestAggregateState("someVersion"));
                _existingAggregate.Raise(new AggregateCreated(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "name"));

                return new AggregateRepositoryResponse(_existingAggregate, null);
            }

            public async Task Save<T>(Guid causationId, T aggregate, IStoredDataVersion version) where T : class, IAggregate, IMessageAccessor
            {
                _savedEvents = aggregate.Messages.GetEvents(causationId);
            }
        }

        private Establish context = () =>
        {
            _store = new TestRepository();
            var bus = A.Fake<IBus>();
            _causationId = Guid.NewGuid();
            _correlationId = Guid.NewGuid();

            _updater = new DefaultAggregateUpdater(_store, bus);
        };

        private Because of = () => _ex = Catch.Exception(() => _updater.Update<TestAggregate>(_existingAggregate.Id, _causationId,
                                                                  agg => agg.ThrowNonDomainErrorException(_correlationId, _causationId), true).Await()) as SystemException;

        private It the_exception_should_be_rethrown = () => _ex.ShouldNotBeNull();
    }
}
