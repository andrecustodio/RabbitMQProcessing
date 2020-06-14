using RabbitMQProcessing.Core.Bus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Bus.Abstractions
{
    public interface IEventBus<T, TH> where T : DefaultEvent where TH : IEventHandler<T>
    {
        void Publish(T @event);
        void Subscribe();
        void Unsubscribe();
    }

    //public interface IEventBus
    //{
    //    void Publish(DefaultEvent @event);
    //    void Subscribe()<T, TH> where T : DefaultEvent where TH : IEventHandler<T>;
    //    void Unsubscribe()<T, TH> where T : DefaultEvent where TH : IEventHandler<T>;
    //}
}
