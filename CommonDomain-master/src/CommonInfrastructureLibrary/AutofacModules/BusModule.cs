using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using CommonDomainLibrary.Common;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using Edit;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;
using NodaTime.TimeZones;
using ISerializer = Bus.ISerializer;
using Module = Autofac.Module;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class BusModule : Module
    {
        private static readonly RetryPolicy RetryPolicy = new RetryPolicy<TransientErrorDetectionStrategy>(
            new ExponentialBackoff("Retry policy", 5, TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(30), true));

        public class TransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
        {
            public bool IsTransient(Exception ex)
            {
                return true;
            }
        }

        protected override void Load(ContainerBuilder builder)
        {
            var types = Directory.GetFiles(Path.GetDirectoryName((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath), "*.dll")
                                                      .Select(f => AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(f)))
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
                                                      .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                                                        i.GetGenericTypeDefinition() == typeof(IHandle<>) &&
                                                        !i.GetGenericArguments()[0].IsGenericParameter))
                                                      .ToList();

            foreach (var type in types)
            {
                builder.RegisterType(type).AsSelf().AsImplementedInterfaces();
            }

            builder.RegisterType<BusSerializer>().As<ISerializer>().SingleInstance();
            builder.RegisterModule<SerializationModule>();
            builder.RegisterType<AutofacHandlerResolver>().As<IHandlerResolver>();
            builder.Register(c =>
            {
                var azureBusConnectionString = CloudConfigurationManager.GetSetting("AzureBusConnectionString")
                                                                        .Replace("localhost", Environment.MachineName);
                if (azureBusConnectionString.Equals("managed-by-environment-variable"))
                    azureBusConnectionString = Environment.GetEnvironmentVariable("AzureBusConnectionString");

                return new Bus.Bus(azureBusConnectionString, c.Resolve<IHandlerResolver>(), c.Resolve<ISerializer>());

            }).As<IBus>().SingleInstance();
        }
    }
}
