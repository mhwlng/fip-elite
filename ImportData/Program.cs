using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI;
using HtmlAgilityPack;
using ImportData;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable StringLiteralTypo

namespace ImportData
{
    public static class JsonReaderExtensions
    {
        private static HttpClientHandler httpClientHandler = new HttpClientHandler
        { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };

        public static readonly HttpClient WebClient = new HttpClient(httpClientHandler);

        private static void DeleteExpiredFile(string fullPath, int minutes)
        {
            new FileInfo(fullPath).Directory?.Create();

            if (File.Exists(fullPath))
            {
                var modification = File.GetLastWriteTime(fullPath);

                if ((DateTime.Now - modification).TotalMinutes >= minutes)
                {
                    File.Delete(fullPath);
                }
            }
        }

        private static string GetExePath()
        {
            var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(strExeFilePath);
        }

        public static IEnumerable<T> ParseJson<T>(string path)
        {
            path = Path.Combine(GetExePath(), path);

            if (File.Exists(path))
            {
                var serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                using (var s = File.Open(path, FileMode.Open))
                {
                    using (var sr = new StreamReader(s))
                    {
                        using (var reader = new JsonTextReader(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonToken.StartObject)
                                {
                                    yield return serializer.Deserialize<T>(reader);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static async Task<bool> DownloadJson<T>(string url, string path, bool wasUpdated, bool gzip)
        {
            path = Path.Combine(GetExePath(), path);

            DeleteExpiredFile(path, 1440);

            if (!File.Exists(path))
            {
                var serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                using (var sw = new StreamWriter(File.Open(path, FileMode.Create)))
                {
                    using (var jsonWriter = new JsonTextWriter(sw))
                    {
                        //JsonReaderExtensions.WebClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                        //JsonReaderExtensions.WebClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                        using (var response = await WebClient.GetAsync(url))
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                if (gzip)
                                {
                                    using (var decompressed = new GZipStream(stream, CompressionMode.Decompress))
                                    {
                                        using (var sr = new StreamReader(decompressed))
                                        {
                                            using (var jsonReader = new JsonTextReader(sr))
                                            {
                                                while (jsonReader.Read())
                                                {
                                                    if (jsonReader.TokenType == JsonToken.StartArray)
                                                    {
                                                        jsonWriter.WriteStartArray();
                                                    }
                                                    else if (jsonReader.TokenType == JsonToken.EndArray)
                                                    {
                                                        jsonWriter.WriteEndArray();
                                                    }
                                                    else if (jsonReader.TokenType == JsonToken.StartObject)
                                                    {
                                                        var sd = serializer.Deserialize<T>(jsonReader);

                                                        serializer.Serialize(jsonWriter, sd);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    using (var sr = new StreamReader(stream))
                                    {
                                        using (var jsonReader = new JsonTextReader(sr))
                                        {
                                            while (jsonReader.Read())
                                            {
                                                if (jsonReader.TokenType == JsonToken.StartArray)
                                                {
                                                    jsonWriter.WriteStartArray();
                                                }
                                                else if (jsonReader.TokenType == JsonToken.EndArray)
                                                {
                                                    jsonWriter.WriteEndArray();
                                                }
                                                else if (jsonReader.TokenType == JsonToken.StartObject)
                                                {
                                                    var sd = serializer.Deserialize<T>(jsonReader);

                                                    serializer.Serialize(jsonWriter, sd);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                wasUpdated = true;

            }

            return wasUpdated;
        }
    }

    class Program
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string GetExePath()
        {
            var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(strExeFilePath);
        }


        private static List<StationData> GetStationsData(List<StationEDSM> stations)
        {
            return stations.Select(x => new StationData
            {
                Name = x.Name,
                DistanceToArrival = x.DistanceToArrival ?? 0,
                Type = x.Type,

                SystemName = x.SystemName,
                SystemSecurity = x.PopulatedSystemEDDB?.Security,
                SystemPopulation = x.PopulatedSystemEDDB?.Population ?? 0,

                PowerplayState = x.PopulatedSystemEDDB?.PowerState,
                Powers = x.PopulatedSystemEDDB?.Power,

                Allegiance = x.Allegiance,
                Government = x.Government,
                Economy = x.Economy,
                Economies = string.Join(",", x.AdditionalStationDataEDDB?.Economies ?? new List<string>() { x.Economy }),
                Faction = x.ControllingFaction?.Name,

                X = x.PopulatedSystemEDDB?.X ?? 0,
                Y = x.PopulatedSystemEDDB?.Y ?? 0,
                Z = x.PopulatedSystemEDDB?.Z ?? 0,

                Body = x.Body,

                MarketId = x.MarketId ?? 0,

                SystemState = string.Join(",", x.PopulatedSystemEDDB?.States?.Select(y => y.Name) ?? new List<string>())

            }).ToList();

        }

        private static void DeleteExpiredFile(string fullPath, int minutes)
        {
            new FileInfo(fullPath).Directory?.Create();

            if (File.Exists(fullPath))
            {
                var modification = File.GetLastWriteTime(fullPath);

                if ((DateTime.Now - modification).TotalMinutes >= minutes)
                {
                    File.Delete(fullPath);
                }
            }
        }

        private static void StationSerialize(List<StationEDSM> stations, string path)
        {
            path = Path.Combine(GetExePath(), path);

            new FileInfo(path).Directory?.Create();

            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            using (var sw = new StreamWriter(path))
            using (var writer = new JsonTextWriter(sw))
            {
                var stationsData = GetStationsData(stations);

                serializer.Serialize(writer, stationsData);
            }
        }


        private static bool NeedToUpdateFile(string path, int minutes)
        {
            path = Path.Combine(GetExePath(), path);

            if (File.Exists(path))
            {
                var modification = File.GetLastWriteTime(path);

                if ((DateTime.Now - modification).TotalMinutes <= minutes)
                {
                    return false;
                }
            }
            else
            {
                new FileInfo(path).Directory?.Create();
            }

            return true;
        }


        public class CNBSystemData
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

        private static async Task<List<CNBSystemData>> GetCnbSystems(string url)
        {
            try
            {
                var data = await JsonReaderExtensions.WebClient.GetStringAsync(url);

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
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return new List<CNBSystemData>();

        }

        private static void CnbSystemsSerialize(List<CNBSystemData> systems, string fullPath)
        {
            new FileInfo(fullPath).Directory?.Create();

            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            using (var sw = new StreamWriter(fullPath))
            using (var writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, systems);
            }
        }

        private static async Task DownloadCnbSystems(string path, Dictionary<string, PopulatedSystemEDDB> populatedSystemsEDDBbyName)
        {
            path = Path.Combine(GetExePath(), path);

            DeleteExpiredFile(path, 1440);

            if (!File.Exists(path))
            {
                Console.WriteLine("looking up Compromised Nav Beacons");

                var cnbSystems = await GetCnbSystems("http://edtools.cc/res.json");

                cnbSystems.ForEach(z =>
                {
                    if (populatedSystemsEDDBbyName.ContainsKey(z.Name))
                    {
                        var systemInfo = populatedSystemsEDDBbyName[z.Name];

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

                CnbSystemsSerialize(cnbSystems, path);
            }
        }

        private static async Task DownloadHotspotSystems(string path, string url, string material)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                DeleteExpiredFile(path, 1440);

                if (!File.Exists(path))
                {
                    Console.WriteLine("looking up " + material + " Hotspots");

                    var data = await JsonReaderExtensions.WebClient.GetStringAsync(url + material);

                    File.WriteAllText(path, data);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static async Task DownloadPoiGEC(string path, string url)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                DeleteExpiredFile(path, 1440);

                if (!File.Exists(path))
                {
                    Console.WriteLine("looking up GEC POIs");

                    var data = await JsonReaderExtensions.WebClient.GetStringAsync(url);

                    File.WriteAllText(path, data);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }


        private static void GalnetSerialize(List<GalnetData> galnet, string fullPath)
        {
            new FileInfo(fullPath).Directory?.Create();

            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            using (var sw = new StreamWriter(fullPath))
            using (var writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, galnet);
            }
        }

        private static void CommunityGoalSerialize(List<CommunityGoal> cg, string fullPath)
        {
            new FileInfo(fullPath).Directory?.Create();

            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            using (var sw = new StreamWriter(fullPath))
            using (var writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, cg);
            }
        }

        private static bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        private static async Task DownloadGalnet(string path, string url)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                DeleteExpiredFile(path, 720);

                if (!File.Exists(path))
                {
                    Console.WriteLine("looking up galnet");

                    var data = await JsonReaderExtensions.WebClient.GetStringAsync(url);

                    var galnetJson = JsonConvert.DeserializeObject<GalnetRoot>(data)?.Data.Select(x => x.Attributes).ToList();

                    if (galnetJson?.Any() == true)
                    {
                        galnetJson.ForEach(x =>
                        {
                            x.ImageList = new List<string>();

                            if (!string.IsNullOrEmpty(x.Image))
                            {
                                foreach (var i in x.Image.TrimStart(',').Split(',').ToList())
                                {
                                    if (!string.IsNullOrEmpty(i))
                                    {
                                        x.ImageList.Add(i);
                                    }
                                }
                            }

                            x.Image = null;

                            if (x.BodyItem != null)
                            {
                                x.Body = x.BodyItem.Value.Replace("\r\n", "<br>");

                                x.BodyItem = null;
                            }
                        });

                        GalnetSerialize(galnetJson, path);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static async Task DownloadCommunityGoals(string path, string url)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                DeleteExpiredFile(path, 180);

                if (!File.Exists(path))
                {
                    Console.WriteLine("looking up community goals");

                    var data = await JsonReaderExtensions.WebClient.GetStringAsync(url);

                    var cgJson = JsonConvert.DeserializeObject<CommunityGoalsData>(data);

                    if (cgJson != null)
                    {
                        CommunityGoalSerialize(cgJson.ActiveInitiatives, path);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }


        private class HotspotStationData
        {
            public string System { get; set; }
            public string Station { get; set; }
            public int Price { get; set; }
            public string Pad { get; set; }
            public int AgoSec { get; set; }
            public int Demand { get; set; }

            [JsonIgnore]
            public StationEDSM StationEDSM { get; set; }
        }

        private static List<MiningStationData> GetMiningStationsData(List<HotspotStationData> stations, List<StationEDSM> stationsEDSM)
        {
            if (stationsEDSM != null)
            {
                foreach (var s in stations)
                {
                    s.StationEDSM = stationsEDSM.FirstOrDefault(x => x.Name == s.Station && x.SystemName == s.System);
                }
            }

            return stations.Where(x => x.StationEDSM?.AdditionalStationDataEDDB?.IsPlanetary == false)
                .Select(x => new MiningStationData
            {
                Name = x.Station,
                SystemName = x.System,

                Price = x.Price,
                Pad = x.Pad,
                AgoSec = x.AgoSec,
                Demand = x.Demand,

                X = x.StationEDSM?.PopulatedSystemEDDB?.X ?? 0,
                Y = x.StationEDSM?.PopulatedSystemEDDB?.Y ?? 0,
                Z = x.StationEDSM?.PopulatedSystemEDDB?.Z ?? 0,

                DistanceToArrival = x.StationEDSM?.DistanceToArrival,
                Type = x.StationEDSM?.Type,

                SystemSecurity = x.StationEDSM?.PopulatedSystemEDDB?.Security,
                SystemPopulation = x.StationEDSM?.PopulatedSystemEDDB?.Population ?? 0,

                PowerplayState = x.StationEDSM?.PopulatedSystemEDDB?.PowerState,
                Powers = x.StationEDSM?.PopulatedSystemEDDB?.Power,

                Allegiance = x.StationEDSM?.Allegiance,
                Government = x.StationEDSM?.Government,
                Economy = x.StationEDSM?.Economy,
                Economies = string.Join(",", x.StationEDSM?.AdditionalStationDataEDDB?.Economies ?? new List<string>() { x.StationEDSM?.Economy }),
                Faction = x.StationEDSM?.ControllingFaction?.Name,

                Body = x.StationEDSM?.Body,

                SystemState = string.Join(",", x.StationEDSM?.PopulatedSystemEDDB?.States?.Select(y => y.Name) ?? new List<string>())

            }).ToList();
        }

        private static void MiningStationsSerialize(List<MiningStationData> stations, string fullPath)
        {
            new FileInfo(fullPath).Directory?.Create();

            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            using (var sw = new StreamWriter(fullPath))
            using (var writer = new JsonTextWriter(sw))
            {

                serializer.Serialize(writer, stations);
            }
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        private static async Task DownloadInaraMiningStationsHtml(string path, string url, string material, List<StationEDSM> stationsEDSM)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                Console.WriteLine("looking up " + material + " Stations");

                var data = await JsonReaderExtensions.WebClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(data);

                var currentTime = ConvertToUnixTimestamp(DateTime.UtcNow);


                var stationInfo = doc.DocumentNode.SelectSingleNode("//table[@class='tablesorterintab']")
                    .Descendants("tr")
                    .Skip(1)
                    .Where(tr => !tr.HasClass("hideable1") /*&& !tr.HasClass("hideable2")*/ && !tr.HasClass("hideable3"))
                    .Select(tr => tr.Elements("td").ToList())
                    .Select(td => new HotspotStationData
                    {
                        Station = td[0].Descendants("span").FirstOrDefault()?.InnerText?.Replace(" | ", "") ?? "?",
                        System = td[0].Descendants("span").Skip(1).FirstOrDefault()?.InnerText ?? "?",
                        Price = Convert.ToInt32(td[5].GetAttributeValue("data-order", "0")),
                        Demand = Convert.ToInt32(td[4].GetAttributeValue("data-order", "0")),
                        Pad = td[1].InnerText, //tr[5],
                        AgoSec = (int)(currentTime - Convert.ToInt64(td[7].GetAttributeValue("data-order", "0")))
                    })
                    .ToList();

                var stationsData = GetMiningStationsData(stationInfo, stationsEDSM);

                MiningStationsSerialize(stationsData, path);
                       
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private async Task DownloadEddbMiningStationsHtml(string path, string url, string material, int cid, List<StationEDSM> stationsEDSM, bool sell)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                Console.WriteLine("looking up " + material + " Stations");

                var data = await JsonReaderExtensions.WebClient.GetStringAsync(url + cid);

                var doc = new HtmlDocument();
                doc.LoadHtml(data);

                var id = "table-stations-max-sell";
                if (!sell)
                {
                    id = "table-stations-min-buy";
                }

                var stationInfo = doc.DocumentNode.SelectSingleNode("//table[@id='" + id + "']")
                    .Descendants("tr")
                    .Where(tr => tr.Elements("td").Count() == 7)
                    .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                    .Select(tr => new HotspotStationData
                    {
                        Station = tr[0],
                        System = tr[1],
                        Price = Convert.ToInt32(tr[2].Replace(",", "").Replace(".", "")),
                        Demand = Convert.ToInt32(tr[4].Replace(",", "").Replace(".", "")),
                        Pad = tr[5],
                        AgoSec = Convert.ToInt32(tr[6].Substring(2, tr[6].IndexOf("}", StringComparison.Ordinal) - 2))
                    })
                    .ToList();

                var stationsData = GetMiningStationsData(stationInfo, stationsEDSM);

                MiningStationsSerialize(stationsData, path);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        static async Task Main(string[] args)
        {
            Log.Info("ImportData started");

            try
            {
                List<StationEDSM> stationsEDSM = null;
                List<StationEDDB> stationsEDDBList = null;
                Dictionary<string, StationEDDB> stationsEDDB;
                List<PopulatedSystemEDDB> populatedSystemsEDDBList;
                Dictionary<int, PopulatedSystemEDDB> populatedSystemsEDDBbyEdsmId;
                Dictionary<int, PopulatedSystemEDDB> populatedSystemsEDDBbyId;

                JsonReaderExtensions.WebClient.Timeout = new TimeSpan(0, 0, 5, 0, 0);

                var wasAnyUpdated = false;

                //Console.WriteLine("downloading station list from Spansh");
                //wasAnyUpdated = await JsonReaderExtensions.DownloadJson<SystemSpansh>("https://downloads.spansh.co.uk/galaxy_stations.json.gz", @"Data\stationsSpansh.json", wasAnyUpdated, true);

                Console.WriteLine("downloading station list from EDSM");

                ///DownloadJson(@"Data\stationsEDDB.json", "https://eddb.io/archive/v6/stations.json", ref wasAnyUpdated);
                //JsonReaderExtensions.DownloadJson<StationEDDB>("https://eddb.io/archive/v6/stations.json", @"Data\stationsEDDB.json", ref wasAnyUpdated);
                wasAnyUpdated = await JsonReaderExtensions.DownloadJson<StationEDSM>("https://www.edsm.net/dump/stations.json.gz", @"Data\stationsEDSM.json", wasAnyUpdated, true);

                Console.WriteLine("downloading populated systems from EDDB");

                //DownloadJson(@"Data\stationsEDSM.json", "https://www.edsm.net/dump/stations.json.gz", ref wasAnyUpdated);
                //JsonReaderExtensions.DownloadJson<StationEDSM>("https://www.edsm.net/dump/stations.json.gz", @"Data\stationsEDSM.json", ref wasAnyUpdated);
                wasAnyUpdated = await JsonReaderExtensions.DownloadJson<PopulatedSystemEDDB>("https://eddb.io/archive/v6/systems_populated.json", @"Data\populatedsystemsEDDB.json", wasAnyUpdated, false);

                Console.WriteLine("downloading station list from EDDB");

                //DownloadJson(@"Data\populatedsystemsEDDB.json", "https://eddb.io/archive/v6/systems_populated.json", ref wasAnyUpdated);
                //JsonReaderExtensions.DownloadJson<PopulatedSystemEDDB>("https://eddb.io/archive/v6/systems_populated.json", @"Data\populatedsystemsEDDB.json", ref wasAnyUpdated);
                wasAnyUpdated = await JsonReaderExtensions.DownloadJson<StationEDDB>("https://eddb.io/archive/v6/stations.json", @"Data\stationsEDDB.json", wasAnyUpdated, false);

                Console.WriteLine("checking station and system data");

                populatedSystemsEDDBList = JsonReaderExtensions.ParseJson<PopulatedSystemEDDB>(@"Data\populatedsystemsEDDB.json").ToList();

                var populatedSystemsEDDBbyName = populatedSystemsEDDBList
                    .ToDictionary(x => x.Name);

                if (NeedToUpdateFile(@"Data\cnbsystems.json", 1440))
                {
                    await DownloadCnbSystems(@"Data\cnbsystems.json", populatedSystemsEDDBbyName);
                }

                if (wasAnyUpdated || NeedToUpdateFile(@"Data\painitestations.json", 15))
                {
                    populatedSystemsEDDBbyId = populatedSystemsEDDBList
                        .ToDictionary(x => x.Id);

                    populatedSystemsEDDBbyEdsmId = populatedSystemsEDDBList
                        .Where(x => x.EdsmId != null)
                        .ToDictionary(x => (int) x.EdsmId);

                    stationsEDDBList = JsonReaderExtensions.ParseJson<StationEDDB>(@"Data\stationsEDDB.json").ToList();

                    stationsEDDBList.ForEach(z =>
                    {
                        populatedSystemsEDDBbyId.TryGetValue(z.SystemId, out var system);
                        z.SystemName = system?.Name;
                    });

                    // there are multiple stations with the same name ???
                    stationsEDDB = stationsEDDBList
                        .GroupBy(x => x.SystemName + x.Name).Select(x => x.First())
                        .ToDictionary(x => x.SystemName + x.Name);

                    Console.WriteLine("looking up additional EDDB station information for all stations");

                    stationsEDSM = JsonReaderExtensions.ParseJson<StationEDSM>(@"Data\stationsEDSM.json").ToList();

                    Console.WriteLine("looking up EDDB system information for all stations");

                    stationsEDSM.ForEach(z =>
                    {
                        stationsEDDB.TryGetValue(z.SystemName + z.Name, out var station);
                        z.AdditionalStationDataEDDB = station;

                        populatedSystemsEDDBbyEdsmId.TryGetValue(z.SystemId, out var system);
                        if (system != null)
                        {
                            z.PopulatedSystemEDDB = system;
                        }
                        else
                        {
                            populatedSystemsEDDBbyName.TryGetValue(z.SystemName, out var system2);
                            if (system2 != null)
                            {
                                z.PopulatedSystemEDDB = system2;
                            }
                        }

                        z.PrimaryEconomy = z.AdditionalStationDataEDDB?.Economies?.FirstOrDefault() ?? z.PopulatedSystemEDDB?.PrimaryEconomy ?? z.Economy;

                        z.SecondaryEconomy = z.AdditionalStationDataEDDB?.Economies?.LastOrDefault() ?? z.PopulatedSystemEDDB?.PrimaryEconomy ?? z.Economy;

                        if (z.AdditionalStationDataEDDB?.Economies?.Count == 2 && !string.IsNullOrEmpty(z.PopulatedSystemEDDB?.PrimaryEconomy) && z.PrimaryEconomy != z.PopulatedSystemEDDB.PrimaryEconomy)
                        {
                            z.SecondaryEconomy = z.PrimaryEconomy;
                            z.PrimaryEconomy = z.PopulatedSystemEDDB.PrimaryEconomy;

                            z.AdditionalStationDataEDDB.Economies[0] = z.PrimaryEconomy;
                            z.AdditionalStationDataEDDB.Economies[1] = z.SecondaryEconomy;
                        }
                    });

                    await DownloadInaraMiningStationsHtml(@"Data\painitestations.json", "https://inara.cz/ajaxaction.php?act=goodsdata&refid2=1261&refname=sellmax&refid=84", "Painite", stationsEDSM);
                    await DownloadInaraMiningStationsHtml(@"Data\ltdstations.json", "https://inara.cz/ajaxaction.php?act=goodsdata&refid2=1261&refname=sellmax&refid=144", "LTD", stationsEDSM);
                    await DownloadInaraMiningStationsHtml(@"Data\platinumstations.json", "https://inara.cz/ajaxaction.php?act=goodsdata&refid2=1261&refname=sellmax&refid=81", "Platinum", stationsEDSM);
                    await DownloadInaraMiningStationsHtml(@"Data\tritiumstations.json", "https://inara.cz/ajaxaction.php?act=goodsdata&refid2=1261&refname=sellmax&refid=10269", "Tritium", stationsEDSM);
                    await DownloadInaraMiningStationsHtml(@"Data\tritiumbuystations.json", "https://inara.cz/ajaxaction.php?act=goodsdata&refid2=1261&refname=buymin&refid=10269", "Tritium", stationsEDSM);

                    //await DownloadEddbMiningStationsHtml(@"Data\painitestations.json", "https://eddb.io/commodity/", "Painite", 83, stationsEDSM, true);
                    //await DownloadEddbMiningStationsHtml(@"Data\ltdstations.json", "https://eddb.io/commodity/", "LTD", 276, stationsEDSM, true);
                    //await DownloadEddbMiningStationsHtml(@"Data\tritiumstations.json", "https://eddb.io/commodity/", "Tritium", 362, stationsEDSM, true);
                    //await DownloadEddbMiningStationsHtml(@"Data\tritiumbuystations.json", "https://eddb.io/commodity/", "Tritium", 362, stationsEDSM, false);
                }

                if (wasAnyUpdated)
                {
                    //-------------------------

                    Console.WriteLine("finding Engineers stations");
                    var engineers = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null && // now missing Cloe Sedesi in an uninhabited system !!!!!!!!!
                            x.Government == "Workshop (Engineer)" &&
                            x.SystemName != "Maia").ToList(); // gets rid of second professor palin
                    
                    StationSerialize(engineers, @"Data\engineers.json");

                    Console.WriteLine("finding Aisling Duval stations");
                    var aislingDuval = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Aisling Duval" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(aislingDuval, @"Data\aislingduval.json");

                    Console.WriteLine("finding Archon Delaine stations");
                    var archonDelaine = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Archon Delaine" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(archonDelaine, @"Data\archondelaine.json");

                    Console.WriteLine("finding Arissa Lavigny-Duval stations");
                    var arissaLavignyDuval = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Arissa Lavigny-Duval" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(arissaLavignyDuval, @"Data\arissalavignyduval.json");

                    Console.WriteLine("finding Denton Patreus stations");
                    var dentonPatreus = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Denton Patreus" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(dentonPatreus, @"Data\dentonpatreus.json");

                    Console.WriteLine("finding Edmund Mahon stations");
                    var edmundMahon = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Edmund Mahon" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(edmundMahon, @"Data\edmundmahon.json");

                    Console.WriteLine("finding Felicia Winters stations");
                    var feliciaWinters = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Felicia Winters" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(feliciaWinters, @"Data\feliciawinters.json");

                    Console.WriteLine("finding Li Yong-Rui stations");
                    var liYongRui = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Li Yong-Rui" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(liYongRui, @"Data\liyongrui.json");

                    Console.WriteLine("finding Pranav Antal stations");
                    var pranavAntal = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Pranav Antal" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(pranavAntal, @"Data\pranavantal.json");

                    Console.WriteLine("finding Yuri Grom stations");
                    var yuriGrom = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Yuri Grom" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(yuriGrom, @"Data\yurigrom.json");

                    Console.WriteLine("finding Zachary Hudson stations");
                    var zacharyHudson = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Zachary Hudson" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(zacharyHudson, @"Data\zacharyhudson.json");

                    Console.WriteLine("finding Zemina Torval stations");
                    var zeminaTorval = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PopulatedSystemEDDB != null &&
                            x.PopulatedSystemEDDB.Power == "Zemina Torval" &&
                            x.PopulatedSystemEDDB.PowerState == "Control" &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();
                    StationSerialize(zeminaTorval, @"Data\zeminatorval.json");

                    //----------------

                    Console.WriteLine("finding interstellar factors");

                    var interStellarFactors = stationsEDSM
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    x.OtherServices.Any(y => y == "Interstellar Factors Contact") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(interStellarFactors, @"Data\interstellarfactors.json");

                    // see https://github.com/EDCD/FDevIDs/blob/a2655d9836fa32c4d9a8041edd8b2a4a7ed9d15b/How%20to%20determine%20MatTrader%20and%20Broker%20type

                    Console.WriteLine("finding encoded data traders");

                    //Encoded data trader
                    //Found in systems with medium-high security, a 'high tech' or 'military' economy
                    var encodedDataTraders = stationsEDSM
                        .Where(x =>
                            x.Type != "Fleet Carrier" &&
                            x.Government != "Fleet Carrier" &&
                            x.Economy != "Fleet Carrier" &&
                            !string.IsNullOrEmpty(x.Type) &&
                            !string.IsNullOrEmpty(x.Government) &&
                            !string.IsNullOrEmpty(x.Economy) &&
                            x.PrimaryEconomy != "Extraction" &&
                            x.PrimaryEconomy != "Refinery" &&
                            x.PrimaryEconomy != "Industrial" &&

                            (x.PrimaryEconomy == "High Tech" || x.PrimaryEconomy == "Military" || x.SecondaryEconomy == "High Tech" || x.SecondaryEconomy == "Military") &&

                            x.OtherServices.Any(y => y == "Material Trader") &&
                            x.PopulatedSystemEDDB != null &&
                            x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                            x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(encodedDataTraders, @"Data\encodeddatatraders.json");

                    Console.WriteLine("finding raw material traders");

                    //Raw material trader
                    //Found in systems with medium-high security, an 'extraction' or 'refinery' economy
                    var rawMaterialTraders = stationsEDSM
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    x.PrimaryEconomy != "High Tech" &&
                                    x.PrimaryEconomy != "Military" &&
                                    x.PrimaryEconomy != "Industrial" &&

                                    (x.PrimaryEconomy == "Extraction" || x.PrimaryEconomy == "Refinery" || x.SecondaryEconomy == "Extraction" || x.SecondaryEconomy == "Refinery") &&
                                    
                                    x.OtherServices.Any(y => y == "Material Trader") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(rawMaterialTraders, @"Data\rawmaterialtraders.json");

                    Console.WriteLine("finding manufactured material traders");

                    //Manufactured material trader
                    //Found in systems with medium-high security, an 'industrial' economy
                    var manufacturedMaterialTraders = stationsEDSM
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    x.PrimaryEconomy != "High Tech" &&
                                    x.PrimaryEconomy != "Military" &&
                                    x.PrimaryEconomy != "Extraction" &&
                                    x.PrimaryEconomy != "Refinery" &&

                                    (x.PrimaryEconomy == "Industrial" || x.SecondaryEconomy == "Industrial") &&

                                    x.OtherServices.Any(y => y == "Material Trader") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(manufacturedMaterialTraders, @"Data\manufacturedmaterialtraders.json");

                    Console.WriteLine("finding human technology brokers");

                    //Human Technology Broker
                    //Found in systems with an 'Industrial' economy
                    var humanTechnologyBrokers = stationsEDSM
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    !(x.PrimaryEconomy == "High Tech" || (x.PrimaryEconomy != "Industrial" && x.SecondaryEconomy == "High Tech")) &&

                                    x.OtherServices.Any(y => y == "Technology Broker") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(humanTechnologyBrokers, @"Data\humantechnologybrokers.json");

                    Console.WriteLine("finding guardian technology brokers");

                    //Guardian Technology Broker
                    //Found in systems with a 'high tech' economy
                    var guardianTechnologyBrokers = stationsEDSM
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    (x.PrimaryEconomy == "High Tech" || (x.PrimaryEconomy != "Industrial" && x.SecondaryEconomy == "High Tech")) &&

                                    x.OtherServices.Any(y => y == "Technology Broker") &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(guardianTechnologyBrokers, @"Data\guardiantechnologybrokers.json");

                    Console.WriteLine("finding full station list");

                    //Full Station List
                    var fullStationList = stationsEDSM
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    x.PopulatedSystemEDDB != null &&
                                    x.AdditionalStationDataEDDB?.IsPlanetary == false &&
                                    x.AdditionalStationDataEDDB.MaxLandingPadSize == "L").ToList();

                    StationSerialize(fullStationList, @"Data\fullstationlist.json");

                    //Colonia Bridge
                    var coloniaBridge = stationsEDSM
                        .Where(x =>
                            x.Name.StartsWith("CB-") &&
                            x.Type == "Mega ship" &&
                            x.AdditionalStationDataEDDB?.ControllingMinorFactionId == 77645
                        ).ToList();

                    StationSerialize(coloniaBridge, @"Data\coloniabridge.json");

                    //Odyssey Settlements
                    var odysseySettlements = stationsEDSM
                        .Where(x =>
                            x.Type == "Odyssey Settlement"
                        ).ToList();

                    StationSerialize(odysseySettlements, @"Data\odysseysettlements.json");

                }

                await DownloadHotspotSystems(@"Data\painitesystems.json", "http://edtools.cc/miner?a=r&n=", "Painite");
                await DownloadHotspotSystems(@"Data\ltdsystems.json", "http://edtools.cc/miner?a=r&n=", "LTD");
                await DownloadHotspotSystems(@"Data\platinumsystems.json", "http://edtools.cc/miner?a=r&n=", "Platinum");

                await DownloadPoiGEC(@"Data\poigec.json", "https://edastro.com/poi/json/all");

                // https://gist.github.com/corenting/b6ac5cf8f446f54856e08b6e287fe835


                // stopped woring 29/05/2021
                //"https://elitedangerous-website-backend-production.elitedangerous.com/api/galnet?_format=json"

                await DownloadGalnet(@"Data\galnet.json", "https://cms.zaonce.net/en-GB/jsonapi/node/galnet_article?&sort=-published_at&page[offset]=0&page[limit]=100");

                // stopped working 1 dec 2020
                //DownloadCommunityGoals(@"Data\communitygoals.json", "https://elitedangerous-website-backend-production.elitedangerous.com/api/initiatives/list?_format=json&lang=en"); 
                await DownloadCommunityGoals(@"Data\communitygoals.json", "https://api.orerve.net/2.0/website/initiatives/list?lang=en");

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            Log.Info("ImportData ended");
        }
    }
}
