using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Bus.Events
{
    public class DefaultEvent
    {
        public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public DefaultEvent()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
        }

    }
}
