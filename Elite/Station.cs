using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            InterStellarFactors = 2,
            RawMaterialTraders = 3,
            ManufacturedMaterialTraders = 4,
            EncodedDataTraders = 5,
            HumanTechnologyBrokers = 6,
            GuardianTechnologyBrokers = 7
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

        public static List<StationData> GetAllStations(string path)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<List<StationData>>(File.ReadAllText(path));
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new List<StationData>();

        }

        public static List<StationData> GetNearestStations(List<double> starPos, List<StationData> stationData)
        {
            if (stationData?.Any() == true && starPos?.Count == 3)
            {
                stationData.ForEach(stationItem =>
                {
                    var xs = starPos[0];
                    var ys = starPos[1];
                    var zs = starPos[2];

                    var xd = stationItem.X;
                    var yd = stationItem.Y;
                    var zd = stationItem.Z;

                    var deltaX = xs - xd;
                    var deltaY = ys - yd;
                    var deltaZ = zs - zd;

                    stationItem.Distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                });

                return stationData.Where(x => x.Distance >= 0).OrderBy(x => x.Distance).Take(5).ToList();
            }

            return new List<StationData>();

        }
    }
}
