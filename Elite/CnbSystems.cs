using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

// ReSharper disable IdentifierTypo

namespace Elite
{

    public static class CnbSystems
    {
        public static List<CnbSystemData> FullCnbSystemsList = null;

        public class CnbSystemData
        {
            [JsonProperty("x")]
            [DefaultValue(0)]
            public double X { get; set; }

            [JsonProperty("y")]
            [DefaultValue(0)]
            public double Y { get; set; }

            [JsonProperty("z")]
            [DefaultValue(0)]
            public double Z { get; set; }

            [JsonProperty("beac")]
            public string CompromisedNavBeacon { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("systemsecurity")]
            public string SystemSecurity { get; set; }

            [JsonProperty("systempopulation")]
            [DefaultValue(0)]
            public long SystemPopulation { get; set; }

            [JsonProperty("allegiance")]
            public string Allegiance { get; set; }

            [JsonProperty("primary_economy")]
            public string PrimaryEconomy { get; set; }

            [JsonProperty("government")]
            public string Government { get; set; }
            
            [JsonIgnore]
            public double Distance { get; set; }

        }

        public static List<CnbSystemData> GetAllCnbSystems(string path)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<List<CnbSystemData>>(File.ReadAllText(path));
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new List<CnbSystemData>();
        }

        public static List<CnbSystemData> GetNearestCnbSystems(List<double> starPos)
        {
            if (FullCnbSystemsList?.Any() == true && starPos?.Count == 3)
            {
                FullCnbSystemsList.ForEach(systemItem =>
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

                return FullCnbSystemsList.Where(x => x.Distance >= 0).OrderBy(x => x.Distance).Take(10).ToList();
            }

            return new List<CnbSystemData>();

        }
    }
}
