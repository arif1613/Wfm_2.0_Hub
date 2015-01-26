using System;
using System.Threading.Tasks;
using CommonDomainLibrary.Commands;
using CommonDomainLibrary.Security;

namespace CommonReadModelLibrary.Rebuild
{
    public abstract class BaseRebuildReadModelViewHandler
    {
        private readonly IViewRebuilder _viewRebuilder;

        protected BaseRebuildReadModelViewHandler(IViewRebuilder viewRebuilder)
        {
            _viewRebuilder = viewRebuilder;
        }

        public async Task Handle(RebuildReadModelView e, bool lastTry)
        {
            var view = Type.GetType(e.ViewType, false);

            if (view == null)
                return;

            await _viewRebuilder.RebuildView(view, e.Sender());
        }
    }
}
