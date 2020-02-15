using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ImportData
{
    public class UpdateTime
    {

        [JsonProperty("information")]
        public string Information { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("shipyard")]
        public string Shipyard { get; set; }

        [JsonProperty("outfitting")]
        public string Outfitting { get; set; }
    }

    public class ControllingFaction
    {

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Body
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }
    }

    public class StationEDSM
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("marketId")]
        public long? MarketId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("distanceToArrival")]
        public double? DistanceToArrival { get; set; }

        [JsonProperty("allegiance")]
        public string Allegiance { get; set; }

        [JsonProperty("government")]
        public string Government { get; set; }

        [JsonProperty("economy")]
        public string Economy { get; set; }

        [JsonProperty("secondEconomy")]
        public string SecondEconomy { get; set; }

        [JsonProperty("haveMarket")]
        public bool HaveMarket { get; set; }

        [JsonProperty("haveShipyard")]
        public bool HaveShipyard { get; set; }

        [JsonProperty("haveOutfitting")]
        public bool HaveOutfitting { get; set; }

        [JsonProperty("otherServices")]
        public IList<string> OtherServices { get; set; }

        [JsonProperty("updateTime")]
        public UpdateTime UpdateTime { get; set; }

        [JsonProperty("systemId")]
        public int SystemId { get; set; }

        [JsonProperty("systemId64")]
        public object SystemId64 { get; set; }

        [JsonProperty("systemName")]
        public string SystemName { get; set; }

        [JsonProperty("controllingFaction")]
        public ControllingFaction ControllingFaction { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }

        [JsonIgnore]
        public PopulatedSystemEDDB PopulatedSystemEDDB { get; set; }

        [JsonIgnore]
        public StationEDDB AdditionalStationDataEDDB { get; set; }

    }

}
