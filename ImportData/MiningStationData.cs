using Newtonsoft.Json;

namespace ImportData
{
    public class MiningStationData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("systemName")]
        public string SystemName { get; set; }

        [JsonProperty("distanceToArrival")]
        public double? DistanceToArrival { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("allegiance")]
        public string Allegiance { get; set; }
        [JsonProperty("government")]
        public string Government { get; set; }
        [JsonProperty("economy")]
        public string Economy { get; set; }
        [JsonProperty("faction")]
        public string Faction { get; set; }

        [JsonProperty("systemsecurity")]
        public string SystemSecurity { get; set; }

        [JsonProperty("systempopulation")]
        public long? SystemPopulation { get; set; }

        [JsonProperty("powerplaystate")]
        public string PowerplayState { get; set; }

        [JsonProperty("powers")]
        public string Powers { get; set; }
        
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
        
        [JsonProperty("price")]
        public int Price { get; set; }
        [JsonProperty("pad")]
        public string Pad { get; set; }
        [JsonProperty("ago")]
        public string Ago { get; set; }
        [JsonProperty("ago_sec")]
        public int AgoSec { get; set; }
        [JsonProperty("demand")]
        public int Demand { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }

        [JsonProperty("economies")]
        public string Economies { get; set; }

        [JsonProperty("systemstate")]
        public string SystemState { get; set; }

    }
}
