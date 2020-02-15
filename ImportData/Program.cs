using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;


            using (StreamWriter sw = new StreamWriter(fileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                var stationsData = GetStationsData(stations);

                serializer.Serialize(writer, stationsData);
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

                string jsonText;

                Console.WriteLine("downloading populated systems from EDDB");

                if (File.Exists("populatedsystemsEDDB.json"))
                {
                    jsonText = File.ReadAllText("populatedsystemsEDDB.json");
                }
                else
                {
                    jsonText = GetJson("https://eddb.io/archive/v6/systems_populated.json");

                    File.WriteAllText("populatedsystemsEDDB.json", jsonText);
                }

                populatedSystemsEDDB = JsonConvert.DeserializeObject<List<PopulatedSystemEDDB>>(jsonText)
                    .Where(x => x.EdsmId != null)
                    .ToDictionary(x => (int)x.EdsmId);

                Console.WriteLine("downloading station list from EDSM");

                if (File.Exists("stationsEDSM.json"))
                {
                    jsonText = File.ReadAllText("stationsEDSM.json");
                }
                else
                {
                    jsonText = GetJson("https://www.edsm.net/dump/stations.json.gz");

                    File.WriteAllText("stationsEDSM.json", jsonText);
                }

                stationsEDSM = JsonConvert.DeserializeObject<List<StationEDSM>>(jsonText);

                Console.WriteLine("downloading station list from EDDB");

                if (File.Exists("stationsEDDB.json"))
                {
                    jsonText = File.ReadAllText("stationsEDDB.json");
                }
                else
                {
                    jsonText = GetJson("https://eddb.io/archive/v6/stations.json");

                    File.WriteAllText("stationsEDDB.json", jsonText);
                }

                // there are multiple stations with the same name ???
                stationsEDDB = JsonConvert.DeserializeObject<List<StationEDDB>>(jsonText)
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
                
                Console.WriteLine("finding interstellar factors");

                var interStellarFactors = stationsEDSM
                    .Where(x => x.OtherServices.Any(y => y == "Interstellar Factors Contact") &&
                                x.PopulatedSystemEDDB != null &&
                                x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                x.AdditionalStationDataEDDB.MaxLandingPadSize=="L").ToList();

                StationSerialize(interStellarFactors, "interstellarfactors.json");

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

                StationSerialize(rawMaterialTraders, "rawmaterialtraders.json");

                Console.WriteLine("finding manufactured material traders");

                //Manufactured material trader
                //Found in systems with medium-high security, an 'industrial' economy
                var manufacturedMaterialEconomies = new List<string> { "Industrial" };
                var manufacturedMaterialTraders = stationsEDSM
                    .Where(x => manufacturedMaterialEconomies.Contains(x.Economy) && 
                                x.OtherServices.Any(y => y == "Material Trader") &&
                                x.PopulatedSystemEDDB != null &&
                                x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                StationSerialize(manufacturedMaterialTraders, "manufacturedmaterialtraders.json");

                Console.WriteLine("finding encoded data traders");

                //Encoded data trader
                //Found in systems with medium-high security, a 'high tech' or 'military' economy
                var encodedDataEconomies = new List<string> { "High Tech" ,"Military"};
                var encodedDataTraders = stationsEDSM
                    .Where(x => encodedDataEconomies.Contains(x.Economy) && 
                                x.OtherServices.Any(y => y == "Material Trader") &&
                                x.PopulatedSystemEDDB != null &&
                                x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                StationSerialize(encodedDataTraders, "encodeddatatraders.json");

                Console.WriteLine("finding human technology brokers");

                //Human Technology Broker
                //Found in systems with an 'Industrial' economy
                var humanTechnologyEconomies = new List<string> { "Industrial" };
                var humanTechnologyBrokers = stationsEDSM
                    .Where(x => humanTechnologyEconomies.Contains(x.Economy) && 
                                x.OtherServices.Any(y => y == "Technology Broker") &&
                                x.PopulatedSystemEDDB != null &&
                                x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                StationSerialize(humanTechnologyBrokers, "humantechnologybrokers.json");

                Console.WriteLine("finding guardian technology brokers");

                //Guardian Technology Broker
                //Found in systems with a 'high tech' economy
                var guardianTechnologyEconomies = new List<string> { "High Tech"};
                var guardianTechnologyBrokers = stationsEDSM
                    .Where(x => guardianTechnologyEconomies.Contains(x.Economy) && 
                                x.OtherServices.Any(y => y == "Technology Broker") &&
                                x.PopulatedSystemEDDB != null &&
                                x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                StationSerialize(guardianTechnologyBrokers, "guardiantechnologybrokers.json");


            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }
    }
}
