using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using CommonDomainLibrary.Common;

namespace CommonInfrastructureLibrary
{
    public class AutofacHandlerResolver : IHandlerResolver
    {
        private readonly ILifetimeScope _container;

        public AutofacHandlerResolver(ILifetimeScope container)
        {
            _container = container;
        }

        public object Resolve(Type handlerType)
        {
            return _container.IsRegistered(handlerType) ? _container.Resolve(handlerType) : null;
        }

        public object Resolve(Type handlerType, Dictionary<string, object> parameters)
        {
            if (_container.IsRegistered(handlerType))
            {
                var paramList = parameters.Keys.Select(key => new NamedParameter(key, parameters[key])).ToList();

                return _container.Resolve(handlerType, paramList);
            }
            return null;
        }
    }
}
