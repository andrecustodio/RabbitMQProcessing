using RabbitMQ.Client;
using RabbitMQProcessing.Core.Domain.Messages;
using RabbitMQProcessing.Core.Stores;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQProcessing.Service.SectorialValve.SimpleSendReceive
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Application Start");
            Guid senderUniqueId = Guid.NewGuid();

            bool booleanValueToChangeMessage = false;

            while (true)
            {
                booleanValueToChangeMessage = !booleanValueToChangeMessage;

                var message = MakeMessage(senderUniqueId, booleanValueToChangeMessage);
                ClientConnectionFactory.Send("HostQueue", message);

                Console.WriteLine($"Sending message id {message.Id}");
                Task.Delay(5000).Wait();
            }
        }

        public static SectorialEventMessage MakeMessage(Guid senderId, bool hasReachedTarget)
        {
            return new SectorialEventMessage
            {
                ReachedHumidity = hasReachedTarget,
                ServiceId = senderId,
                Id = Guid.NewGuid()
            };
        }
    }
}
