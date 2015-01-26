using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using CommonDomainLibrary.Security;
using Edit;
using FakeItEasy;
using Machine.Specifications;

namespace CommonDomainLibrary.Tests
{
    public class When_the_updater_fails_the_aggregate_action_execution_with_a_non_retriable_exception
    {
        private static DefaultAggregateUpdater _updater;
        private static IAggregateRepository _store;
        private static TestAggregate _existingAggregate;
        private static Guid _causationId;
        private static Guid _correlationId;
        private static IEnumerable<IEvent> _savedEvents;
        private static bool _errorPublished;
        private static IBus _bus;

        public class TestRepository : IAggregateRepository
        {
            public async Task<AggregateRepositoryResponse> GetById<T>(Guid aggregateId) where T : class, IAggregate, IMessageAccessor
            {
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
            _bus = A.Fake<IBus>();
            _causationId = Guid.NewGuid();
            _correlationId = Guid.NewGuid();
            _errorPublished = false;

            A.CallTo(() => _bus.Publish(A<IMessage>.Ignored, A<ICommonIdentity>.Ignored)).WithAnyArguments().Invokes(
                p =>
                {
                    var errorEvent = p.Arguments[0] as ErrorEvent;
                    if (errorEvent != null)
                    {
                        _errorPublished = true;
                    }
                }).Returns(Task.Factory.StartNew(() => { }));

            _existingAggregate = new TestAggregate(new TestAggregateState("someVersion"));
            _existingAggregate.Raise(new AggregateCreated(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "name"));

            _updater = new DefaultAggregateUpdater(_store, _bus);
        };

        private Because of = () => _updater.Update<TestAggregate>(_existingAggregate.Id, _causationId,
                                                                  agg => agg.ThrowNonretriableException(_correlationId, _causationId), false).Await();

        private It the_error_event_should_be_published_on_the_bus =
            () => _errorPublished.ShouldBeTrue();

        private It no_events_should_be_raised_in_the_aggregate =
            () => _existingAggregate.Messages.GetMessages(_causationId).ShouldBeEmpty();
    }
}
