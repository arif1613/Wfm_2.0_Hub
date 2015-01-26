using System;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using FakeItEasy;
using Machine.Specifications;

namespace CommonDomainLibrary.Tests
{
    public class When_the_updated_is_trying_to_do_an_update_for_a_non_existent_aggregate
    {
        private static IAggregateRepository _store;
        private static DomainError _ex;
        private static DefaultAggregateUpdater _updater;

        private Establish context = () =>
        {
            _store = A.Fake<IAggregateRepository>();
            var bus = A.Fake<IBus>();

            var task = Task.Factory.StartNew(() => new AggregateRepositoryResponse(null, null));

            A.CallTo(() => _store.GetById<TestAggregate>(A<Guid>.Ignored)).Returns(task);

            _updater = new DefaultAggregateUpdater(_store, bus);
        };

        private Because of =
            () => _ex = Catch.Exception(() => _updater.Update<TestAggregate>(Guid.NewGuid(), Guid.NewGuid(),
                                                                             ag =>
                                                                             ag.ChangeName(Guid.NewGuid(),
                                                                                           Guid.NewGuid(), "test"), false)
                                                      .Await()) as DomainError;

        private It A_DomainError_exception_should_be_generated = () => _ex.ShouldNotBeNull();
        private It The_exception_should_have_the_right_name = () => _ex.Name.ShouldEqual("aggregate-repository-response-error");
    }
}
