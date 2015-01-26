using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using CommonTestingFramework;
using Machine.Specifications;
using Microsoft.ServiceBus;
using NLog;

namespace Bus.Tests.BusTests
{
    public class when_subscribing_to_a_contract_message_type
    {
        private static Bus _bus;
        private static Exception _exception;
        private static readonly IHandlerResolver HandlerResolver = new TestHandlerResolver(new Dictionary<Type, object>()
            {
                {typeof(TestConsumer<BeCool>), new TestConsumer<BeCool>()}
            });

        private Establish context = () =>
        {
            _bus = new Bus(AssemblyContext.ServiceBusConnectionString, HandlerResolver, new BusSerializer(new Serializer()));
            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
            foreach (var topic in manager.GetTopics())
            {
                manager.DeleteTopic(topic.Path);
            }
        };

        private Because of = () => _bus.Subscribe(typeof(IMessage), typeof(TestConsumer<IMessage>)).Await();

        private It no_topics_should_be_created_for_contract_types = () =>
            {
                var associatedMessageTypes = Directory.GetFiles(Path.GetDirectoryName((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath), "*.dll")
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
                                                      .Where(t => typeof(IMessage).IsAssignableFrom(t) && !t.IsInterface)
                                                      .ToList();

                var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
                foreach (var topic in manager.GetTopics())
                {
                    associatedMessageTypes.Any(t => t.FullName.ToLower() == topic.Path.ToLower()).ShouldBeTrue();
                }                
            };

        private Cleanup cleanup = () =>
        {
            LogManager.Flush();
            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
            foreach (var topic in manager.GetTopics())
            {
                manager.DeleteTopic(topic.Path);
            }
        };
    }
}
