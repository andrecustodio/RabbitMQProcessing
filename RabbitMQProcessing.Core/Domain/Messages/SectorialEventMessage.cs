using RabbitMQProcessing.Core.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Domain.Messages
{
    public class SectorialEventMessage : IMessage
    {
        public bool ReachedHumidity { get; set; }
    }
}
