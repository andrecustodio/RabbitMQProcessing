using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Stores
{
    public static class ConsumerFactory
    {
        public static EventingBasicConsumer CreateConsumer(IModel channel, string queueName, EventHandler<BasicDeliverEventArgs> consumer_Received)
        {
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
            consumer.Received += consumer_Received;

            return consumer;
        }
    }
}
