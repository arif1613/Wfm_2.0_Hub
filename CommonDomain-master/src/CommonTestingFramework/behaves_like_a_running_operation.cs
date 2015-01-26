using CommonReadModelLibrary.Models;
using Machine.Specifications;

namespace CommonTestingFramework
{
    [Behaviors]
    public class behaves_like_a_running_operation
    {
        protected static RequestedOperation _document;

        private It the_completed_flag_should_be_false = () => _document.Completed.ShouldBeFalse();
        private It the_deleted_flag_should_be_false = () => _document.Deleted.ShouldBeFalse();
        private It the_failed_flag_should_be_false = () => _document.Failed.ShouldBeFalse();
        private It a_status_message_should_be_set = () => _document.Status.ShouldNotBeEmpty();
    }
}
