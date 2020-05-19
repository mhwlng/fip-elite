using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable IdentifierTypo

namespace Elite
{

    public static class CnbSystems
    {
        public static List<CnbSystemData> FullCnbSystemsList = null;

        public class CnbSystemData
        {
            [JsonProperty("x")]
            public double X { get; set; }

            [JsonProperty("y")]
            public double Y { get; set; }

            [JsonProperty("z")]
            public double Z { get; set; }

            [JsonProperty("beac")]
            public string CompromisedNavBeacon { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("systemsecurity")]
            public string SystemSecurity { get; set; }

            [JsonProperty("systempopulation")]
            public long? SystemPopulation { get; set; }

            [JsonProperty("powerplaystate")]
            public string PowerplayState { get; set; }

            [JsonProperty("powers")]
            public string Powers { get; set; }

            [JsonProperty("allegiance")]
            public string Allegiance { get; set; }

            [JsonProperty("primary_economy")]
            public string PrimaryEconomy { get; set; }

            [JsonProperty("government")]
            public string Government { get; set; }

            [JsonProperty("controlling_minor_faction")]
            public string ControllingMinorFaction { get; set; }


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
                App.log.Error(ex);
            }

            return new List<CnbSystemData>();
        }

        public static List<CnbSystemData> GetNearestCnbSystems(List<double> starPos)
        {
            if (FullCnbSystemsList?.Any() == true && starPos?.Count == 3)
            {
                FullCnbSystemsList.ForEach(systemItem =>
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

                return FullCnbSystemsList.Where(x => x.Distance >= 0).OrderBy(x => x.Distance).Take(5).ToList();
            }

            return new List<CnbSystemData>();

        }
    }
}
