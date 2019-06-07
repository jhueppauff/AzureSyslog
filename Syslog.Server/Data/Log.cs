//-----------------------------------------------------------------------
// <copyright file="Log.cs" company="https://github.com/jhueppauff/Syslog.Server">
// Copyright 2018 Jhueppauff
// MIT License
// For licence details visit https://github.com/jhueppauff/Syslog.Server/blob/master/LICENSE
// </copyright>
//-----------------------------------------------------------------------

namespace Syslog.Server.Data
{
    using AzureStorageAdapter.Table;
    using Syslog.Shared.Model;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Log Class
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Log"/> class.
        /// </summary>
        public Log()
        {
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="path">The path of the file.</param>
        public async Task WriteToLog(Message[] messages, string connectionString)
        {
            TableStorageAdapter tableStorageAdapter = new TableStorageAdapter(connectionString);

            if (messages.Length != 0)
            {
                try
                {
                    // ToDo dynamical name
                    await tableStorageAdapter.ExcuteBatchOperationToTable("logMessages", messages).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured: " + ex.Message);
                }
            }
        }
    }
}