using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using CommonInfrastructureLibrary.Serialization.JsonNet;
using Machine.Specifications;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NLog;
using Newtonsoft.Json;

namespace Bus.Tests.BusTests
{
    public class when_handling_a_message_fails
    {
        private static Bus _bus;
        public static int _retries;
        private static SubscriptionClient _client;
        private static CancellationTokenSource waitLock;
        private const int AllowedRetries = 3;
        private static FailingConsumer _consumer;

        private class FailingConsumer: IHandle<BeCool>
        {
            public Task Handle(BeCool e, bool lastTry)
            {
                _retries++;
                throw new NotImplementedException();
            }
        }

        private Establish context = () =>
        {
            waitLock = new CancellationTokenSource();
            _retries = 0;

            _consumer = new FailingConsumer();
            _bus = new Bus(AssemblyContext.ServiceBusConnectionString, new TestHandlerResolver(new Dictionary<Type, object>()
                {
                    {_consumer.GetType(), _consumer}
                }), new BusSerializer(new Serializer()));
            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
            if (manager.TopicExists(typeof(BeCool).ToString()))
            {
                manager.DeleteTopic(typeof(BeCool).ToString());
            }            

            _bus.Subscribe(typeof(BeCool), _consumer.GetType()).Await();
            var md5 = MD5.Create();
            var data = md5.ComputeHash(new MemoryStream(Encoding.UTF8.GetBytes(_consumer.GetType().FullName)));
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }
            var handlerNameHash = stringBuilder.ToString();


            _client = SubscriptionClient.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString,
                                                                       typeof (BeCool).ToString(),
                                                                        handlerNameHash + "/$DeadLetterQueue");
            _client.OnMessageAsync(async m =>
                {
                    var type = Type.GetType(m.ContentType, false);

                    if (type == null)
                    {
                        throw new SystemException("Error getting message content type");
                    }

                    var serializer = new BusSerializer(new Serializer());

                    IMessage message;
                    using (var stream = m.GetBody<Stream>())
                    {
                        message = serializer.Deserialize(type, stream) as IMessage;
                    }

                    waitLock.Cancel();
                });
        };

        private Because of = () =>
        {
            var message = new BeCool(Guid.NewGuid(), Guid.NewGuid());

            var task = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(10000);
                        if (waitLock.IsCancellationRequested) break;
                    }
                }, waitLock.Token);

            waitLock.CancelAfter(TimeSpan.FromSeconds(30));
            _bus.Publish(message).Await();
            task.Await();
        };

        private It the_handling_should_be_retried_the_preconfigured_number_of_times =
            () => _retries.ShouldEqual(AllowedRetries);

        private Cleanup cleanup = () =>
        {
            LogManager.Flush();
            var manager = NamespaceManager.CreateFromConnectionString(AssemblyContext.ServiceBusConnectionString);
            if (manager.TopicExists(typeof(BeCool).ToString()))
            {
                manager.DeleteTopic(typeof(BeCool).ToString());
            }
        };
    }
}
