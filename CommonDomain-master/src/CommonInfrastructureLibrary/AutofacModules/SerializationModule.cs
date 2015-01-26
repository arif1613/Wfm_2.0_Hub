using System.Reflection;
using Autofac;
using CommonInfrastructureLibrary.Serialization;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using System;
using System.Linq;
using Module = Autofac.Module;

//using ISerializer = CommonDomainLibrary.ISerializer;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class SerializationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            foreach (
                var type in
                    AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a =>
                        {
                            try
                            {
                                return a.GetTypes();
                            }
                            catch (ReflectionTypeLoadException)
                            {
                                return new Type[0];
                            }
                        })
                        .Where(t => !t.IsAbstract && typeof (IContractVersionMigrator).IsAssignableFrom(t)))
            {
                builder.RegisterType(type).As<IContractVersionMigrator>();
            }

            builder.RegisterType<Serializer>().As<ISerializer>().SingleInstance();
        }
    }
}