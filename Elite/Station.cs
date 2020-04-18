using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
// ReSharper disable IdentifierTypo

namespace Elite
{
    public class StationData
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


        [JsonIgnore]
        public double Distance { get; set; }

    }

    public static class Station
    {
        public enum PoiTypes
        {
            InterStellarFactors = 1,
            RawMaterialTraders = 2,
            ManufacturedMaterialTraders = 3,
            EncodedDataTraders = 4,
            HumanTechnologyBrokers = 5,
            GuardianTechnologyBrokers = 6
        }

        public enum PowerTypes
        {
            AislingDuval,
            ArchonDelaine,
            ArissaLavignyDuval,
            DentonPatreus,
            EdmundMahon,
            FeliciaWinters,
            LiYongRui,
            PranavAntal,
            YuriGrom,
            ZacharyHudson,
            ZeminaTorval
        }

        public static List<PoiItem> FullPoiItemList = null;

        public static Dictionary<PoiTypes, List<StationData>> FullStationList = new Dictionary<PoiTypes, List<StationData>>
        {
            {PoiTypes.InterStellarFactors, new List<StationData>()},
            {PoiTypes.RawMaterialTraders, new List<StationData>()},
            {PoiTypes.ManufacturedMaterialTraders, new List<StationData>()},
            {PoiTypes.EncodedDataTraders, new List<StationData>()},
            {PoiTypes.HumanTechnologyBrokers, new List<StationData>()},
            {PoiTypes.GuardianTechnologyBrokers, new List<StationData>()}
        };

        public static Dictionary<PowerTypes, List<StationData>> FullPowerStationList = new Dictionary<PowerTypes, List<StationData>>
        {
            {PowerTypes.AislingDuval, new List<StationData>()},
            {PowerTypes.ArchonDelaine, new List<StationData>()},
            {PowerTypes.ArissaLavignyDuval, new List<StationData>()},
            {PowerTypes.DentonPatreus, new List<StationData>()},
            {PowerTypes.EdmundMahon, new List<StationData>()},
            {PowerTypes.FeliciaWinters, new List<StationData>()},
            {PowerTypes.LiYongRui, new List<StationData>()},
            {PowerTypes.PranavAntal, new List<StationData>()},
            {PowerTypes.YuriGrom, new List<StationData>()},
            {PowerTypes.ZacharyHudson, new List<StationData>()},
            {PowerTypes.ZeminaTorval, new List<StationData>()}
        };

        public static List<StationData> GetAllStations(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    return JsonConvert.DeserializeObject<List<StationData>>(File.ReadAllText(fileName));
                }
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }

            return new List<StationData>();

        }

        public static List<StationData> GetNearestStationItems(List<double> starPos, List<StationData> stationData)
        {
            if (stationData?.Any() == true && starPos?.Count == 3)
            {
                stationData.ForEach(stationItem =>
                {
                    var Xs = starPos[0];
                    var Ys = starPos[1];
                    var Zs = starPos[2];

                    var Xd = stationItem.X;
                    var Yd = stationItem.Y;
                    var Zd = stationItem.Z;

                    double deltaX = Xs - Xd;
                    double deltaY = Ys - Yd;
                    double deltaZ = Zs - Zd;

                    stationItem.Distance = (double) Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                });

                return stationData.Where(x => x.Distance >= 0).OrderBy(x => x.Distance).Take(5).ToList();
            }

            return new List<StationData>();

        }
    }
}
