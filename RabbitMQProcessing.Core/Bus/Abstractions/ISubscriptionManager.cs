using RabbitMQProcessing.Core.Bus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Bus.Abstractions
{
    public interface ISubscriptionsManager
    {
        event EventHandler<string> OnEventRemoved;

        string GetEventKey<T>();

        bool IsEmpty { get; }

        void AddSubscription<T, TH>()
            where T : DefaultEvent
            where TH : IEventHandler<T>;

        void RemoveSubscription<T, TH>()
            where TH : IEventHandler<T>
            where T : DefaultEvent;

        void AddDynamicSubscription<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void RemoveDynamicSubscription<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void Clear();

        bool HasSubscriptionsForEvent<T>() where T : DefaultEvent;

        bool HasSubscriptionsForEvent(string eventName);

        Type GetEventTypeByName(string eventName);

        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : DefaultEvent;

        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

        public class SubscriptionInfo
        {
            public bool IsDynamic { get; }
            public Type HandlerType { get; }
            private SubscriptionInfo(bool isDynamic, Type handlerType)
            {
                IsDynamic = isDynamic;
                HandlerType = handlerType;
            }
            public static SubscriptionInfo Dynamic(Type handlerType)
            {
                return new SubscriptionInfo(true, handlerType);
            }
            public static SubscriptionInfo Typed(Type handlerType)
            {
                return new SubscriptionInfo(false, handlerType);
            }
        }
    }
}
