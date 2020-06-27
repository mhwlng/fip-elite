using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EliteJournalReader.Events;
using Newtonsoft.Json;

namespace Elite
{
    public static class History
    {
        // 1478 x 1125

        public const double SpaceMinXL = -41715.0;
        public const double SpaceMaxXL = 41205.0;
        public const double SpaceMinZL = -19737.0;
        public const double SpaceMaxZL = 68073.0;

        // 1125 x 1478

        public const double SpaceMinXP = -31812.8;
        public const double SpaceMaxXP = 31302.85;
        public const double SpaceMinZP = -33513.4;
        public const double SpaceMaxZP = 81849.41;

        public class FSDJumpInfo
        {
            public string StarSystem { get; set; }
            public List<double> StarPos { get; set; }
        }

        public class CarrierJumpInfo
        {
            public bool Docked { get; set; }
            public string StarSystem { get; set; }
            public List<double> StarPos { get; set; }
        }

        public static List<PointF> TravelHistoryPointsL = new List<PointF>();
        public static List<PointF> TravelHistoryPointsP = new List<PointF>();

        public class LocationInfo

        {
            //"Docked":true, "StationName":"Jameson Memorial", "StarSystem":"Shinrarta Dezhra",  "StarPos":[55.71875,17.59375,27.15625], 

            public bool Docked { get; set; }
            public string StationName { get; set; }
            public string StarSystem { get; set; }
            public List<double> StarPos { get; set; }

        }

        public class DockedInfo

        {
            // "StationName":"Jameson Memorial", "StarSystem":"Shinrarta Dezhra"

            public string StationName { get; set; }
            public string StarSystem { get; set; }
        }

        public static Dictionary<string, List<double>> VisitedSystemList = new Dictionary<string, List<double>>();

        public static double GalaxyImageLWidth { get; set; }
        public static double GalaxyImageLHeight { get; set; }
        public static double GalaxyImagePWidth { get; set; }
        public static double GalaxyImagePHeight { get; set; }

        private class UnsafeNativeMethods
        {
            [DllImport("Shell32.dll")]
            public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
                IntPtr hToken, out IntPtr ppszPath);
        }


        /// <summary>
        /// The standard Directory of the Player Journal files (C:\Users\%username%\Saved Games\Frontier Developments\Elite Dangerous).
        /// </summary>
        private static DirectoryInfo StandardDirectory
        {
            get
            {
                var result = UnsafeNativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
                    0, new IntPtr(0), out var path);
                if (result >= 0)
                {
                    try
                    {
                        return new DirectoryInfo(Marshal.PtrToStringUni(path) +
                                                 @"\Frontier Developments\Elite Dangerous");
                    }
                    catch
                    {
                        return new DirectoryInfo(Directory.GetCurrentDirectory());
                    }
                }
                else
                {
                    return new DirectoryInfo(Directory.GetCurrentDirectory());
                }
            }
        }

        public static void AddTravelPos(List<double> starPos)
        {
            if (starPos?.Count == 3)
            {
                var x = starPos[0];
                var y = starPos[1];
                var z = starPos[2];

                var imgX = (x - SpaceMinXL) / (SpaceMaxXL - SpaceMinXL) * GalaxyImageLWidth;
                var imgY = (SpaceMaxZL - z) / (SpaceMaxZL - SpaceMinZL) * GalaxyImageLHeight;

                TravelHistoryPointsL.Add(new PointF
                {
                    X = (float) imgX,
                    Y = (float) imgY
                });

                imgX = (x - SpaceMinXP) / (SpaceMaxXP - SpaceMinXP) * GalaxyImagePWidth;
                imgY = (SpaceMaxZP - z) / (SpaceMaxZP - SpaceMinZP) * GalaxyImagePHeight;

                TravelHistoryPointsP.Add(new PointF
                {
                    X = (float)imgX,
                    Y = (float)imgY
                });
            }
        }


        public static string GetEliteHistory()
        {
            var journalDirectory = StandardDirectory;

            if (!Directory.Exists(journalDirectory.FullName))
            {
                App.Log.Error($"Directory {journalDirectory.FullName} not found.");
                return null;
            }

            try
            {
                var imageL = Image.FromFile(Path.Combine(App.ExePath, "Templates\\images\\galaxyL.png"));
                GalaxyImageLWidth = imageL.Width;
                GalaxyImageLHeight = imageL.Height;

                var imageP = Image.FromFile(Path.Combine(App.ExePath, "Templates\\images\\galaxyP.png"));
                GalaxyImagePWidth = imageP.Width;
                GalaxyImagePHeight = imageP.Height;

                var journalFiles = journalDirectory.GetFiles("Journal.*").OrderBy(x => x.LastWriteTime);

                string lastJumpedSystem = string.Empty;

                foreach (var journalFile in journalFiles)
                {
                    using (var fileStream =
                        journalFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var streamReader = new StreamReader(fileStream))
                        {
                            while (!streamReader.EndOfStream)
                            {
                                try
                                {
                                    var json = streamReader.ReadLine();

                                    if (json?.Contains("\"event\":\"CarrierJump\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<CarrierJumpInfo>(json);

                                        if (info.Docked)
                                        {
                                            if (info.StarPos?.Count == 3)
                                            {
                                                AddTravelPos(info.StarPos);
                                                lastJumpedSystem = info.StarSystem;
                                            }

                                            Ships.HandleShipFsdJump(info.StarSystem, info.StarPos.ToList());
                                        }
                                    }
                                    else
                                    if (json?.Contains("\"event\":\"FSDJump\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<FSDJumpInfo>(json);

                                        if (info.StarPos?.Count == 3)
                                        {
                                            AddTravelPos(info.StarPos);
                                            lastJumpedSystem = info.StarSystem;
                                        }

                                        Ships.HandleShipFsdJump(info.StarSystem, info.StarPos.ToList());
                                    }
                                    else if (json?.Contains("\"event\":\"LoadGame\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<LoadGameEvent.LoadGameEventArgs>(json);

                                        Ships.HandleLoadGame(info.ShipID, info.Ship, info.ShipName);
                                    }
                                    else if (json?.Contains("\"event\":\"SetUserShipName\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<SetUserShipNameEvent.SetUserShipNameEventArgs>(json);

                                        Ships.HandleSetUserShipName(info.ShipID, info.UserShipName, info.Ship);
                                    }
                                    else if (json?.Contains("\"event\":\"HullDamage\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<HullDamageEvent.HullDamageEventArgs>(json);

                                        Ships.HandleHullDamage(info.Health);
                                    }
                                    else if (json?.Contains("\"event\":\"Location\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<LocationInfo>(json);

                                        Ships.HandleShipLocation(info.Docked, info.StarSystem, info.StationName,
                                            info.StarPos);
                                    }
                                    else if (json?.Contains("\"event\":\"Docked\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<DockedInfo>(json);

                                        Ships.HandleShipDocked(info.StarSystem, info.StationName);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardNew\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<ShipyardNewEvent.ShipyardNewEventArgs>(
                                                json);

                                        Ships.HandleShipyardNew(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardBuy\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<ShipyardBuyEvent.ShipyardBuyEventArgs>(
                                                json);

                                        Ships.HandleShipyardBuy(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardSell\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ShipyardSellEvent.ShipyardSellEventArgs>(json);

                                        Ships.HandleShipyardSell(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardSwap\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ShipyardSwapEvent.ShipyardSwapEventArgs>(json);

                                        Ships.HandleShipyardSwap(info);
                                    }
                                    else if (json?.Contains("\"event\":\"StoredShips\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<StoredShipsEvent.StoredShipsEventArgs>(
                                                json);

                                        Ships.HandleStoredShips(info);
                                    }
                                    /*else if (json?.Contains("\"event\":\"ShipyardTransfer\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<ShipyardTransferEvent.ShipyardTransferEventArgs>(json);
                                    }*/
                                    else if (json?.Contains("\"event\":\"Loadout\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<LoadoutEvent.LoadoutEventArgs>(json);

                                        Ships.HandleLoadout(info);
                                        Module.HandleLoadout(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleBuy\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<ModuleBuyEvent.ModuleBuyEventArgs>(json);

                                        Module.HandleModuleBuy(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleSell\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleSellEvent.ModuleSellEventArgs>(json);

                                        Module.HandleModuleSell(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleSellRemote\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleSellRemoteEvent.ModuleSellRemoteEventArgs>(
                                                json);

                                        Module.HandleModuleSellRemote(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleStore\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleStoreEvent.ModuleStoreEventArgs>(json);

                                        Module.HandleModuleStore(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleSwap\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleSwapEvent.ModuleSwapEventArgs>(json);

                                        Module.HandleModuleSwap(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleRetrieve\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleRetrieveEvent.ModuleRetrieveEventArgs>(json);

                                        Module.HandleModuleRetrieve(info);
                                    }
                                    else if (json?.Contains("\"event\":\"MassModuleStore\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<MassModuleStoreEvent.MassModuleStoreEventArgs>(json);

                                        Module.HandleMassModuleStore(info);
                                    }

                                    /*else if (json?.Contains("\"event\":\"MissionCompleted\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<MissionCompletedEvent.MissionCompletedEventArgs>(json);

                                    }*/
                                    else if (json?.Contains("\"event\":\"MaterialCollected\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<MaterialCollectedEvent.MaterialCollectedEventArgs>(json);

                                        if (!string.IsNullOrEmpty(lastJumpedSystem))
                                        {
                                            var name = (info.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.Name.ToLower())).Trim();

                                            // { "timestamp":"2018-08-11T15:29:20Z", "event":"MaterialCollected", "Category":"Encoded", "Name":"shielddensityreports", "Name_Localised":"Untypical Shield Scans ", "Count":3 }

                                            Material.AddHistory(name, lastJumpedSystem, info.Count);
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    App.Log.Error(ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return journalDirectory.FullName;
        }
    }
}

