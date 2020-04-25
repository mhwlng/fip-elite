using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

// ReSharper disable StringLiteralTypo

namespace ImportData
{
    class Program
    {
        public static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static byte[] Decompress(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip),
                CompressionMode.Decompress))
            {
                const int size = 100000;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        public static string GetJson(string url)
        {
            using (var client = new WebClient())
            {

                client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                var data = client.DownloadData(url);
                var decompress = Decompress(data);
                return System.Text.Encoding.UTF8.GetString(decompress);
            }
        }

        public static List<StationData> GetStationsData(List<StationEDSM> stations)
        {
            return stations.Select(x => new StationData
            {
                Name = x.Name,
                DistanceToArrival = x.DistanceToArrival,
                Type = x.Type,

                SystemName = x.SystemName,
                SystemSecurity = x.PopulatedSystemEDDB?.Security,
                SystemPopulation = x.PopulatedSystemEDDB?.Population,

                PowerplayState = x.PopulatedSystemEDDB?.PowerState,
                Powers = x.PopulatedSystemEDDB?.Power,

                Allegiance = x.Allegiance,
                Government = x.Government,
                Economy = x.Economy,
                Faction = x.ControllingFaction?.Name,

                X = x.PopulatedSystemEDDB?.X ?? 0,
                Y = x.PopulatedSystemEDDB?.Y ?? 0,
                Z = x.PopulatedSystemEDDB?.Z ?? 0

            }).ToList();

        }

        public static void StationSerialize(List<StationEDSM> stations, string fileName)
        {
            (new FileInfo(fileName)).Directory?.Create();

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(fileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                var stationsData = GetStationsData(stations);

                serializer.Serialize(writer, stationsData);
            }
        }

        public static void DeleteExpiredFile(string path)
        {

            (new FileInfo(path)).Directory?.Create();

            if (File.Exists(path))
            {
                var modification = File.GetLastWriteTime(path);

                if ((DateTime.Now - modification).TotalHours >= 24)
                {
                    File.Delete(path);
                }
            }
        }


        public static string DownloadJson(string path, string url, ref bool wasUpdated)
        {

            DeleteExpiredFile(path);

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            else
            {
                var jsonText = GetJson(url);

                File.WriteAllText(path, jsonText);

                wasUpdated = true;

                return jsonText;
            }
        }

        public class CNBSystemData
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

        }

        public static List<CNBSystemData> GetCnbSystems(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var data = client.DownloadString(url);

                    var jObj = JObject.Parse(data);

                    var systemInfo = jObj.ToObject<Dictionary<string, CNBSystemData>>();

                    return systemInfo.Where(x => x.Value.CompromisedNavBeacon == "1").Select(x => new CNBSystemData
                    {
                        CompromisedNavBeacon = x.Value.CompromisedNavBeacon,
                        X = x.Value.X,
                        Y = x.Value.Y,
                        Z = x.Value.Z,
                        Name = x.Key
                    }).ToList();

                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return new List<CNBSystemData>();

        }

        public static void CnbSystemSerialize(List<CNBSystemData> systems, string fileName)
        {
            (new FileInfo(fileName)).Directory?.Create();

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(fileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, systems);
            }
        }

        public class PainiteLocationData
        {
            [JsonProperty("x")]
            public double X { get; set; }

            [JsonProperty("y")]
            public double Y { get; set; }

            [JsonProperty("z")]
            public double Z { get; set; }

        }

        public class PainiteSystemData
        {
            [JsonProperty("name")]
            public double Name { get; set; }

            [JsonProperty("comment")]
            public string Comment { get; set; }

            [JsonProperty("coords")]
            public PainiteLocationData Coords { get; set; }
        }


        public static void GetHotspotSystems(string path, string url, string material)
        {
            try
            {
                DeleteExpiredFile(path);

                if (!File.Exists(path))
                {
                    Console.WriteLine("looking up " + material + " Hotspots");

                    using (var client = new WebClient())
                    {
                        var data = client.DownloadString(url+material);

                        File.WriteAllText(path, data);

                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        static void Main(string[] args)
        {
            log.Info("ImportData started");

            try
            {
                List<StationEDSM> stationsEDSM = null;
                Dictionary<string,StationEDDB> stationsEDDB = null;
                Dictionary<int, PopulatedSystemEDDB> populatedSystemsEDDB = null;

                var wasAnyUpdated = false;

                Console.WriteLine("downloading populated systems from EDDB");

                var jsonPopulatedsystemsEDDBText = DownloadJson(@"Data\populatedsystemsEDDB.json", "https://eddb.io/archive/v6/systems_populated.json", ref wasAnyUpdated);

                Console.WriteLine("downloading station list from EDSM");

                var jsonStationsEDSMText = DownloadJson(@"Data\stationsEDSM.json", "https://www.edsm.net/dump/stations.json.gz", ref wasAnyUpdated);

                Console.WriteLine("downloading station list from EDDB");

                var jsonStationsEDDBText = DownloadJson(@"Data\stationsEDDB.json", "https://eddb.io/archive/v6/stations.json", ref wasAnyUpdated);

                if (wasAnyUpdated)
                {
                    Console.WriteLine("checking station and system data");

                    populatedSystemsEDDB = JsonConvert.DeserializeObject<List<PopulatedSystemEDDB>>(jsonPopulatedsystemsEDDBText)
                        .Where(x => x.EdsmId != null)
                        .ToDictionary(x => (int)x.EdsmId);

                    stationsEDSM = JsonConvert.DeserializeObject<List<StationEDSM>>(jsonStationsEDSMText);

                    // there are multiple stations with the same name ???
                    stationsEDDB = JsonConvert.DeserializeObject<List<StationEDDB>>(jsonStationsEDDBText)
                        .GroupBy(x => x.Name).Select(x => x.First())
                        .ToDictionary(x => x.Name);

                    Console.WriteLine("looking up additional EDDB station information for all stations");

                    stationsEDSM.ForEach(z =>
                    {
                        if (stationsEDDB.ContainsKey(z.Name))
                        {
                            z.AdditionalStationDataEDDB = stationsEDDB[z.Name];
                        }
                    });

                    Console.WriteLine("looking up EDDB system information for all stations");

                    stationsEDSM.ForEach(z =>
                    {
                        if (populatedSystemsEDDB.ContainsKey(z.SystemId))
                        {
                            z.PopulatedSystemEDDB = populatedSystemsEDDB[z.SystemId];
                        }
                    });

                    //-------------------------

                    Console.WriteLine("finding Aisling Duval stations");
                    var aislingDuval = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Aisling Duval" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(aislingDuval, @"Data\aislingduval.json");

                    Console.WriteLine("finding Archon Delaine stations");
                    var archonDelaine = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Archon Delaine" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(archonDelaine, @"Data\archondelaine.json");

                    Console.WriteLine("finding Arissa Lavigny-Duval stations");
                    var arissaLavignyDuval = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Arissa Lavigny-Duval" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(arissaLavignyDuval, @"Data\arissalavignyduval.json");

                    Console.WriteLine("finding Denton Patreus stations");
                    var dentonPatreus = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Denton Patreus" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(dentonPatreus, @"Data\dentonpatreus.json");

                    Console.WriteLine("finding Edmund Mahon stations");
                    var edmundMahon = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Edmund Mahon" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(edmundMahon, @"Data\edmundmahon.json");

                    Console.WriteLine("finding Felicia Winters stations");
                    var feliciaWinters = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Felicia Winters" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(feliciaWinters, @"Data\feliciawinters.json");

                    Console.WriteLine("finding Li Yong-Rui stations");
                    var liYongRui = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Li Yong-Rui" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(liYongRui, @"Data\liyongrui.json");

                    Console.WriteLine("finding Pranav Antal stations");
                    var pranavAntal = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Pranav Antal" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(pranavAntal, @"Data\pranavantal.json");

                    Console.WriteLine("finding Yuri Grom stations");
                    var yuriGrom = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Yuri Grom" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(yuriGrom, @"Data\yurigrom.json");

                    Console.WriteLine("finding Zachary Hudson stations");
                    var zacharyHudson = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Zachary Hudson" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(zacharyHudson, @"Data\zacharyhudson.json");

                    Console.WriteLine("finding Zemina Torval stations");
                    var zeminaTorval = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Zemina Torval" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(zeminaTorval, @"Data\zeminatorval.json");

                    //----------------

                    Console.WriteLine("finding interstellar factors");

                    var interStellarFactors = stationsEDSM
                        .Where(x => x.OtherServices.Any(y => y == "Interstellar Factors Contact") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(interStellarFactors, @"Data\interstellarfactors.json");

                    Console.WriteLine("finding raw material traders");

                    //Raw material trader
                    //Found in systems with medium-high security, an 'extraction' or 'refinery' economy
                    var rawMaterialEconomies = new List<string> {"Extraction", "Refinery"};
                    var rawMaterialTraders = stationsEDSM
                        .Where(x => rawMaterialEconomies.Contains(x.Economy) &&
                                    x.OtherServices.Any(y => y == "Material Trader") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(rawMaterialTraders, @"Data\rawmaterialtraders.json");

                    Console.WriteLine("finding manufactured material traders");

                    //Manufactured material trader
                    //Found in systems with medium-high security, an 'industrial' economy
                    var manufacturedMaterialEconomies = new List<string> {"Industrial"};
                    var manufacturedMaterialTraders = stationsEDSM
                        .Where(x => manufacturedMaterialEconomies.Contains(x.Economy) &&
                                    x.OtherServices.Any(y => y == "Material Trader") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(manufacturedMaterialTraders, @"Data\manufacturedmaterialtraders.json");

                    Console.WriteLine("finding encoded data traders");

                    //Encoded data trader
                    //Found in systems with medium-high security, a 'high tech' or 'military' economy
                    var encodedDataEconomies = new List<string> {"High Tech", "Military"};
                    var encodedDataTraders = stationsEDSM
                        .Where(x => encodedDataEconomies.Contains(x.Economy) &&
                                    x.OtherServices.Any(y => y == "Material Trader") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(encodedDataTraders, @"Data\encodeddatatraders.json");

                    Console.WriteLine("finding human technology brokers");

                    //Human Technology Broker
                    //Found in systems with an 'Industrial' economy
                    var humanTechnologyEconomies = new List<string> {"Industrial"};
                    var humanTechnologyBrokers = stationsEDSM
                        .Where(x => humanTechnologyEconomies.Contains(x.Economy) &&
                                    x.OtherServices.Any(y => y == "Technology Broker") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(humanTechnologyBrokers, @"Data\humantechnologybrokers.json");

                    Console.WriteLine("finding guardian technology brokers");

                    //Guardian Technology Broker
                    //Found in systems with a 'high tech' economy
                    var guardianTechnologyEconomies = new List<string> {"High Tech"};
                    var guardianTechnologyBrokers = stationsEDSM
                        .Where(x => guardianTechnologyEconomies.Contains(x.Economy) &&
                                    x.OtherServices.Any(y => y == "Technology Broker") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(guardianTechnologyBrokers, @"Data\guardiantechnologybrokers.json");
                }

                //--------------------------

                // Compromised Nav Beacons

                const string cnbPath = @"Data\cnbsystems.json";

                DeleteExpiredFile(cnbPath);

                if (!File.Exists(cnbPath))
                {
                    Console.WriteLine("looking up Compromised Nav Beacons");

                    var populatedSystemsByNameEDDB = JsonConvert
                        .DeserializeObject<List<PopulatedSystemEDDB>>(jsonPopulatedsystemsEDDBText)
                        .ToDictionary(x => x.Name);

                    var cnbSystems = GetCnbSystems("http://edtools.ddns.net/res.json");

                    cnbSystems.ForEach(z =>
                    {
                        if (populatedSystemsByNameEDDB.ContainsKey(z.Name))
                        {
                            var systemInfo = populatedSystemsByNameEDDB[z.Name];

                            z.SystemSecurity = systemInfo.Security;
                            z.SystemPopulation = systemInfo.Population;
                            z.PowerplayState = systemInfo.PowerState;
                            z.Powers = systemInfo.Power;
                            z.PrimaryEconomy = systemInfo.PrimaryEconomy;
                            z.Government = systemInfo.Government;
                            z.ControllingMinorFaction = systemInfo.ControllingMinorFaction;
                            z.Allegiance = systemInfo.Allegiance;
                        }
                    });

                    CnbSystemSerialize(cnbSystems, cnbPath);
                }

                //--------------------------

                 GetHotspotSystems(@"Data\painitesystems.json", "http://edtools.ddns.net/miner?a=r&n=", "Painite");
                 GetHotspotSystems(@"Data\ltdsystems.json", "http://edtools.ddns.net/miner?a=r&n=", "LTD");


            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            log.Info("ImportData ended");
        }
    }
}
