using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQProcessing.Core.Bus.Abstractions;
using RabbitMQProcessing.Core.Bus.Configuration.Settings;
using RabbitMQProcessing.Core.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQProcessing.Core.Bus.Manager
{
    public class RMQConnectionManager : IRMQConnectionManager
    {
        //Default Properties
        private IConnection connection;
        RMQSettings settings;

        //Control Properties
        private bool disposed;
        private readonly object syncRoot = new object();

        public RMQConnectionManager(RMQSettings settings)
        {
            this.settings = settings;
            connection = ClientConnectionFactory.CreateConnection(settings.HostName);
        }

        public bool IsConnected => connection != null && connection.IsOpen && !disposed;

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                if (!TryConnect()) throw new Exception("Not possible to create a connection at the moment");
                return CreateModel();
            }

            return connection.CreateModel();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            try
            {
                connection.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("RMQConnectionManager Error:");
                Console.WriteLine($"{ex}");
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        public bool TryConnect()
        {
            lock (syncRoot)
            {
                if (IsConnected) return true;

                connection = ClientConnectionFactory.CreateConnection(settings.HostName);

                if (IsConnected)
                {
                    connection.ConnectionShutdown += OnConnectionShutdown;
                    connection.CallbackException += OnCallbackException;
                    connection.ConnectionBlocked += OnConnectionBlocked;

                    return true;
                }

                return false;
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (disposed) return;
            Console.WriteLine("Connection has Shutdown. Attempting to reconnect");
            Console.WriteLine();
            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (disposed) return;
            Console.WriteLine("Connection has thrown an exception. Exception Details");
            Console.WriteLine($"{e.Exception}");
            Console.WriteLine("Attempting to reconnect");
            Console.WriteLine();
            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (disposed) return;
            Console.WriteLine("Connection has Shutdown. Attempting to reconnect");
            Console.WriteLine();
            TryConnect();
        }
    }
}
