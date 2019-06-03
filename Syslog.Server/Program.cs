//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="https://github.com/jhueppauff/Syslog.Server">
// Copyright 2018 Jhueppauff
// MIT License
// For licence details visit https://github.com/jhueppauff/Syslog.Server/blob/master/LICENSE
// </copyright>
//-----------------------------------------------------------------------

namespace Syslog.Server
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Syslog.Server.Data;
    using Syslog.Shared.Model;

    /// <summary>
    /// Program class
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Program : IDisposable
    {
        /// <summary>
        /// As long this is true the Service will continue to receive new Messages.
        /// </summary>
        private static readonly bool queueing = true;

        /// <summary>
        /// Message Queue of the type Data.Message.
        /// </summary>
        private static Queue<Message> messageQueue = new Queue<Message>();

        /// <summary>
        /// Message Trigger
        /// </summary>
        private static AutoResetEvent messageTrigger = new AutoResetEvent(false);

        /// <summary>
        /// Listener Address
        /// </summary>
        private static IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 514);

        /// <summary>
        /// Listener Port and Protocol
        /// </summary>
        private static UdpClient udpListener = new UdpClient(514);

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposedValue = false;

        private static IConfiguration configuration;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Syslog Server started");
            Console.ResetColor();

            configuration = GetConfiguration();

            // Main processing Thread
            Thread handler = new Thread(new ThreadStart(HandleMessage))
            {
                IsBackground = true
            };
            handler.Start();

            /* Main Loop */
            /* Listen for incoming data on udp port 514 (default for SysLog events) */
            while (queueing || messageQueue.Count != 0)
            {
                try
                {
                    anyIP.Port = 514;

                    // Receive the message
                    byte[] bytesReceive = udpListener.Receive(ref anyIP);

                    DateTime now = DateTime.Now;

                    // push the message to the queue, and trigger the queue
                    Message message = new Message($"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}", DateTime.Now.ToFileTimeUtc().ToString())
                    {
                        MessageText = Encoding.ASCII.GetString(bytesReceive),
                        RecvTime = now,
                        SourceIP = anyIP.Address.ToString()
                    };

                    lock (messageQueue)
                    {
                        try
                        {
                            messageQueue.Enqueue(message);
                        }
                        catch (OutOfMemoryException)
                        {
                            Console.WriteLine("Out of memory Exception occured. Dropping incoming message");
                        }
                    }

                    messageTrigger.Set();
                }
                catch (Exception)
                {
                    // ToDo: Add Error Handling
                }
            }
        }

        /// <summary>
        /// Gets the Configuration
        /// </summary>
        /// <returns></returns>
        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.development.json", true)
                .Build();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    udpListener.Dispose();
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Internal Message handler
        /// </summary>
        private static void HandleMessage()
        {
            while (queueing)
            {
                messageTrigger.WaitOne(10000);    // A 5000ms timeout to force processing
                Message[] messageArray = null;

                lock (messageQueue)
                {
                    messageArray = messageQueue.ToArray();
                }

                if (messageArray.Length != 0)
                {
                    Task.Run(() => HandleMessageProcessing(messageArray).Wait(2000));
                }
            }
        }

        /// <summary>
        /// Message Processing handler, call in a new thread
        /// </summary>
        /// <param name="messages">Array of type <see cref="Data.Message"/></param>
        private static async Task HandleMessageProcessing(Message[] messages)
        {
            Log log = new Log();
            await log.WriteToLog(messages, configuration.GetSection("AzureStorage:StorageConnectionString").Value);

            foreach (Message message in messages)
            {
                Console.WriteLine(message.MessageText);

                if (Program.messageQueue.Count != 0)
                {
                    Program.messageQueue.Dequeue();
                }
            }
        }
    }
}