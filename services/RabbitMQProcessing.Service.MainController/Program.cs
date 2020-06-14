using RabbitMQProcessing.Service.MainController.Application.Services.SimpleSendReceive;
using System;

namespace RabbitMQProcessing.Service.MainController.SimpleSendReceive
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //Should Receive messages
            var wateringService = new WateringService();
            wateringService.Start();

            Console.ReadLine();
        }
    }
}
