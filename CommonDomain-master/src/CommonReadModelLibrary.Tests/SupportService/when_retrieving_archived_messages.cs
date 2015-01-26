using System;
using System.Collections.Generic;
using System.Linq;
using CommonDomainLibrary;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Models;
using CommonReadModelLibrary.Support;
using Machine.Specifications;
using Newtonsoft.Json;
using NodaTime;

namespace CommonReadModelLibrary.Tests.SupportService
{
    public class when_retrieving_archived_messages
    {
        private sealed class TestMessage1 : IMessage, IComparable<TestMessage1>
        {
            public Guid CausationId { get; set; }
            public Guid MessageId { get; set; }
            public Guid CorrelationId { get; set; }
            public Instant Timestamp { get; set; }
            public InnerClass InnerObject { get; set; }
            public Dictionary<string, string> Dictionary { get; set; }
            public List<string> List { get; set; }

            public int CompareTo(TestMessage1 other)
            {
                var equal = MessageId.Equals(other.MessageId);
                equal = equal && CausationId.Equals(other.CausationId);
                equal = equal && CorrelationId.Equals(other.CorrelationId);
                equal = equal && Timestamp.Equals(other.Timestamp);
                equal = equal && InnerObject.CompareTo(other.InnerObject) == 0;
                equal = Dictionary.Aggregate(equal, (c, kvp) => c && kvp.Value.Equals(other.Dictionary[kvp.Key]));
                equal = equal && Dictionary.Count.Equals(other.Dictionary.Count);
                equal = List.Aggregate(equal, (current, listItem) => current && other.List.Contains(listItem));
                equal = other.List.Aggregate(equal, (current, listItem) => current && List.Contains(listItem));
                equal = equal && List.Count.Equals(other.List.Count);

                if (equal)
                    return 0;
                return -1;
            }
        }

        private sealed class TestMessage2 : IMessage, IComparable<TestMessage2> 
        {
            public Guid CausationId { get; set; }
            public Guid MessageId { get; set; }
            public Guid CorrelationId { get; set; }
            public Instant Timestamp { get; set; }

            public int CompareTo(TestMessage2 other)
            {
                var equal = MessageId.Equals(other.MessageId);
                equal = equal && CausationId.Equals(other.CausationId);
                equal = equal && CorrelationId.Equals(other.CorrelationId);
                equal = equal && Timestamp.Equals(other.Timestamp);

                if (equal)
                    return 0;
                return -1;
            }
        }

        private sealed class InnerClass : IComparable<InnerClass>
        {
            public string MyValue { get; set; }
            public string MyOtherValue;

            public int CompareTo(InnerClass other)
            {
                if (MyValue.Equals(other.MyValue) && MyOtherValue.Equals(other.MyOtherValue))
                    return 0;
                return -1;
            }
        }

        private sealed class Identity : ICommonIdentity
        {
            public string Name { get; private set; }
            public string AuthenticationType { get; private set; }
            public bool IsAuthenticated { get; private set; }
            public Guid Id { get; private set; }
            public Guid OwnerId { get; set; }
            public Guid ClientId { get; private set; }
        }

        private static ISupportService _supportService;
        private static MockRequestHelper _requestHelper;
        private static string _resourceUrl;

        private static Guid _holderId;
        private static string _requestUrl1;
        private static string _requestUrl2;

        private static TestMessage1 _message1;
        private static TestMessage2 _message2;

        private static IEnumerable<ArchivedMessage> _archivedMessagesResponse;

        private Establish context = () =>
        {
            _holderId = Guid.NewGuid();

            _resourceUrl = "someurl";

            _message1 = new TestMessage1
            {
                CausationId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                MessageId = Guid.NewGuid(),
                Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow),
                Dictionary = new Dictionary<string, string> { { "key", "value" } },
                InnerObject = new InnerClass { MyValue = "someValue", MyOtherValue = "someOtherValue"},
                List = new List<string> { "listIitem" }
            };

            _message2 = new TestMessage2
            {
                CausationId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                MessageId = Guid.NewGuid(),
                Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow.AddMinutes(-1)),
            };

            _requestHelper = new MockRequestHelper();

            _requestUrl1 = string.Format("{0}/{1}/archivedMessages/{2}/{3}", _resourceUrl, _holderId.ToString("N"),
            typeof(TestMessage1).Assembly.GetName().Name.Replace(".", "-"), typeof(TestMessage1).FullName.Replace(".", "-"));

            _requestHelper.WhenGET(
                _requestUrl1,
                JsonConvert.SerializeObject(new[]
                {
                    new
                    {
                        id = Guid.NewGuid(),
                        message = new
                        {
                            causation_id = _message1.CausationId,
                            correlation_id = _message1.CorrelationId,
                            message_id = _message1.MessageId,
                            timestamp = _message1.Timestamp.ToDateTimeUtc(),
                            dictionary = _message1.Dictionary,
                            inner_object = new
                            {
                                my_value = _message1.InnerObject.MyValue,
                                my_other_value = _message1.InnerObject.MyOtherValue
                            },
                            list = _message1.List
                        },
                        message_type = _message1.GetType()
                    }
                }));

            _requestUrl2 = string.Format("{0}/{1}/archivedMessages/{2}/{3}", _resourceUrl, _holderId.ToString("N"),
            typeof(TestMessage2).Assembly.GetName().Name.Replace(".", "-"), typeof(TestMessage2).FullName.Replace(".", "-"));

            _requestHelper.WhenGET(
                _requestUrl2,
                JsonConvert.SerializeObject(new[]
                {
                    new
                    {
                        id = Guid.NewGuid(),
                        message = new
                        {
                            causation_id = _message2.CausationId,
                            correlation_id = _message2.CorrelationId,
                            message_id = _message2.MessageId,
                            timestamp = _message2.Timestamp.ToDateTimeUtc(),
                        },
                        message_type = _message2.GetType()
                    }
               }));

            _supportService = new Support.SupportService(_requestHelper, _resourceUrl);
        };

        private Because of = () => _archivedMessagesResponse = _supportService.GetArchivedMessages(new List<Type>
        {
            typeof(TestMessage1),
            typeof(TestMessage2)
        }, new Identity { OwnerId = _holderId }, Guid.NewGuid(), null);

        private It the_correct_requests_are_made = () =>
        {
            _requestHelper.MadeRequests.Keys.ShouldContain(_requestUrl1);
            _requestHelper.MadeRequests.Keys.ShouldContain(_requestUrl2);
        };
            

        private It the_archived_messages_are_returned_in_chronological_order = () =>
        {
            var testMessage2 = _archivedMessagesResponse.ToArray()[0].Message as TestMessage2;
            if (testMessage2 != null)
                testMessage2.MessageId.ShouldEqual(_message2.MessageId);

            var testMessage1 = _archivedMessagesResponse.ToArray()[1].Message as TestMessage1;
            if (testMessage1 != null)
                testMessage1.MessageId.ShouldEqual(_message1.MessageId);
        };

        private It the_returned_messages_are_identical_to_the_archived_messages = () =>
        {
            (_archivedMessagesResponse.ToArray()[0].Message as TestMessage2).ShouldEqual(_message2);
            (_archivedMessagesResponse.ToArray()[1].Message as TestMessage1).ShouldEqual(_message1);
        };
    }
}
