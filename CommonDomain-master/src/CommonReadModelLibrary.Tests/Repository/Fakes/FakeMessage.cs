using System;
using CommonDomainLibrary;
using NodaTime;

namespace CommonReadModelLibrary.Tests.Repository.Fakes
{
    public class FakeMessage : IMessage
    {
        public FakeMessage()
        {
            MessageId = Guid.NewGuid();
            CorrelationId = Guid.NewGuid();
            Timestamp = new Instant();
        }

        public Guid CausationId { get; set; }
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }
    }
}