using System;

namespace EventStore
{
    public interface IAggregateDependencyResolver
    {
        object GetDependencyInstance(Type type);
    }
}
