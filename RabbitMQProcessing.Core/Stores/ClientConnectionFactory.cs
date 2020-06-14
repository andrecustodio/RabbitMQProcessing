using RabbitMQ.Client;
using RabbitMQProcessing.Core.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Stores
{
    public static class ClientConnectionFactory
    {
        private static IConnection conn => CreateConnection("localhost");

        public static void Send(string queue, IMessage message)
        {
            using (var channel = conn.CreateModel())
            {
                channel.QueueDeclare(queue, true, false, false);

                var body = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                var encoded = Encoding.UTF8.GetBytes(body);

                channel.BasicPublish(string.Empty, queue, Properties(channel, false), encoded);
            }
        }

        public static IConnection CreateConnection(string host)
        {
            var factory = new ConnectionFactory() { HostName = host };
            var connection = factory.CreateConnection();

            if (connection == null) throw new Exception("Connection not possible at the moment");

            return connection;
        }

        private static IBasicProperties Properties(IModel channel, bool persistant)
        {
            var basicProps = channel.CreateBasicProperties();

            basicProps.Persistent = persistant;
            basicProps.DeliveryMode = persistant ? Byte.Parse("2") : Byte.Parse("1");

            return basicProps;
        }
    }
}
