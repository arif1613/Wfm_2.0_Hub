using System;
using System.Collections.Generic;
using CommonReadModelLibrary.Models;
using CommonDomainLibrary;
using FakeItEasy;
using Machine.Specifications;
using NodaTime;

namespace CommonReadModelLibrary.Tests.ViewExtensions
{
    public class when_setting_a_property_of_a_nested_object_inside_the_document
    {
        private static MyMockDocument _document;
        private static IEvent _event;
        private static Type _type;

        private class MagicObject
        {
            public int MagicValue { get; set; }
            public MagicObject2 MoreMagic { get; set; }
        }

        private class MagicObject2
        {
            public int MagicValue { get; set; }
        }

        private class MyMockDocument : IViewDocument
        {
            public string Id { get; set; }
            public bool Deleted { get; set; }
            public Guid HolderId { get; set; }
            public IList<Guid> HandledMessages { get; set; }
            public IDictionary<string, Instant> FieldChanges { get; set; }
            public Instant LastChangeTime { get; set; }
            public MagicObject MagicProperty { get; set; }

            public MyMockDocument()
            {
                HandledMessages = new List<Guid>();
                FieldChanges = new Dictionary<string, Instant>();
            }
        }

        private Establish context = () =>
        {
            _document = new MyMockDocument();

            _event = A.Fake<IEvent>();
            _event.Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
            _type = _event.GetType();
        };

        private Because of = () =>
            {
                _document.Set(_event, d => d.MagicProperty.MagicValue, 0);
                _document.Set(_event, d => d.MagicProperty.MoreMagic.MagicValue, 1);
            };

        private It the_property_of_the_nested_object_should_be_set = () => _document.MagicProperty.MagicValue.ShouldEqual(0);

        private It the_property_inside_the_second_lvl_nested_object_should_be_set =
            () => _document.MagicProperty.MoreMagic.MagicValue.ShouldEqual(1);

        private It the_document_field_changes_should_contain_the_changes =
            () =>
                {
                    _document.FieldChanges.ContainsKey("MagicProperty.MagicValue").ShouldBeTrue();
                    _document.FieldChanges.ContainsKey("MagicProperty.MoreMagic.MagicValue").ShouldBeTrue();
                };

        private It the_change_time_should_be_stored =
            () =>
                {
                    _document.FieldChanges["MagicProperty.MagicValue"].ShouldEqual(_event.Timestamp);
                    _document.FieldChanges["MagicProperty.MoreMagic.MagicValue"].ShouldEqual(_event.Timestamp);
                };

        private It the_last_changed_time_should_be_set =
            () => _document.LastChangeTime.ShouldNotEqual(new Instant());
    }
}
