using RabbitMQProcessing.Core.Bus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQProcessing.Core.Bus.Abstractions
{
    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : DefaultEvent
    {
        Task Handle(DefaultEvent @event);
    }

    public interface IEventHandler
    {
    }

}
