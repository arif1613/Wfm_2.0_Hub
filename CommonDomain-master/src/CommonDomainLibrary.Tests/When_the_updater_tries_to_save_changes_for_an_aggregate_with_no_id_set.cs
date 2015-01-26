using System;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using CommonDomainLibrary.Security;
using Edit;
using FakeItEasy;
using Machine.Specifications;

namespace CommonDomainLibrary.Tests
{
    public class When_the_updater_tries_to_save_changes_for_an_aggregate_with_no_id_set
    {
        private static DefaultAggregateUpdater _updater;
        private static DomainError _ex;
        private static IAggregateRepository _store;
        private static Guid _causationId;
        private static bool _saveCalled;

        public class TestRepository : IAggregateRepository
        {
            public async Task<AggregateRepositoryResponse> GetById<T>(Guid aggregateId) where T : class, IAggregate, IMessageAccessor
            {
                var existingAggregate = new TestAggregate(new TestAggregateState("someVersion"));

                return new AggregateRepositoryResponse(existingAggregate, null);
            }

            public async Task Save<T>(Guid causationId, T aggregate, IStoredDataVersion version) where T : class, IAggregate, IMessageAccessor
            {
                _saveCalled = true;
            }
        }

        private Establish context = () =>
        {
            var bus = A.Fake<IBus>();
            A.CallTo(() => bus.Publish(A.Fake<IMessage>(), A<ICommonIdentity>.Ignored)).WithAnyArguments().Returns(Task.Factory.StartNew(() => { }));
            _causationId = Guid.NewGuid();
            _store = new TestRepository();

            _updater = new DefaultAggregateUpdater(_store, bus);
        };

        private Because of = () => _ex = Catch.Exception(() => _updater.Update<TestAggregate>(Guid.NewGuid(), _causationId,
                                                                             ag =>
                                                                             ag.ChangeName(Guid.NewGuid(),
                                                                                           _causationId, "test"), false)
                                                      .Await()) as DomainError;

        private It A_DomainError_exception_should_be_generated = () => _ex.ShouldNotBeNull();
        private It the_exception_should_be_retryable = () => _ex.Retry.ShouldBeTrue();
    }
}
