using System;
using System.Collections.Generic;
using CommonDomainLibrary;
using CommonReadModelLibrary.Models;
using FakeItEasy;
using Machine.Specifications;
using NodaTime;

namespace CommonReadModelLibrary.Tests.ViewExtensions
{
    public class when_setting_a_value_into_a_dictionary_property_of_the_document
    {
        private static MyMockDocument _document;
        private static IEvent _event;

        private class MyMockDocument : IViewDocument
        {
            public string Id { get; set; }
            public bool Deleted { get; set; }
            public Guid HolderId { get; set; }
            public IList<Guid> HandledMessages { get; set; }
            public IDictionary<string, Instant> FieldChanges { get; set; }
            public Instant LastChangeTime { get; set; }
            public Dictionary<string, string> MagicProperty { get; set; }

            public MyMockDocument()
            {
                HandledMessages = new List<Guid>();
                FieldChanges = new Dictionary<string, Instant>();
                MagicProperty = new Dictionary<string, string>();
            }
        }

        private Establish context = () =>
        {
            _document = new MyMockDocument();

            _event = A.Fake<IEvent>();
            _event.Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
        };

        private Because of = () => _document.Set(_event, d => d.MagicProperty, "test", "value");

        private It the_document_property_should_be_set = () => _document.MagicProperty["test"].ShouldEqual("value");

        private It the_document_field_changes_should_contain_the_change =
            () => _document.FieldChanges.ContainsKey("MagicProperty[test]").ShouldBeTrue();

        private It the_change_time_should_be_stored =
            () => _document.FieldChanges["MagicProperty[test]"].ShouldEqual(_event.Timestamp);

        private It the_last_changed_time_should_be_set =
            () => _document.LastChangeTime.ShouldNotEqual(new Instant());
    }
}
