using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using EliteJournalReader.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Module = EliteJournalReader.Module;

namespace Elite
{
    public static class EliteHistory
    {
        public const double SpaceMinX = -41715.0;
        public const double SpaceMaxX = 41205.0;
        public const double SpaceMinZ = -19737.0;
        public const double SpaceMaxZ = 68073.0;

        public class FSDJumpInfo
        {
            public string StarSystem { get; set; }
            public List<double> StarPos { get; set; }
        }

        public static List<PointF> TravelHistoryPoints = new List<PointF>();

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


        public class ShipData
        {
            public string StationName { get; set; }
            public string StarSystem { get; set; }
            public string ShipType { get; set; }
            public string ShipTypeFull { get; set; }
            public string ShipImage { get; set; }
            public int ShipID { get; set; }

            public bool Stored { get; set; }

            public List<double> StarPos { get; set; } = new List<double>();

            public double Distance { get; set; }

            public int HullValue { get; set; }
            public int ModulesValue { get; set; }
            public double HullHealth { get; set; }
            public double UnladenMass { get; set; }
            public double FuelCapacity { get; set; }
            public int CargoCapacity { get; set; }
            public double MaxJumpRange { get; set; }
            public int Rebuy { get; set; }
            public bool Hot { get; set; }
            public Module[] Modules { get; set; }


        }

        public static List<ShipData> ShipsList = new List<ShipData>();


        public static Dictionary<string, List<double>> VisitedSystemList = new Dictionary<string, List<double>>();

        public static double ImageWidth  { get; set; }
        public static double ImageHeight { get; set; }

        private class UnsafeNativeMethods
        {
            [DllImport("Shell32.dll")]
            public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
        }


        /// <summary>
        /// The standard Directory of the Player Journal files (C:\Users\%username%\Saved Games\Frontier Developments\Elite Dangerous).
        /// </summary>
        public static DirectoryInfo StandardDirectory
        {
            get
            {
                int result = UnsafeNativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out IntPtr path);
                if (result >= 0)
                {
                    try { return new DirectoryInfo(Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous"); }
                    catch { return new DirectoryInfo(Directory.GetCurrentDirectory()); }
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

                var imgX = (x - SpaceMinX) / (SpaceMaxX - SpaceMinX) * ImageWidth;
                var imgY = (SpaceMaxZ - z) / (SpaceMaxZ - SpaceMinZ) * ImageHeight;

                TravelHistoryPoints.Add(new PointF
                {
                    X = (float)imgX,
                    Y = (float)imgY
                });
            }
        }

        public static void AddShip(int shipId,string shipType, string starSystem, string stationName, List<double> starPos, bool stored)
        {
            if (shipType == "testbuggy") return;

            if (starPos == null) return;

            if (!ShipsList.Any(x => x.ShipType == shipType && x.ShipID == shipId))
            {
                EliteData.ShipsByEliteID.TryGetValue(shipType, out var shipTypeFull);

                ShipsList.Add(new ShipData()
                {
                    ShipID = shipId,
                    ShipType = shipType,
                    ShipTypeFull = shipTypeFull,
                    ShipImage = shipTypeFull?.Trim() + ".png",

                    StarSystem =  starSystem,
                    StationName = stationName,
                    StarPos = starPos,
                    Stored = stored
                });
            }
        }

        public static void RemoveShip(int shipId, string shipType)
        {
            if (shipType == "testbuggy") return;

            var ship = ShipsList.FirstOrDefault(x =>
                x.ShipType == shipType && x.ShipID == shipId);

            if (ship != null)
            {
                ShipsList.Remove(ship);
            }
        }
        
        public static void SetCurrentShip(int shipId, string shipType, string starSystem, string stationName, List<double> starPos)
        {
            if (shipType == "testbuggy") return;

            if (!ShipsList.Any(x => x.ShipType == shipType && x.ShipID == shipId))
            {
                AddShip(shipId, shipType, starSystem, stationName, starPos, false);
            }

            foreach (var s in ShipsList)
            {
                s.Stored = (s.ShipType != shipType && s.ShipID != shipId);
            }
        }

        public static void HandleShipDistance(List<double> starPos)
        {
            if (ShipsList?.Any() == true && starPos?.Count == 3)
            {
                ShipsList.ForEach(item =>
                {
                    var Xs = starPos[0];
                    var Ys = starPos[1];
                    var Zs = starPos[2];

                    var Xd = item.StarPos[0];
                    var Yd = item.StarPos[1];
                    var Zd = item.StarPos[2];

                    double deltaX = Xs - Xd;
                    double deltaY = Ys - Yd;
                    double deltaZ = Zs - Zd;

                    item.Distance = (double)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                });

            }

        }

        public static void HandleShipFsdJump(string starSystem, List<double> starPos)
        {
            if (!VisitedSystemList.ContainsKey(starSystem))
            {
                VisitedSystemList.Add(starSystem, starPos);
            }
        }

        public static void HandleShipLocation(bool docked, string starSystem, string stationName, List<double> starPos)
        {
            var ship = ShipsList.FirstOrDefault(x =>
                x.Stored == false);

            if (docked && ship != null)
            {
                ship.StarSystem = starSystem;
                ship.StationName = stationName;
                ship.StarPos = starPos;
            }
        }

        public static void HandleShipDocked(string starSystem, string stationName)
        {
            VisitedSystemList.TryGetValue(starSystem, out var starPos);

            var ship = ShipsList.FirstOrDefault(x =>
                x.Stored == false);

            if (ship != null && starPos != null)
            {
                ship.StarSystem = starSystem;
                ship.StationName = stationName;
                ship.StarPos = starPos;
            }
        }

        public static void HandleShipyardNew(ShipyardNewEvent.ShipyardNewEventArgs info)
        {
            var ship = ShipsList.FirstOrDefault(x =>
                x.Stored == false);

            if (ship != null)
            {
                AddShip(info.NewShipID, info.ShipType.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos, true);
            }
        }

        public static void HandleShipyardSell(ShipyardSellEvent.ShipyardSellEventArgs info)
        {
            if (!string.IsNullOrEmpty(info.ShipType))
            {
                RemoveShip(info.SellShipId, info.ShipType.ToLower());
            }
        }

        public static void HandleShipyardBuy(ShipyardBuyEvent.ShipyardBuyEventArgs info)
        {
            if (info.SellShipID != null && !string.IsNullOrEmpty(info.SellOldShip))
            {
                RemoveShip((int)info.SellShipID, info.SellOldShip.ToLower());
            }
        }

        public static void HandleShipyardSwap(ShipyardSwapEvent.ShipyardSwapEventArgs info)
        {
            var ship = ShipsList.FirstOrDefault(x =>
                x.Stored == false);

            if (ship != null)
            {
                if (info.SellShipID != null && !string.IsNullOrEmpty(info.SellOldShip))
                {
                    RemoveShip((int) info.SellShipID, info.SellOldShip.ToLower());
                }

                if (info.StoreShipID != null && !ShipsList.Any(x =>
                        x.ShipType == info.StoreOldShip.ToLower() && x.ShipID == info.StoreShipID))
                {
                    AddShip((int) info.StoreShipID, info.StoreOldShip.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos, true);
                }

                if (!ShipsList.Any(x => x.ShipType == info.ShipType.ToLower() && x.ShipID == info.ShipID))
                {
                    AddShip(info.ShipID, info.ShipType.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos, false);
                }

                SetCurrentShip(info.ShipID, info.ShipType.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos);
            }

        }

        public static void HandleStoredShips(StoredShipsEvent.StoredShipsEventArgs info)
        {
            foreach (var s in ShipsList)
            {
                s.Stored = false;
            }

            foreach (var s in info.ShipsHere)
            {
                VisitedSystemList.TryGetValue(info.StarSystem, out var starPos);

                if (starPos != null)
                {
                    AddShip(s.ShipID, s.ShipType.ToLower(), info.StarSystem, info.StationName, starPos, true);
                }
            }
        }

        public static void HandleLoadout(LoadoutEvent.LoadoutEventArgs info)
        {
            var ship = ShipsList.FirstOrDefault(x =>
                x.Stored == false);

            if (ship != null)
            {
                ship.HullValue = info.HullValue;
                ship.ModulesValue = info.ModulesValue;
                ship.HullHealth = info.HullHealth * 100.0;
                ship.UnladenMass = info.UnladenMass;
                ship.FuelCapacity = info.FuelCapacity.Main;

                ship.CargoCapacity = info.CargoCapacity;
                ship.MaxJumpRange = info.MaxJumpRange;
                ship.Rebuy = info.Rebuy;
                ship.Hot = info.Hot;
                ship.Modules = info.Modules?.Select(m => m.Clone()).ToArray();
            }
        }

        public static string GetEliteHistory()
        {
            var journalDirectory = StandardDirectory;

            if (!Directory.Exists(journalDirectory.FullName))
            {
                App.log.Error($"Directory {journalDirectory.FullName} not found.");
                return null;
            }

            try
            {
                var image = Image.FromFile("Templates\\images\\galaxy.png");
                ImageWidth = image.Width;
                ImageHeight = image.Height;

                var journalFiles = journalDirectory.GetFiles("Journal.*").OrderBy(x => x.LastWriteTime);

                foreach (var journalFile in journalFiles)
                {
                    using (FileStream fileStream =
                        journalFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            while (!streamReader.EndOfStream)
                            {
                                try
                                {
                                    var json = streamReader.ReadLine();

                                    if (json?.Contains("\"event\":\"FSDJump\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<FSDJumpInfo>(json);

                                        if (info.StarPos?.Count == 3)
                                        {
                                            AddTravelPos(info.StarPos);
                                        }

                                        HandleShipFsdJump(info.StarSystem, info.StarPos.ToList());
                                    }
                                    else if (json?.Contains("\"event\":\"Location\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<LocationInfo>(json);

                                        HandleShipLocation(info.Docked, info.StarSystem, info.StationName, info.StarPos);
                                    }
                                    else if (json?.Contains("\"event\":\"Docked\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<DockedInfo>(json);

                                        HandleShipDocked(info.StarSystem, info.StationName);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardNew\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<ShipyardNewEvent.ShipyardNewEventArgs>(json);

                                        HandleShipyardNew(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardBuy\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<ShipyardBuyEvent.ShipyardBuyEventArgs>(json);

                                        HandleShipyardBuy(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardSell\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<ShipyardSellEvent.ShipyardSellEventArgs>(json);

                                        HandleShipyardSell(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardSwap\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<ShipyardSwapEvent.ShipyardSwapEventArgs>(json);

                                        HandleShipyardSwap(info);
                                    }
                                    else if (json?.Contains("\"event\":\"StoredShips\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<StoredShipsEvent.StoredShipsEventArgs>(json);

                                        HandleStoredShips(info);
                                    }
                                    /*else if (json?.Contains("\"event\":\"ShipyardTransfer\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<ShipyardTransferEvent.ShipyardTransferEventArgs>(json);
                                    }*/
                                    else if (json?.Contains("\"event\":\"Loadout\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<LoadoutEvent.LoadoutEventArgs>(json);

                                        HandleLoadout(info);
                                    }


                                }
                                catch (Exception ex)
                                {
                                    App.log.Error(ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }

            return journalDirectory.FullName;
        }

    }
}
