using System;
using System.Threading.Tasks;
using CommonDomainLibrary.Security;

namespace CommonReadModelLibrary.Rebuild
{
    public interface IViewRebuilder
    {
        Task RebuildView(Type viewType, ICommonIdentity identity);
    }
}
