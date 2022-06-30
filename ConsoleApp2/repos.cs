using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    internal class ReposToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class ReposData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("maxoccupancy")]
        public int MaxOccupancy { get; set; }

        [JsonPropertyName("sumins")]
        public int SumIns { get; set; }

        [JsonPropertyName("sumouts")]
        public int SumOuts { get; set; }

        [JsonPropertyName("recordDate_hour_1")]
        public DateTime DateTime { get; set; }
    }

    public class OccupancyResponce
    {
        [JsonPropertyName("messages")]
        public List<string> Messages { get; set; }

        [JsonPropertyName("results")]
        public List<ReposData> Results { get; set; }
    }

    public class UnifyPostData
    {
        [JsonPropertyName("rc")]
        public List<string> Rc { get; set; }
    }

    public class UnifyPostResponce
    {
        [JsonPropertyName("meta")]
        public List<UnifyPostData> Meta { get; set; }

        [JsonPropertyName("data")]
        public List<string> Data { get; set; }
    }

    public class UnifyGetData
    {
        [JsonPropertyName("site_id")]
        public string SiteId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }
    }

    public class UnifyGetResponce
    {
        [JsonPropertyName("meta")]
        public List<UnifyPostData> Meta { get; set; }

        [JsonPropertyName("data")]
        public List<UnifyGetData> Data { get; set; }
    }
}
