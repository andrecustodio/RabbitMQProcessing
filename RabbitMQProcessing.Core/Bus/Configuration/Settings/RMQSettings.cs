using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Bus.Configuration.Settings
{
    public class RMQSettings
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
        public string BrokerName { get; set; }

        public static RMQSettings Build(IConfiguration configuration)
        {
            var newSettings = new RMQSettings
            {
                HostName = configuration.GetSection("RMQSettings:HostName").Value,
                UserName = configuration.GetSection("RMQSettings:UserName").Value,
                Password = configuration.GetSection("RMQSettings:Password").Value,
                QueueName = configuration.GetSection("RMQSettings:QueueName").Value,
                BrokerName = configuration.GetSection("RMQSettings:BrokerName").Value
            };

            return newSettings;
        }
    }
}
