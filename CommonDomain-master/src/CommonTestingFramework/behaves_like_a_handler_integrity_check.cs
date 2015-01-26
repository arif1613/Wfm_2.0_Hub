using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Machine.Specifications;

namespace CommonTestingFramework
{
    [Behaviors]
    public class behaves_like_a_handler_integrity_check
    {
        private It all_handle_methods_should_have_a_corresponding_IHandle_interface_implemented_by_the_view = () =>
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
                                                      .Where(t => t.Namespace != null && (t.Namespace.Contains("ReadModel.Views") || t.Namespace.Contains("ApplicationServices")) && t.IsClass && !t.IsNested)
                                                      .ToList();

            foreach (var handler in types)
            {
                var handleMethods = handler.GetMethods().Where(m => m.Name.Contains("Handle"));
                foreach (var method in handleMethods)
                {
                    var handledMessage = method.GetParameters()[0].ParameterType;
                    var handleInterface =
                        handler.GetInterfaces().SingleOrDefault(i => i.IsGenericType && i.GetGenericArguments()[0] == handledMessage);
                    if (handleInterface == null) throw new SpecificationException("Handler '" + handler.FullName + "' has method Handle(" + handledMessage.FullName + ") but does not implement IHandle<" + handledMessage.FullName + ">");
                }
            }
        };
    }
}
