using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

// ReSharper disable IdentifierTypo

namespace Elite
{

    public static class MiningStations
    {
        public enum MaterialTypes
        {
            Painite = 1,
            LTD = 3,
            Platinum = 5,
            TritiumBuy = 6,
            TritiumSell = 7
        }

        public static Dictionary<MaterialTypes, List<MiningStationData>> FullMiningStationsList = new Dictionary<MaterialTypes, List<MiningStationData>>
        {
            {MaterialTypes.Painite, new List<MiningStationData>()},
            {MaterialTypes.LTD, new List<MiningStationData>()},
            {MaterialTypes.Platinum, new List<MiningStationData>()},
            {MaterialTypes.TritiumBuy, new List<MiningStationData>()},
            {MaterialTypes.TritiumSell, new List<MiningStationData>()}
        };

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
            [DefaultValue(0)]
            public long SystemPopulation { get; set; }

            [JsonProperty("powerplaystate")]
            public string PowerplayState { get; set; }

            [JsonProperty("powers")]
            public string Powers { get; set; }


            [JsonProperty("x")]
            [DefaultValue(0)]
            public double X { get; set; }

            [JsonProperty("y")]
            [DefaultValue(0)]
            public double Y { get; set; }

            [JsonProperty("z")]
            [DefaultValue(0)]
            public double Z { get; set; }


            [JsonProperty("price")]
            public int Price { get; set; }
            [JsonProperty("pad")]
            public string Pad { get; set; }
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

            [JsonIgnore]
            public double Distance { get; set; }

            [JsonIgnore]
            public DateTime UpdatedTime { get; set; }

        }

        public static List<MiningStationData> GetAllMiningStations(string path)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    var data = JsonConvert.DeserializeObject<List<MiningStationData>>(File.ReadAllText(path));

                    var modification = File.GetLastWriteTime(path);

                    foreach (var d in data)
                    {
                        d.UpdatedTime = modification;
                    }

                    return data;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new List<MiningStationData>();
        }

        public static List<MiningStationData> GetNearestMiningStations(List<double> starPos, List<MiningStationData> data, bool sell)
        {
            if (data?.Any() == true && starPos?.Count == 3)
            {
                data.ForEach(systemItem =>
                {
                    var xs = starPos[0];
                    var ys = starPos[1];
                    var zs = starPos[2];

                    var xd = systemItem.X;
                    var yd = systemItem.Y;
                    var zd = systemItem.Z;

                    var deltaX = xs - xd;
                    var deltaY = ys - yd;
                    var deltaZ = zs - zd;

                    systemItem.Distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                });

                if (sell)
                    return data.Where(x => x.Distance >= 0)
                        .OrderByDescending(x => x.Price).ThenBy(x => x.AgoSec)/*.OrderBy(x => x.Distance)*/.Take(20).ToList();
                else
                    return data.Where(x => x.Distance >= 0)
                        .OrderBy(x => x.Price).ThenBy(x => x.AgoSec)/*.OrderBy(x => x.Distance)*/.Take(20).ToList();
            }

            return new List<MiningStationData>();

        }


    }
}
