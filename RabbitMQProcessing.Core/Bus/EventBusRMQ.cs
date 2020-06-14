using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQProcessing.Core.Bus.Abstractions;
using RabbitMQProcessing.Core.Bus.Events;
using RabbitMQProcessing.Core.Stores;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQProcessing.Core.Bus
{
    public class EventBusRMQ<T, TH> : IEventBus<T, TH>, IDisposable
        where T : DefaultEvent where TH : IEventHandler<T>
    {

        //<EventName, List of HandlerType>
        private Dictionary<string, List<Type>> handlers;
        private string host = "localhost";
        private string broker = "local_broker";
        private string queueName;
        private IConnection conn;
        private IModel channel;

        public EventBusRMQ()
        {
            conn = ClientConnectionFactory.CreateConnection(host);
            channel = CreateChannel(conn);
        }

        private IModel CreateChannel(IConnection conn)
        {
            var model = conn.CreateModel();
            model.ExchangeDeclare(broker, "direct", true, false, null);
            model.QueueDeclare(queueName, true, false, false, null);

            model.CallbackException += Model_CallbackException;

            return model;
        }

        private void Model_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            Console.WriteLine($"Callback Exception Called");
            Console.WriteLine($"Details {e.Detail}");
            Console.WriteLine($"Exception {e.Exception}");

            channel.Dispose();
            channel = CreateChannel(conn);
            StartConsumer();
        }

        public void Dispose() { }

        public void Publish(T @event)
        {
            var eventName = typeof(T).Name;
            using (var channel = conn.CreateModel())
            {
                channel.ExchangeDeclare(broker, "direct", true, false, null);
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

                channel.BasicPublish(broker, eventName, null, body);
            }
        }

        public void Subscribe()
        {
            var eventName = typeof(T).Name;
            var found = handlers.ContainsKey(eventName);
            var channel = conn.CreateModel();

            if (!found)
            {
                channel.QueueBind(queue: queueName, exchange: broker, routingKey: eventName);
            }

            if (!found)
            {
                handlers.Add(eventName, new List<Type>());
            }

            handlers[eventName].Add(typeof(TH));

            StartConsumer();
        }

        private void StartConsumer()
        {
            if (channel != null)
            {
                var csmr = new AsyncEventingBasicConsumer(channel);
                csmr.Received += Csmr_Received;

                channel.BasicConsume(queueName, false, csmr);
            }
        }

        private System.Threading.Tasks.Task Csmr_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = Encoding.UTF8.GetString(@event.Body.ToArray());

            try
            {
                foreach (var sub in handlers[eventName])
                {

                    Activator.CreateInstance(sub);
                    var handler = scope.ResolveOptional();
                    if (handler == null) continue;
                    var eventType = _subsManager.GetEventTypeByName(eventName);
                    var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                    var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Csmr_Received Exception");
                Console.WriteLine($"{ex}");
                Console.WriteLine();
                Console.WriteLine();
            }

            channel.BasicAck(@event.DeliveryTag, multiple: false);
        }

        private Task ProcessEvent(string eventName, string message)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe()
        {
            var eventName = typeof(T).Name;


        }
    }
}
