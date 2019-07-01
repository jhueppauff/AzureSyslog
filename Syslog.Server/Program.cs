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
    using Model.Configuration;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;

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
        private static readonly Queue<Message> messageQueue = new Queue<Message>();

        /// <summary>
        /// Message Trigger
        /// </summary>
        private static readonly AutoResetEvent messageTrigger = new AutoResetEvent(false);

        /// <summary>
        /// Listener Address
        /// </summary>
        private static IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 514);

        /// <summary>
        /// Listener Port and Protocol
        /// </summary>
        private static readonly UdpClient udpListener = new UdpClient(514);

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposedValue = false;
        private static Log log;

        private static IConfiguration configuration;
        private static List<StorageEndpointConfiguration> storageEndpointConfigurations;

        /// <summary>
        /// Application Insights Telemetry Client
        /// </summary>
        public static TelemetryClient telemetryClient;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Syslog Server started");
            Console.ResetColor();

            configuration = GetConfiguration();
            storageEndpointConfigurations = GetStorageConfig();

            if (configuration["ApplicationInsights:InstrumentationKey"] != null)
            {
                ConfigureApplicationInsights();
            }

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
                catch (Exception ex)
                {
                    telemetryClient.TrackException(ex);
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

        private static List<StorageEndpointConfiguration> GetStorageConfig()
        {
            List<StorageEndpointConfiguration> endpointConfiguration = new List<StorageEndpointConfiguration>();

            var rootSection = configuration.GetSection("StorageEndpointConfiguration").GetChildren();

            foreach (var rootItem in rootSection)
            {
                if (Convert.ToBoolean(rootItem["Enabled"]))
                {
                    endpointConfiguration.Add(new StorageEndpointConfiguration()
                    {
                        ConnectionString = rootItem["ConnectionString"],
                        ConnectionType = rootItem["ConnectionType"],
                        Enabled = Convert.ToBoolean(rootItem["Enabled"]),
                        Name = rootItem["Name"]
                    });
                }
            }

            return endpointConfiguration;
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

                telemetryClient.Flush();
                // flush is not blocking so wait a bit
                Task.Delay(5000).Wait();

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
                messageTrigger.WaitOne(10000);    // A 10000ms timeout to force processing
                Message[] messageArray = null;

                if (messageQueue.Count != 0)
                {
                    lock (messageQueue)
                    {
                        messageArray = messageQueue.ToArray();
                    }

                    if (messageArray.Length != 0)
                    {
                        Task.Run(() => HandleMessageProcessing(messageArray).Wait(6000));
                    }
                }
            }
        }

        /// <summary>
        /// Message Processing handler, call in a new thread
        /// </summary>
        /// <param name="messages">Array of type <see cref="Data.Message"/></param>
        private static async Task HandleMessageProcessing(Message[] messages)
        {
            if (log is null)
            {
                log = new Log(storageEndpointConfigurations);
            }

            await log.WriteToLog(messages);

            foreach (Message message in messages)
            {
                if (!Convert.ToBoolean(configuration["DisableConsoleOutput"]))
                {
                    Console.WriteLine($"{DateTime.Now} : {message.MessageText}");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} : Processed {messages.Length} messages");
                }

                var metric = new Dictionary<string, double>();
                metric.Add("Processed Messages", messages.Length);

                telemetryClient.TrackEvent("MessagesProcessed", null, metric);
                
                if (Program.messageQueue.Count != 0)
                {
                    Program.messageQueue.Dequeue();
                }
            }
        }

        /// <summary>
        /// Configures Application Insights
        /// </summary>
        private static void ConfigureApplicationInsights()
        {
            TelemetryConfiguration configuration = TelemetryConfiguration.Active;

            configuration.InstrumentationKey = Program.configuration["ApplicationInsights:InstrumentationKey"];
            configuration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

            telemetryClient = new TelemetryClient();
        }
    }
}