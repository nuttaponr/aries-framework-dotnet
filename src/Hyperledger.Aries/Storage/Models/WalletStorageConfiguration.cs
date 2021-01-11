using Newtonsoft.Json;

namespace Hyperledger.Aries.Storage
{
    public partial class WalletConfiguration
    {
        /// <summary>
        /// Wallet storage configuration.
        /// </summary>
        public class WalletStorageConfiguration
        {
            /// <summary>
            /// Gets or sets the path.
            /// </summary>
            /// <value>The path.</value>
            [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
            public string Path { get; set; }

            [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
            public string Url { get; set; }

            [JsonProperty("max_connections", NullValueHandling = NullValueHandling.Ignore)]
            public int MaxConnections { get; set; }

            [JsonProperty("wallet_scheme", NullValueHandling = NullValueHandling.Ignore)]
            public string WalletScheme { get; set; }

            [JsonProperty("tls", NullValueHandling = NullValueHandling.Ignore)]
            public string Tls { get; set; }

            /// <inheritdoc />
            public override string ToString() =>
                $"{GetType().Name}: " +
                $"Path={Path}";
        }
    }
}
