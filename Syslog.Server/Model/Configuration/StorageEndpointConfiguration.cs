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
