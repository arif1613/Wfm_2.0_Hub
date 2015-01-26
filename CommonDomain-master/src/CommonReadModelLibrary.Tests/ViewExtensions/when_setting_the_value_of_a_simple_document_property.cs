using System;
using System.Collections.Generic;
using CommonDomainLibrary;
using CommonReadModelLibrary.Models;
using FakeItEasy;
using Machine.Specifications;
using NodaTime;

namespace CommonReadModelLibrary.Tests.ViewExtensions
{
    public class when_setting_the_value_of_a_simple_document_property
    {
        private static IViewDocument _document;
        private static IEvent _event;

        private Establish context = () =>
            {
                _document = A.Fake<IViewDocument>();
                _document.FieldChanges = new Dictionary<string, Instant>();
                _document.HandledMessages = new List<Guid>();                

                _event = A.Fake<IEvent>();
                _event.OwnerId = Guid.NewGuid();
                _event.Timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
            };

        private Because of = () => _document.Set(_event, d => d.HolderId, _event.OwnerId);

        private It the_document_property_should_be_set = () => _document.HolderId.ShouldEqual(_event.OwnerId);

        private It the_document_field_changes_should_contain_the_change =
            () => _document.FieldChanges.ContainsKey("HolderId").ShouldBeTrue();

        private It the_change_time_should_be_stored =
            () => _document.FieldChanges["HolderId"].ShouldEqual(_event.Timestamp);

        private It the_last_change_time_should_be_set =
            () => _document.LastChangeTime.ShouldNotEqual(new Instant());
    }
}
