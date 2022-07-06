using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ConsoleApp2
{


    public class ClientGetData
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("ap_mac")]
        public string APMac { get; set; }
        [JsonPropertyName("essid")]
        public string Essid { get; set; }

    }

    public class ClientGetResponce
    {

        [JsonProperty("data")]
        public List<ClientGetData> Data { get; set; }
    }

    public class AccessPointGetData 
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mac")]
        public string Mac { get; set; }
    }

    public class AccessPointGetResponce
    {
        [JsonProperty("data")]
        public List<AccessPointGetData> Data { get; set; }
    }
}
