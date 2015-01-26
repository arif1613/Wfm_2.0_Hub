using System;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using Machine.Specifications;

namespace CommonTestingFramework
{
    [Behaviors]
    public class An_exception_is_thrown<T> where T : IMessageAccessor, IAggregate
    {
        protected static DomainError _ex;
        protected static string _exName;
        protected static T _aggregate;
        protected static Guid _causationId;
        protected static bool _exRetry;

        private It an_exception_should_be_thrown = () => _ex.ShouldNotBeNull();

        private It the_exception_has_the_right_name = () => _ex.Name.ShouldEqual(_exName);

        private It the_exception_has_the_right_retry = () => _ex.Retry.ShouldEqual(_exRetry);

        private It no_new_messages_are_generated = () => _aggregate.Messages.GetMessages(_causationId).ShouldBeEmpty();
    }
}
