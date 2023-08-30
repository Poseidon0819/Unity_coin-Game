using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace MetaMask.Models
{
    public class MetaMaskTokenCheckRequest
    {

        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonProperty("options")]
        [JsonPropertyName("options")]
        public object Options { get; set; }
    }

    public class MetaMaskTokenCheckOptions
    {
        [JsonProperty("address")]
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonProperty("tokenId")]
        [JsonPropertyName("tokenId")]
        public object TokenId { get; set; }
    }
    
}