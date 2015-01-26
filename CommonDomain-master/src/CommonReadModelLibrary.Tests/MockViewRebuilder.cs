using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Rebuild;
using Raven.Client;

namespace CommonReadModelLibrary.Tests
{
    public class MockViewRebuilder : IViewRebuilder
    {
        private readonly List<Type> _types;

        public MockViewRebuilder()
        {
            _types = new List<Type>();
        }

        public bool HasBeenCalledWithType<T>()
        {
            return _types.Contains(typeof (T));
        }

        public async Task RebuildView(Type viewType, ICommonIdentity dummy = null)
        {
            await Task.Run(() => _types.Add(viewType));
        }
    }
}
