using System;
using Autofac;
using EventStore;

namespace CommonInfrastructureLibrary
{
    public class AutofacAggregateDependencyResolver : IAggregateDependencyResolver
    {
        private readonly ILifetimeScope _container;

        public AutofacAggregateDependencyResolver(ILifetimeScope container)
        {
            _container = container;
        }

        public object GetDependencyInstance(Type type)
        {
            return _container.Resolve(type);
        }
    }
}
