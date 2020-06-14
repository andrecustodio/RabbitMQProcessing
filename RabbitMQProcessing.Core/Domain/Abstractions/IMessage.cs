using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Domain.Abstractions
{
    public abstract class IMessage
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }


        public string ToBodyMessage()
        {
            var str = JsonConvert.SerializeObject(this);
            return str;
        }

    }
}
