using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RabbitMQProcessing.Service.SectorialValve.Configuration.SimpleSendReceive
{
    public static class ConfigurationLoader
    {
        public static Guid ServiceName { get; private set; }

        public static void Load(IConfiguration configuration)
        {
            ServiceName = Guid.Parse(configuration.GetSection("ServiceGuid").ToString());
        }
    }
}
