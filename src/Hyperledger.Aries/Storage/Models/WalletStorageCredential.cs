using System;
using Newtonsoft.Json;
namespace Hyperledger.Aries.Storage
{
    public partial class WalletConfiguration
    {
        public class WalletStorageCredential
        {
            [JsonProperty("account")]
            public string Account { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }

            [JsonProperty("admin_account")]
            public string AdminAccount { get; set; }

            [JsonProperty("admin_password")]
            public string AdminPassword { get; set; }
        }
    }
}
