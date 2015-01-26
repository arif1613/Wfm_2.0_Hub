using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonDomainLibrary;
using NodaTime;

namespace WfmHubWorker.Models
{
    public class TopicCommand:ICommand
    {
        public Guid CausationId { get; set; }
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public Instant Timestamp { get; set; }
        public Guid Id { get; set; }
    }
}
