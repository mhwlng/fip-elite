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

    public static class HotspotSystems
    {
        public enum MaterialTypes
        {
            Painite = 0,
            LTD = 2

        }

        public static Dictionary<MaterialTypes, List<HotspotSystemData>> FullHotspotSystemsList = new Dictionary<MaterialTypes, List<HotspotSystemData>>
        {
            {MaterialTypes.Painite, new List<HotspotSystemData>()},
            {MaterialTypes.LTD, new List<HotspotSystemData>()}
        };

        public class HotspotSystemCoordsData
        {
            [JsonProperty("x")]
            public double X { get; set; }

            [JsonProperty("y")]
            public double Y { get; set; }

            [JsonProperty("z")]
            public double Z { get; set; }
        }

        public class HotspotSystemData
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("comment")]
            public string Comment { get; set; }

            [JsonProperty("coords")]
            public HotspotSystemCoordsData Coords { get; set; }

            [JsonIgnore]
            public double Distance { get; set; }
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static List<HotspotSystemData> GetAllHotspotSystems(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    var data = JsonConvert.DeserializeObject<List<HotspotSystemData>>(File.ReadAllText(fileName));

                    data.ForEach(x => { x.Comment = StripHTML(x.Comment); });
                    return data;
                }
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }

            return new List<HotspotSystemData>();
        }

        public static List<HotspotSystemData> GetNearestHotspotSystems(List<double> starPos, List<HotspotSystemData> data)
        {
            if (data?.Any() == true && starPos?.Count == 3)
            {
                data.ForEach(systemItem =>
                {
                    var Xs = starPos[0];
                    var Ys = starPos[1];
                    var Zs = starPos[2];

                    var Xd = systemItem.Coords.X;
                    var Yd = systemItem.Coords.Y;
                    var Zd = systemItem.Coords.Z;

                    double deltaX = Xs - Xd;
                    double deltaY = Ys - Yd;
                    double deltaZ = Zs - Zd;

                    systemItem.Distance = (double) Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                });

                return data.Where(x => x.Distance >= 0).OrderBy(x => x.Distance)/*.Take(10)*/.ToList();
            }

            return new List<HotspotSystemData>();

        }
    }
}
