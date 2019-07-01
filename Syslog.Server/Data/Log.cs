//-----------------------------------------------------------------------
// <copyright file="Log.cs" company="https://github.com/jhueppauff/Syslog.Server">
// Copyright 2018 Jhueppauff
// MIT License
// For licence details visit https://github.com/jhueppauff/Syslog.Server/blob/master/LICENSE
// </copyright>
//-----------------------------------------------------------------------

using AzureStorageAdapter.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Syslog.Server.Model.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Syslog.Server.Data
{
    /// <summary>
    /// Log Class
    /// </summary>
    public class Log
    {
        private readonly List<StorageEndpointConfiguration> configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log"/> class.
        /// </summary>
        /// <param name="configuration">Inject the <see cref="IConfiguration"/> class.</param>
        public Log(List<StorageEndpointConfiguration> configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="path">The path of the file.</param>
        public async Task WriteToLog(Syslog.Shared.Model.Message[] messages)
        {
            if (messages.Length != 0)
            {
                try
                {
                    foreach (StorageEndpointConfiguration configItem in configuration)
                    {
                        switch (configItem.ConnectionType)
                        {
                            case "TableStorage":
                                TableStorageAdapter tableStorageAdapter = new TableStorageAdapter(configItem.ConnectionString);

                                await tableStorageAdapter.ExcuteBatchOperationToTable(configItem.Name, messages).ConfigureAwait(false);
                                break;
                            case "ServiceBus":
                                QueueClient queueClient = new QueueClient(configItem.ConnectionString, configItem.Name);

                                List<Microsoft.Azure.ServiceBus.Message> serviceBusMessages = new List<Microsoft.Azure.ServiceBus.Message>();

                                foreach (Syslog.Shared.Model.Message logMessage in messages)
                                {
                                    Microsoft.Azure.ServiceBus.Message serviceBusMessage = new Microsoft.Azure.ServiceBus.Message()
                                    {
                                        Body = Encoding.UTF8.GetBytes(logMessage.MessageText),
                                        MessageId = logMessage.RowKey,
                                        PartitionKey = logMessage.PartitionKey,
                                    };

                                    serviceBusMessage.UserProperties.Add("SourceIP", logMessage.SourceIP);
                                    serviceBusMessage.UserProperties.Add("RecvTime", logMessage.RecvTime);

                                    serviceBusMessages.Add(serviceBusMessage);
                                }

                                await queueClient.SendAsync(serviceBusMessages);
                                break;
                            case "LocalFile":
                                foreach (var item in messages)
                                {
                                    await File.AppendAllTextAsync(configItem.ConnectionString, JsonConvert.SerializeObject(messages));
                                }
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Unknow output type: {configItem.ConnectionType}. Check your appsettings");
                                Console.ResetColor();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.telemetryClient.TrackException(ex);
                    Console.WriteLine("An error occured: " + ex.Message);
                }
            }
        }
    }
}