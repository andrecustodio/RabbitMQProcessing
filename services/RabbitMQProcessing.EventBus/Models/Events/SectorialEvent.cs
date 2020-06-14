using RabbitMQProcessing.Core.Bus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQProcessing.EventBus.Models.Events
{
    public class SectorialEvent : DefaultEvent
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public bool ReachedHumidity { get; set; }
    }
}
