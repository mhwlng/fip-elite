using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable IdentifierTypo

namespace Elite
{

    public static class MiningStations
    {
        public enum MaterialTypes
        {
            Painite = 1,
            LTD = 3
        }

        public static Dictionary<MaterialTypes, List<MiningStationData>> FullMiningStationsList = new Dictionary<MaterialTypes, List<MiningStationData>>
        {
            {MaterialTypes.Painite, new List<MiningStationData>()},
            {MaterialTypes.LTD, new List<MiningStationData>()}
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
            [JsonProperty("ago_sec")]
            public int AgoSec { get; set; }
            [JsonProperty("demand")]
            public int Demand { get; set; }

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
                App.log.Error(ex);
            }

            return new List<MiningStationData>();
        }

        public static List<MiningStationData> GetNearestMiningStations(List<double> starPos, List<MiningStationData> data)
        {
            if (data?.Any() == true && starPos?.Count == 3)
            {
                data.ForEach(systemItem =>
                {
                    var Xs = starPos[0];
                    var Ys = starPos[1];
                    var Zs = starPos[2];

                    var Xd = systemItem.X;
                    var Yd = systemItem.Y;
                    var Zd = systemItem.Z;

                    double deltaX = Xs - Xd;
                    double deltaY = Ys - Yd;
                    double deltaZ = Zs - Zd;

                    systemItem.Distance = (double) Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                });

                return data.Where(x => x.Distance >= 0)
                    .OrderByDescending(x => x.Price).ThenBy(x => x.AgoSec)/*.OrderBy(x => x.Distance).Take(10)*/.ToList();
            }

            return new List<MiningStationData>();

        }
    }
}
