using RabbitMQProcessing.Core.Bus.Abstractions;
using RabbitMQProcessing.Core.Bus.Events;
using RabbitMQProcessing.EventBus.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQProcessing.EventBus.Models.Handlers
{
    public class SectorialEventHandler : IEventHandler<SectorialEvent>
    {
        public async Task Handle(DefaultEvent @event)
        {
            Console.WriteLine("That's the event that has been sent over:");
            Console.WriteLine($"{@event}");
            Console.WriteLine();
            Console.WriteLine();

            await Task.Yield();
        }
    }
}
