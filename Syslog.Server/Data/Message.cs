//-----------------------------------------------------------------------
// <copyright file="Message.cs" company="https://github.com/jhueppauff/Syslog.Server">
// Copyright 2018 Jhueppauff
// MIT License
// For licence details visit https://github.com/jhueppauff/Syslog.Server/blob/master/LICENSE
// </copyright>
//-----------------------------------------------------------------------

namespace Syslog.Server.Data
{
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Net;

    /// <summary>
    /// Message Data
    /// </summary>
    public class Message : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        public Message(string partitionKey, string rowKey) : base(partitionKey, rowKey)
        {
        }

        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key.
        /// </summary>
        public String RowKey { get; set; }

        /// <summary>
        /// Gets or sets the Time on which the Syslog Message was receive
        /// </summary>
        public DateTime RecvTime { get; set; }

        /// <summary>
        /// Gets or sets the Message Text of the Syslog Package
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Gets or sets the source IP of the Syslog Sender
        /// </summary>
        public string SourceIP { get; set; }
    }
}