using CommonReadModelLibrary.Models;
using Machine.Specifications;

namespace CommonTestingFramework
{
    [Behaviors]
    public class behaves_like_a_failed_operation
    {
        protected static RequestedOperation _document;
        protected static string _errorMessage;

        private It the_completed_flag_should_be_true = () => _document.Completed.ShouldBeTrue();
        private It the_deleted_flag_should_be_false = () => _document.Deleted.ShouldBeFalse();
        private It the_failed_flag_should_be_true = () => _document.Failed.ShouldBeTrue();
        private It the_error_message_should_be_set = () => _document.ErrorMessage.ShouldEqual(_errorMessage);
    }
}
