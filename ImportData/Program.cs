using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web.UI;
using HtmlAgilityPack;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable StringLiteralTypo

namespace ImportData
{
    public static class JsonReaderExtensions
    {
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
                var serializer = new JsonSerializer();
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

        public static void DownloadJson<T>(string url, string path, ref bool wasUpdated)
        {
            path = Path.Combine(GetExePath(), path);

            DeleteExpiredFile(path, 1440);

            if (!File.Exists(path))
            {
                var serializer = new JsonSerializer();

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

                    using (var sw = new StreamWriter(File.Open(path, FileMode.Create)))
                    {
                        using (var jsonWriter = new JsonTextWriter(sw))
                        {
                            using (var compressedStream = new MemoryStream(client.DownloadData(url)))
                            {
                                using (var s = new GZipStream(compressedStream, CompressionMode.Decompress))
                                {
                                    using (var sr = new StreamReader(s))
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
                Z = x.PopulatedSystemEDDB?.Z ?? 0,

                Body = x.Body

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
                NullValueHandling = NullValueHandling.Ignore
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

        private static List<CNBSystemData> GetCnbSystems(string url)
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
                Log.Error(ex);
            }

            return new List<CNBSystemData>();

        }

        private static void CnbSystemsSerialize(List<CNBSystemData> systems, string fullPath)
        {
            new FileInfo(fullPath).Directory?.Create();

            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            using (var sw = new StreamWriter(fullPath))
            using (var writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, systems);
            }
        }

        private static void DownloadCnbSystems(string path, Dictionary<string, PopulatedSystemEDDB> populatedSystemsEDDBbyName)
        {
            path = Path.Combine(GetExePath(), path);

            DeleteExpiredFile(path, 1440);

            if (!File.Exists(path))
            {
                Console.WriteLine("looking up Compromised Nav Beacons");

                var cnbSystems = GetCnbSystems("http://edtools.cc/res.json");

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

        private static void DownloadHotspotSystems(string path, string url, string material)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                DeleteExpiredFile(path, 1440);

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
                Log.Error(ex);
            }
        }


        private static void GalnetSerialize(List<GalnetData> galnet, string fullPath)
        {
            new FileInfo(fullPath).Directory?.Create();

            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
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
                NullValueHandling = NullValueHandling.Ignore
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

        private static void DownloadGalnet(string path, string url)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                DeleteExpiredFile(path, 720);

                if (!File.Exists(path))
                {
                    Console.WriteLine("looking up galnet");

                    using (var client = new WebClient())
                    {
                        var data = client.DownloadString(url);

                        var galnetJson = JsonConvert.DeserializeObject<List<GalnetData>>(data)?.Take(100).ToList();

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
                                            if (RemoteFileExists("https://hosting.zaonce.net/elite-dangerous/galnet/" + i + ".png"))
                                            {
                                                x.ImageList.Add(i);
                                            }
                                        }
                                    }
                                }

                                x.Image = null;

                            });

                            GalnetSerialize(galnetJson, path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static void DownloadCommunityGoals(string path, string url)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                DeleteExpiredFile(path, 180);

                if (!File.Exists(path))
                {
                    Console.WriteLine("looking up community goals");

                    using (var client = new WebClient())
                    {
                        var data = client.DownloadString(url);

                        var cgJson = JsonConvert.DeserializeObject<CommunityGoalsData>(data);

                        if (cgJson != null)
                        {
                            CommunityGoalSerialize(cgJson.ActiveInitiatives, path);
                        }
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
                Demand =x.Demand,

                X = x.StationEDSM?.PopulatedSystemEDDB?.X ?? 0,
                Y = x.StationEDSM?.PopulatedSystemEDDB?.Y ?? 0,
                Z = x.StationEDSM?.PopulatedSystemEDDB?.Z ?? 0,

                DistanceToArrival = x.StationEDSM?.DistanceToArrival,
                Type = x.StationEDSM?.Type,

                SystemSecurity = x.StationEDSM?.PopulatedSystemEDDB?.Security,
                SystemPopulation = x.StationEDSM?.PopulatedSystemEDDB?.Population,

                PowerplayState = x.StationEDSM?.PopulatedSystemEDDB?.PowerState,
                Powers = x.StationEDSM?.PopulatedSystemEDDB?.Power,

                Allegiance = x.StationEDSM?.Allegiance,
                Government = x.StationEDSM?.Government,
                Economy = x.StationEDSM?.Economy,
                Faction = x.StationEDSM?.ControllingFaction?.Name,

                Body  = x.StationEDSM?.Body

            }).ToList();
        }

        private static void MiningStationsSerialize(List<MiningStationData> stations, string fullPath)
        {
            new FileInfo(fullPath).Directory?.Create();

            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
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

        private static void DownloadInaraMiningStationsHtml(string path, string url, string material, List<StationEDSM> stationsEDSM)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                Console.WriteLine("looking up " + material + " Stations");

                using (var client = new WebClient())
                {
                    var data = client.DownloadString(url);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(data);

                    var currentTime = ConvertToUnixTimestamp(DateTime.UtcNow);


                    var stationInfo = doc.DocumentNode.SelectSingleNode("//table[@class='tablesorter']")
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => !tr.HasClass("hideable1") /*&& !tr.HasClass("hideable2")*/ && !tr.HasClass("hideable3"))
                        .Select(tr => tr.Elements("td").ToList())
                        .Select(td => new HotspotStationData
                        {
                            Station = td[0].Descendants("span").FirstOrDefault()?.InnerText ?? "?",
                            System = td[0].Descendants("span").Skip(2).FirstOrDefault()?.InnerText ?? "?",
                            Price = Convert.ToInt32(td[5].GetAttributeValue("data-order", "0")),
                            Demand = Convert.ToInt32(td[4].GetAttributeValue("data-order","0")),
                            Pad = td[1].InnerText, //tr[5],
                            AgoSec = (int)(currentTime - Convert.ToInt64(td[7].GetAttributeValue("data-order", "0")))
                        })
                        .ToList();

                    var stationsData = GetMiningStationsData(stationInfo, stationsEDSM);

                    MiningStationsSerialize(stationsData, path);
                        
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static void DownloadEddbMiningStationsHtml(string path, string url, string material, int cid, List<StationEDSM> stationsEDSM, bool sell)
        {
            try
            {
                path = Path.Combine(GetExePath(), path);

                Console.WriteLine("looking up " + material + " Stations");

                using (var client = new WebClient())
                {
                    var data = client.DownloadString(url + cid);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(data);

                    var id = "table-stations-max-sell";
                    if (!sell)
                    {
                        id = "table-stations-min-buy";
                    }

                    var stationInfo = doc.DocumentNode.SelectSingleNode("//table[@id='"+id+"']")
                        .Descendants("tr")
                        .Where(tr => tr.Elements("td").Count() == 7)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .Select( tr => new HotspotStationData
                        {
                            Station = tr[0],
                            System = tr[1],
                            Price = Convert.ToInt32(tr[2].Replace(",","").Replace(".", "")),
                            Demand = Convert.ToInt32(tr[4].Replace(",","").Replace(".", "")),
                            Pad = tr[5],
                            AgoSec = Convert.ToInt32(tr[6].Substring(2, tr[6].IndexOf("}", StringComparison.Ordinal)-2))
                        })
                        .ToList();

                    var stationsData = GetMiningStationsData(stationInfo, stationsEDSM);

                    MiningStationsSerialize(stationsData, path);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        static void Main(string[] args)
        {
            Log.Info("ImportData started");

            try
            {
                List<StationEDSM> stationsEDSM = null;
                List<StationEDDB> stationsEDDBList = null;
                Dictionary<string,StationEDDB> stationsEDDB;
                List<PopulatedSystemEDDB> populatedSystemsEDDBList;
                Dictionary<int, PopulatedSystemEDDB> populatedSystemsEDDBbyEdsmId;

                var wasAnyUpdated = false;

                Console.WriteLine("downloading station list from EDSM");

                //DownloadJson(@"Data\stationsEDSM.json", "https://www.edsm.net/dump/stations.json.gz", ref wasAnyUpdated);
                JsonReaderExtensions.DownloadJson<StationEDSM>("https://www.edsm.net/dump/stations.json.gz", @"Data\stationsEDSM.json", ref wasAnyUpdated);

                Console.WriteLine("downloading populated systems from EDDB");

                //DownloadJson(@"Data\populatedsystemsEDDB.json", "https://eddb.io/archive/v6/systems_populated.json", ref wasAnyUpdated);
                JsonReaderExtensions.DownloadJson<PopulatedSystemEDDB>("https://eddb.io/archive/v6/systems_populated.json", @"Data\populatedsystemsEDDB.json", ref wasAnyUpdated);

                Console.WriteLine("downloading station list from EDDB");

                //DownloadJson(@"Data\stationsEDDB.json", "https://eddb.io/archive/v6/stations.json", ref wasAnyUpdated);
                JsonReaderExtensions.DownloadJson<StationEDDB>("https://eddb.io/archive/v6/stations.json", @"Data\stationsEDDB.json", ref wasAnyUpdated);

                Console.WriteLine("checking station and system data");

                populatedSystemsEDDBList = JsonReaderExtensions.ParseJson<PopulatedSystemEDDB>(@"Data\populatedsystemsEDDB.json").ToList();

                if (NeedToUpdateFile(@"Data\cnbsystems.json", 1440))
                {
                    // there are multiple stations with the same name ???
                    var populatedSystemsEDDBbyName = populatedSystemsEDDBList
                        .GroupBy(x => x.Name).Select(x => x.First())
                        .ToDictionary(x => x.Name);

                    DownloadCnbSystems(@"Data\cnbsystems.json", populatedSystemsEDDBbyName);
                }

                if (wasAnyUpdated || NeedToUpdateFile(@"Data\painitestations.json", 15))
                {
                    populatedSystemsEDDBbyEdsmId = populatedSystemsEDDBList
                        .Where(x => x.EdsmId != null)
                        .ToDictionary(x => (int) x.EdsmId);


                    stationsEDDBList = JsonReaderExtensions.ParseJson<StationEDDB>(@"Data\stationsEDDB.json").ToList();

                    // there are multiple stations with the same name ???
                    stationsEDDB = stationsEDDBList
                        .GroupBy(x => x.Name).Select(x => x.First())
                        .ToDictionary(x => x.Name);

                    Console.WriteLine("looking up additional EDDB station information for all stations");

                    stationsEDSM = JsonReaderExtensions.ParseJson<StationEDSM>(@"Data\stationsEDSM.json").ToList();

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
                        if (populatedSystemsEDDBbyEdsmId.ContainsKey(z.SystemId))
                        {
                            z.PopulatedSystemEDDB = populatedSystemsEDDBbyEdsmId[z.SystemId];
                        }
                    });

                    DownloadInaraMiningStationsHtml(@"Data\painitestations.json", "https://inara.cz/ajaxaction.php?act=goodsdata&refid2=1261&refname=sellmax&refid=84", "Painite", stationsEDSM);
                    DownloadInaraMiningStationsHtml(@"Data\ltdstations.json", "https://inara.cz/ajaxaction.php?act=goodsdata&refid2=1261&refname=sellmax&refid=144", "LTD", stationsEDSM);
                    DownloadInaraMiningStationsHtml(@"Data\tritiumstations.json", "https://inara.cz/ajaxaction.php?act=goodsdata&refid2=1261&refname=sellmax&refid=10269", "Tritium",stationsEDSM);
                    DownloadInaraMiningStationsHtml(@"Data\tritiumbuystations.json", "https://inara.cz/ajaxaction.php?act=goodsdata&refid2=1261&refname=buymin&refid=10269", "Tritium", stationsEDSM);

                    //DownloadEddbMiningStationsHtml(@"Data\painitestations.json", "https://eddb.io/commodity/", "Painite", 83, stationsEDSM, true);
                    //DownloadEddbMiningStationsHtml(@"Data\ltdstations.json", "https://eddb.io/commodity/", "LTD", 276, stationsEDSM, true);
                    //DownloadEddbMiningStationsHtml(@"Data\tritiumstations.json", "https://eddb.io/commodity/", "Tritium", 362, stationsEDSM, true);
                    //DownloadEddbMiningStationsHtml(@"Data\tritiumbuystations.json", "https://eddb.io/commodity/", "Tritium", 362, stationsEDSM, false);
                }

                if (wasAnyUpdated)
                {
                    //-------------------------

                    Console.WriteLine("finding Engineers stations");
                    var engineers = stationsEDSM
                        .Where(x =>
                            x.PopulatedSystemEDDB != null && // now missing Cloe Sedesi in an uninhabited system !!!!!!!!!
                            x.Government == "Workshop (Engineer)").ToList();
                    
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

                    Console.WriteLine("finding raw material traders");

                    //Raw material trader
                    //Found in systems with medium-high security, an 'extraction' or 'refinery' economy
                    var rawMaterialEconomies = new List<string> {"Extraction", "Refinery"};
                    var rawMaterialTraders = stationsEDSM
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    rawMaterialEconomies.Contains(x.Economy) &&
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
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    manufacturedMaterialEconomies.Contains(x.Economy) &&
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
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    encodedDataEconomies.Contains(x.Economy) &&
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
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    humanTechnologyEconomies.Contains(x.Economy) &&
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
                        .Where(x =>
                                    x.Type != "Fleet Carrier" &&
                                    x.Government != "Fleet Carrier" &&
                                    x.Economy != "Fleet Carrier" &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Type) &&
                                    !string.IsNullOrEmpty(x.Government) &&
                                    !string.IsNullOrEmpty(x.Economy) &&
                                    guardianTechnologyEconomies.Contains(x.Economy) &&
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

                }

                DownloadHotspotSystems(@"Data\painitesystems.json", "http://edtools.cc/miner?a=r&n=", "Painite");
                DownloadHotspotSystems(@"Data\ltdsystems.json", "http://edtools.cc/miner?a=r&n=", "LTD");

                DownloadGalnet(@"Data\galnet.json", "https://elitedangerous-website-backend-production.elitedangerous.com/api/galnet?_format=json");

                // stopped working 1 dec 2020
                //DownloadCommunityGoals(@"Data\communitygoals.json", "https://elitedangerous-website-backend-production.elitedangerous.com/api/initiatives/list?_format=json&lang=en"); 
                DownloadCommunityGoals(@"Data\communitygoals.json", "https://api.orerve.net/2.0/website/initiatives/list?lang=en"); 

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            Log.Info("ImportData ended");
        }
    }
}
