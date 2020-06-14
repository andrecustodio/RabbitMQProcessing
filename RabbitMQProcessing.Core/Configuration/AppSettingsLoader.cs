using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Configuration
{
    public class AppSettingsLoader
    {
        public static Lazy<IConfiguration> Instance { get; } = new Lazy<IConfiguration>(Build);

        private static IConfiguration Build()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }
}
