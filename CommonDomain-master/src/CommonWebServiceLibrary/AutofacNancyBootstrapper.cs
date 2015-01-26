﻿using Autofac;
using System;
using System.Collections.Generic;
using Nancy;
using Nancy.Diagnostics;
using Nancy.Bootstrapper;

namespace CommonWebServiceLibrary
{
    public abstract class AutofacNancyBootstrapper : NancyBootstrapperWithRequestContainerBase<ILifetimeScope>
    {
        protected override IDiagnostics GetDiagnostics()
        {
            return this.ApplicationContainer.Resolve<IDiagnostics>();
        }

        /// <summary>
        /// Gets all registered application startup tasks
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IApplicationStartup"/> instances. </returns>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return this.ApplicationContainer.Resolve<IEnumerable<IApplicationStartup>>();
        }

        /// <summary>
        /// Gets all registered application registration tasks
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IApplicationRegistrations"/> instances.</returns>
        protected override IEnumerable<IApplicationRegistrations> GetApplicationRegistrationTasks()
        {
            return this.ApplicationContainer.Resolve<IEnumerable<IApplicationRegistrations>>();
        }

        /// <summary>
        /// Get INancyEngine
        /// </summary>
        /// <returns>INancyEngine implementation</returns>
        protected override INancyEngine GetEngineInternal()
        {
            return this.ApplicationContainer.Resolve<INancyEngine>();
        }

        /// <summary>
        /// Create a default, unconfigured, container
        /// </summary>
        /// <returns>Container instance</returns>
        protected override ILifetimeScope GetApplicationContainer()
        {
            var builder = new ContainerBuilder();
            return builder.Build();
        }

        /// <summary>
        /// Bind the bootstrapper's implemented types into the container.
        /// This is necessary so a user can pass in a populated container but not have
        /// to take the responsibility of registering things like INancyModuleCatalog manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override void RegisterBootstrapperTypes(ILifetimeScope applicationContainer)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(this).As<INancyModuleCatalog>();
            builder.Update(ApplicationContainer.ComponentRegistry);
        }

        /// <summary>
        /// Bind the default implementations of internally used types into the container as singletons
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="typeRegistrations">Type registrations to register</param>
        protected override void RegisterTypes(ILifetimeScope container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            var builder = new ContainerBuilder();
            foreach (var typeRegistration in typeRegistrations)
            {
                builder.RegisterType(typeRegistration.ImplementationType).As(typeRegistration.RegistrationType).SingleInstance();
            }
            builder.Update(ApplicationContainer.ComponentRegistry);
        }

        /// <summary>
        /// Bind the various collections into the container as singletons to later be resolved
        /// by IEnumerable{Type} constructor dependencies.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="collectionTypeRegistrations">Collection type registrations to register</param>
        protected override void RegisterCollectionTypes(ILifetimeScope container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            var builder = new ContainerBuilder();
            foreach (var collectionTypeRegistration in collectionTypeRegistrations)
            {
                foreach (var implementationType in collectionTypeRegistration.ImplementationTypes)
                {
                    builder.RegisterType(implementationType).As(collectionTypeRegistration.RegistrationType).SingleInstance();
                }
            }
            builder.Update(container.ComponentRegistry);
        }

        /// <summary>
        /// Bind the given instances into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(ILifetimeScope container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            var builder = new ContainerBuilder();
            foreach (var instanceRegistration in instanceRegistrations)
            {
                builder.RegisterInstance(instanceRegistration.Implementation).As(instanceRegistration.RegistrationType);
            }
            builder.Update(container.ComponentRegistry);
        }

        /// <summary>
        /// Creates a per request child/nested container
        /// </summary>
        /// <returns>Request container instance</returns>
        protected override ILifetimeScope CreateRequestContainer()
        {
            return ApplicationContainer.BeginLifetimeScope();
        }

        /// <summary>
        /// Bind the given module types into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="moduleRegistrationTypes"><see cref="INancyModule"/> types</param>
        protected override void RegisterRequestContainerModules(ILifetimeScope container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            var builder =
                new ContainerBuilder();

            foreach (var moduleRegistrationType in moduleRegistrationTypes)
            {
                builder.RegisterType(moduleRegistrationType.ModuleType).As<INancyModule>();
            }

            builder.Update(container.ComponentRegistry);
        }

        /// <summary>
        /// Retrieve all module instances from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <returns>Collection of <see cref="INancyModule"/> instances</returns>
        protected override IEnumerable<INancyModule> GetAllModules(ILifetimeScope container)
        {
            return container.Resolve<IEnumerable<INancyModule>>();
        }

        /// <summary>
        /// Retreive a specific module instance from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleType">Type of the module</param>
        /// <returns>An <see cref="INancyModule"/> instance</returns>
        protected override INancyModule GetModule(ILifetimeScope container, Type moduleType)
        {
            var builder =
                new ContainerBuilder();

            builder.RegisterType(moduleType).As<INancyModule>();
            builder.Update(container.ComponentRegistry);

            return container.Resolve<INancyModule>();
        }
    }
}