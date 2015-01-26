using CommonDomainLibrary.Commands;
using CommonDomainLibrary.Common;
using CommonReadModelLibrary.Rebuild;
using Machine.Specifications;
using Raven.Client;

namespace CommonReadModelLibrary.Tests.ViewRebuilder
{
    public class FakeView
    {
        public FakeView(IAsyncDocumentSession session)
        {
        }
    }

    public class RebuildReadModelViewHandler : BaseRebuildReadModelViewHandler, IHandle<RebuildReadModelView>
    {
        public RebuildReadModelViewHandler(IViewRebuilder viewRebuilder)
            : base(viewRebuilder)
        {
        }
    }

    public class when_getting_a_command_to_rebuild_a_view
    {
        private static IViewRebuilder _viewRebuilder;

        private Establish context = () => _viewRebuilder = new MockViewRebuilder();

        private Because of = () =>
        {
            var handler = new RebuildReadModelViewHandler(_viewRebuilder);
            handler.Handle(new RebuildReadModelView("CommonReadModelLibrary.Tests.ViewRebuilder.FakeView, CommonReadModelLibrary.Tests"), true)
                   .Await();
        };

        private It the_view_rebuilder_should_be_called = () =>
            ((MockViewRebuilder)_viewRebuilder).HasBeenCalledWithType<FakeView>().ShouldBeTrue();
    }
}