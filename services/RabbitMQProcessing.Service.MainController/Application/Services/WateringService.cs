using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQProcessing.Core.Stores;
using System;
using System.Text;

namespace RabbitMQProcessing.Service.MainController.Application.Services.SimpleSendReceive
{
    public class WateringService
    {
        private IModel channel;
        private EventingBasicConsumer consumer;

        public void Start()
        {
            var factory = ClientConnectionFactory.CreateConnection("localhost");
            this.channel = factory.CreateModel();

            channel.QueueDeclare("HostQueue", true, false, false, null);
            consumer = ConsumerFactory.CreateConsumer(channel, "HostQueue", Consumer_Received);
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());

            Console.WriteLine("New Message d00d");
            Console.WriteLine($"Message received is {message}");
        }

    }
}
