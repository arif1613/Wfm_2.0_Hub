using System;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using FakeItEasy;
using Machine.Specifications;

namespace CommonDomainLibrary.Tests
{
    public class When_the_updater_fails_an_aggregate_execution_but_its_not_the_last_try
    {
        private static DefaultAggregateUpdater _updater;
        private static DomainError _ex;
        private static IAggregateRepository _store;
        private static TestAggregate _existingAggregate;
        private static Guid _causationId;
        private static Guid _correlationId;

        private Establish context = () =>
        {
            _store = A.Fake<IAggregateRepository>();
            var bus = A.Fake<IBus>();
            _causationId = Guid.NewGuid();
            _correlationId = Guid.NewGuid();

            _existingAggregate = new TestAggregate(new TestAggregateState("someVersion"));
            _existingAggregate.Raise(new AggregateCreated(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "name"));

            var task = Task.Factory.StartNew(() => new AggregateRepositoryResponse(_existingAggregate, null));

            A.CallTo(() => _store.GetById<TestAggregate>(A<Guid>.Ignored)).Returns(task);

            _updater = new DefaultAggregateUpdater(_store, bus);
        };

        private Because of = () => _ex = Catch.Exception(() => _updater.Update<TestAggregate>(_existingAggregate.Id, _causationId,
                                                                  agg => agg.ThrowRetriableException(_correlationId, _causationId), false).Await()) as DomainError;

        private It the_exception_should_be_rethrown = () => _ex.ShouldNotBeNull();
    }
}
