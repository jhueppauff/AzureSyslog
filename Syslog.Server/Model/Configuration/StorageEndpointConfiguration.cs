//-----------------------------------------------------------------------
// <copyright file="StorageEndpointConfiguration.cs" company="https://github.com/jhueppauff/Syslog.Server">
// Copyright 2018 Jhueppauff
// MIT License
// For licence details visit https://github.com/jhueppauff/Syslog.Server/blob/master/LICENSE
// </copyright>
//-----------------------------------------------------------------------


using Newtonsoft.Json;

namespace Syslog.Server.Model.Configuration
{
    public partial class StorageEndpointConfiguration
    {
        [JsonProperty("ConnectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("Enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("ConnectionType")]
        public string ConnectionType { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
