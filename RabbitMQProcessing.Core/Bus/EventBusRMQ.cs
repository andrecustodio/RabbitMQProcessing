using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQProcessing.Core.Bus.Abstractions;
using RabbitMQProcessing.Core.Bus.Configuration.Settings;
using RabbitMQProcessing.Core.Bus.Events;
using RabbitMQProcessing.Core.Bus.Extension;
using RabbitMQProcessing.Core.Stores;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQProcessing.Core.Bus
{
    public class EventBusRMQ : IEventBus, IDisposable
    {
        private readonly RMQSettings settings;
        private readonly IRMQConnectionManager connectionmanager;
        private readonly ISubscriptionsManager subscriptionManager;
        private IModel channel;
        private readonly string _exchangeType = "direct";

        private readonly IServiceProvider serviceCollection;

        public EventBusRMQ(RMQSettings settings, IServiceProvider service, IRMQConnectionManager connectionManager, ISubscriptionsManager subscriptionManager)
        {
            this.settings = settings;
            this.serviceCollection = service;
            this.connectionmanager = connectionManager;
            this.subscriptionManager = subscriptionManager;

            this.channel = CreateChannel();
            this.subscriptionManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private IModel CreateChannel()
        {
            if (!connectionmanager.IsConnected) connectionmanager.TryConnect();

            var model = connectionmanager.CreateModel();
            model.ExchangeDeclare(settings.BrokerName, "direct", true, false, null);
            model.QueueDeclare(settings.QueueName, true, false, false, null);

            model.CallbackException += Model_CallbackException;

            return model;
        }

        public void Dispose()
        {
            channel.Dispose();
            subscriptionManager.Clear();
        }

        public void Publish(DefaultEvent @event)
        {
            if (!connectionmanager.IsConnected)
            {
                connectionmanager.TryConnect();
            }

            using (var channel = connectionmanager.CreateModel())
            {
                var eventName = @event.GetTypeName();
                channel.ExchangeDeclare(settings.BrokerName, _exchangeType);

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2;
                channel.BasicPublish(settings.BrokerName, eventName, true, properties, body);
            }


        }

        public void Subscribe<T, TH>() where T : DefaultEvent where TH : IEventHandler<T>
        {

            var eventName = subscriptionManager.GetEventKey<T>();
            DoInternalSubscription(eventName);

            subscriptionManager.AddSubscription<T, TH>();
            StartConsumer();
        }

        private void StartConsumer()
        {
            if (channel != null)
            {
                var csmr = new AsyncEventingBasicConsumer(channel);
                csmr.Received += Csmr_Received;
                channel.BasicConsume(settings.QueueName, false, csmr);
            }
        }

        public void Unsubscribe<T, TH>() where T : DefaultEvent where TH : IEventHandler<T>
        {
            var eventName = subscriptionManager.GetEventKey<T>();
            Console.WriteLine("Unsubscribing from event {EventName}", eventName);
            Console.WriteLine();
            subscriptionManager.RemoveSubscription<T, TH>();

        }

        #region Events
        private void SubsManager_OnEventRemoved(object sender, string e)
        {
            //Check to see if the connection is still open
            //Otherwise, try to connect
            if (!connectionmanager.IsConnected)
            {
                connectionmanager.TryConnect();
            }

            //Unbind the removed event
            using (var channel = connectionmanager.CreateModel())
            {
                channel.QueueUnbind(queue: settings.QueueName,
                    exchange: settings.BrokerName,
                    routingKey: e);
                if (subscriptionManager.IsEmpty)
                {
                    channel.Close();
                }
            }
        }
        private void Model_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            Console.WriteLine($"Callback Exception Called");
            Console.WriteLine($"Details {e.Detail}");
            Console.WriteLine($"Exception {e.Exception}");

            channel.Dispose();
            channel = CreateChannel();
            StartConsumer();
        }
        private async System.Threading.Tasks.Task Csmr_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = Encoding.UTF8.GetString(@event.Body.ToArray());

            var processed = false;
            try
            {
                processed = await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Csmr_Received Exception");
                Console.WriteLine($"{ex}");
                Console.WriteLine();
                Console.WriteLine();
            }

            if (processed)
            {
                channel.BasicAck(@event.DeliveryTag, multiple: false);
            }
            else
            {
                channel.BasicNack(@event.DeliveryTag, false, true);
            }
        }
        #endregion

        //Private methods
        private void DoInternalSubscription(string eventName)
        {
            var containsKey = subscriptionManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!connectionmanager.IsConnected)
                {
                    connectionmanager.TryConnect();
                }

                using (var channel = connectionmanager.CreateModel())
                {
                    channel.QueueBind(queue: settings.QueueName,
                        exchange: settings.BrokerName,
                        routingKey: eventName);
                }
            }
        }

        private async Task<bool> ProcessEvent(string eventName, string message)
        {
            var processed = false;
            if (subscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = serviceCollection.CreateScope())
                {
                    var subscriptions = subscriptionManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);
                            if (!(handler is IDynamicEventHandler))
                            {
                                Console.WriteLine($"Cannot find EventHandler, type {subscription.HandlerType.Name}");
                                Console.WriteLine();
                            }

                            dynamic eventData = JObject.Parse(message);
                            await ((IDynamicEventHandler)handler).Handle(eventData);
                        }
                        else
                        {
                            var eventType = subscriptionManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);
                            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] { integrationEvent });
                        }
                    }
                }
                processed = true;
            }
            else
            {
                Console.WriteLine("No subscription for RabbitMQ event: {EventName}", eventName);
                Console.WriteLine();
            }

            return processed;
        }


    }
}
