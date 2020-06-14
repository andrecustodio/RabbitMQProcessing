using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQProcessing.Core.Bus.Abstractions;
using RabbitMQProcessing.Core.Bus.Configuration.Settings;
using RabbitMQProcessing.Core.Bus.Events;
using RabbitMQProcessing.Core.Bus.Manager;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Bus.Extension
{
    public static class EventBusRMQExtensions
    {
        public static void AddRMQEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(provider => RMQSettings.Build(configuration));
            services.AddSingleton<IRMQConnectionManager, RMQConnectionManager>();

            services.AddRabbitMQEvent<EventBusRMQ>();
        }

        public static void AddSubscription<TEvent, TEventHandler>(this IServiceProvider services) where TEvent : DefaultEvent where TEventHandler : IEventHandler<TEvent>
        {
            var bus = services.GetRequiredService<IEventBus>();
            if (bus == null) throw new Exception("EventBus instance not found");
            
            bus.Subscribe<TEvent, TEventHandler>();
        }

        private static void AddRabbitMQEvent<TEventBus>(this IServiceCollection services) where TEventBus : class, IEventBus
        {
            services.AddSingleton<IEventBus, TEventBus>();

            services.AddSingleton<ISubscriptionsManager, SubscriptionManager>();
            services.AddSingleton<IRMQConnectionManager, RMQConnectionManager>();

        }

    }
}
