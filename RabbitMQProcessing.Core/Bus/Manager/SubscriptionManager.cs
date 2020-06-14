using RabbitMQProcessing.Core.Bus.Abstractions;
using RabbitMQProcessing.Core.Bus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RabbitMQProcessing.Core.Bus.Abstractions.ISubscriptionsManager;

namespace RabbitMQProcessing.Core.Bus.Manager
{
    public class SubscriptionManager : ISubscriptionsManager
    {
        //Event Property
        public event EventHandler<string> OnEventRemoved;

        //Controlled Properties
        private readonly Dictionary<string, List<SubscriptionInfo>> dicHandlers;
        private readonly Dictionary<string, Type> dicEventTypes;

        public SubscriptionManager()
        {
            dicHandlers = new Dictionary<string, List<SubscriptionInfo>>();
            dicEventTypes = new Dictionary<string, Type>();
        }

        #region One-liner methods
        public bool IsEmpty => !dicHandlers.Any();
        public void Clear() => dicHandlers.Clear();
        public string GetEventKey<T>() => typeof(T).Name;
        public Type GetEventTypeByName(string eventName) => dicEventTypes[eventName];
        public IEnumerable<ISubscriptionsManager.SubscriptionInfo> GetHandlersForEvent<T>() where T : DefaultEvent => GetHandlersForEvent(GetEventKey<T>());
        public IEnumerable<ISubscriptionsManager.SubscriptionInfo> GetHandlersForEvent(string eventName) => dicHandlers[eventName];
        public bool HasSubscriptionsForEvent<T>() where T : DefaultEvent => dicHandlers.ContainsKey(GetEventKey<T>());
        public bool HasSubscriptionsForEvent(string eventName) => dicHandlers.ContainsKey(eventName);
        #endregion

        #region Dynamic Subscription
        public void RemoveDynamicSubscription<TH>(string eventName) where TH : IDynamicEventHandler
        {
            var handlerToRemove = FindDynamicSubscriptionToRemove<TH>(eventName);
            DoRemoveHandler(eventName, handlerToRemove);
        }

        public void AddDynamicSubscription<TH>(string eventName) where TH : IDynamicEventHandler
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Typed Subscription
        public void AddSubscription<T, TH>()
           where T : DefaultEvent
           where TH : IEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            DoAddSubscription(typeof(TH), eventName, false);
            dicEventTypes.Add(eventName, typeof(T));
        }

        public void RemoveSubscription<T, TH>()
            where T : DefaultEvent
            where TH : IEventHandler<T>
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            DoRemoveHandler(eventName, handlerToRemove);
        }
        #endregion


        //Private Methods for Subscriptions
        private void DoAddSubscription(Type handlerType, string eventName, bool isDynamic)
        {
            //Check for eventname already registered in the dictionary list
            //If it isn't, create a new entry
            if (!HasSubscriptionsForEvent(eventName))
            {
                dicHandlers.Add(eventName, new List<SubscriptionInfo>());
            }

            //Check to see if the handler is already subbed to the event channel
            if (dicHandlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                Console.WriteLine($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
                return;
            }

            dicHandlers[eventName].Add(isDynamic ? SubscriptionInfo.Dynamic(handlerType) : SubscriptionInfo.Typed(handlerType));
        }
        private SubscriptionInfo FindSubscriptionToRemove<T, TH>() where T : DefaultEvent where TH : IEventHandler<T> => DoFindSubscriptionToRemove(GetEventKey<T>(), typeof(TH));
        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }
            return dicHandlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
        }
        private SubscriptionInfo FindDynamicSubscriptionToRemove<TH>(string eventName) where TH : IDynamicEventHandler => DoFindSubscriptionToRemove(eventName, typeof(TH));
        private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                dicHandlers[eventName].Remove(subsToRemove);
                if (!dicHandlers[eventName].Any())
                {
                    dicHandlers.Remove(eventName);
                    if (dicEventTypes.ContainsKey(eventName))
                    {
                        dicEventTypes.Remove(eventName);
                    }
                    RaiseOnEventRemoved(eventName);
                }
            }
        }

        //Event Invokes
        private void RaiseOnEventRemoved(string eventName)
        {
            OnEventRemoved?.Invoke(this, eventName);
        }

    }
}
