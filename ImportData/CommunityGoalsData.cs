using System.Collections.Generic;
using Newtonsoft.Json;

namespace ImportData
{


    public class CommunityGoalsData
    {
        [JsonProperty("activeInitiatives")]
        public List<CommunityGoal> ActiveInitiatives { get; set; }
    }

    public class CommunityGoal
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("expiry")]
        public string Expiry { get; set; }
        [JsonProperty("market_name")]
        public string MarketName { get; set; }
        [JsonProperty("starsystem_name")]
        public string StarsystemName { get; set; }
        [JsonProperty("activityType")]
        public string ActivityType { get; set; }
        [JsonProperty("target_commodity_list")]
        public string TargetCommodityList { get; set; }
        [JsonProperty("target_qty")]
        public string TargetQty { get; set; }
        [JsonProperty("qty")]
        public string Qty { get; set; }
        [JsonProperty("objective")]
        public string Objective { get; set; }
        [JsonProperty("news")]
        public string News { get; set; }
        [JsonProperty("bulletin")]
        public string Bulletin { get; set; }
    }



}
