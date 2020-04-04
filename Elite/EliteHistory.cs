using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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

            //hardpoints = new List<Hardpoint>();
            //compartments = new List<Compartment>();
            //launchbays = new List<LaunchBay>();
            public string Bulkhead { get; set; }
            public string PowerPlant { get; set; }
            public string Engine { get; set; }
            public string PowerDistributor { get; set; }
            public string FrameShiftDrive { get; set; }
            public string LifeSupport { get; set; }
            public string Sensors { get; set; }
            public string GuardianFSDBooster { get; set; }

        }

        public static List<ShipData> ShipsList = new List<ShipData>();


        public static Dictionary<string, List<double>> VisitedSystemList = new Dictionary<string, List<double>>();

        public static double ImageWidth { get; set; }
        public static double ImageHeight { get; set; }

        private class UnsafeNativeMethods
        {
            [DllImport("Shell32.dll")]
            public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
                IntPtr hToken, out IntPtr ppszPath);
        }


        /// <summary>
        /// The standard Directory of the Player Journal files (C:\Users\%username%\Saved Games\Frontier Developments\Elite Dangerous).
        /// </summary>
        public static DirectoryInfo StandardDirectory
        {
            get
            {
                int result = UnsafeNativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
                    0, new IntPtr(0), out IntPtr path);
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

                var imgX = (x - SpaceMinX) / (SpaceMaxX - SpaceMinX) * ImageWidth;
                var imgY = (SpaceMaxZ - z) / (SpaceMaxZ - SpaceMinZ) * ImageHeight;

                TravelHistoryPoints.Add(new PointF
                {
                    X = (float) imgX,
                    Y = (float) imgY
                });
            }
        }

        public static ShipData GetCurrentShip()
        {
            return ShipsList.FirstOrDefault(x => x.Stored == false);
        }


        public static void AddShip(int shipId, string shipType, string starSystem, string stationName,
            List<double> starPos, bool stored)
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

                    StarSystem = starSystem,
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

        public static void SetCurrentShip(int shipId, string shipType, string starSystem, string stationName,
            List<double> starPos)
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

                    item.Distance = (double) Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
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
                RemoveShip((int) info.SellShipID, info.SellOldShip.ToLower());
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

        public static void HandleShipyardSwap(ShipyardSwapEvent.ShipyardSwapEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                if (info.SellShipID != null && !string.IsNullOrEmpty(info.SellOldShip))
                {
                    RemoveShip((int) info.SellShipID, info.SellOldShip.ToLower());
                }

                if (info.StoreShipID != null && !ShipsList.Any(x =>
                    x.ShipType == info.StoreOldShip.ToLower() && x.ShipID == info.StoreShipID))
                {
                    AddShip((int) info.StoreShipID, info.StoreOldShip.ToLower(), ship.StarSystem, ship.StationName,
                        ship.StarPos, true);
                }

                if (!ShipsList.Any(x => x.ShipType == info.ShipType.ToLower() && x.ShipID == info.ShipID))
                {
                    AddShip(info.ShipID, info.ShipType.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos,
                        false);
                }

                SetCurrentShip(info.ShipID, info.ShipType.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos);
            }

        }

        public static void HandleShipyardNew(ShipyardNewEvent.ShipyardNewEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                AddShip(info.NewShipID, info.ShipType.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos, true);
            }
        }

        public static void HandleShipDocked(string starSystem, string stationName)
        {
            VisitedSystemList.TryGetValue(starSystem, out var starPos);

            var ship = GetCurrentShip();

            if (ship != null && starPos != null)
            {
                ship.StarSystem = starSystem;
                ship.StationName = stationName;
                ship.StarPos = starPos;
            }
        }

        public static void HandleShipLocation(bool docked, string starSystem, string stationName, List<double> starPos)
        {
            var ship = GetCurrentShip();

            if (docked && ship != null)
            {
                ship.StarSystem = starSystem;
                ship.StationName = stationName;
                ship.StarPos = starPos;
            }
        }

        public static int GetModuleSize(string item)
        {
            var size = item.Substring(item.IndexOf("_size", StringComparison.OrdinalIgnoreCase) + 5);

            if (size.IndexOf("_", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                size = size.Substring(0, size.IndexOf("_", StringComparison.OrdinalIgnoreCase));
            }

            return Convert.ToInt32(size);
        }

        public static string GetModuleClass(string item)
        {
            var cl = item.Substring(item.IndexOf("_class", StringComparison.OrdinalIgnoreCase) + 6);

            if (cl.IndexOf("_", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                cl = cl.Substring(0, cl.IndexOf("_", StringComparison.OrdinalIgnoreCase));
            }

            switch (cl)
            {
                case "1": return "E";
                case "2": return "D";
                case "3": return "C";
                case "4": return "B";
                case "5": return "A";
                default: return "?";
            }
        }

        public static string GetModuleArmourGrade(string item)
        {
            if (item.IndexOf("_grade1", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Lightweight Alloy";
            }
            if (item.IndexOf("_grade2", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Reinforced Alloy";
            }
            if (item.IndexOf("_grade3", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Military Grade Composite";
            }
            if (item.IndexOf("_mirrored", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Mirrored Surface Composite";
            }
            if (item.IndexOf("_reactive", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Reactive Surface Composite";
            }

            return "?";
        }
        
        public static int UpdateFuelCapacity(string item)
        {
            if (item?.Contains("_fueltank_") == true)
            {
                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return Convert.ToInt32(Math.Pow(2, size));
            }

            return 0;
        }

        public static int UpdateCargoCapacity(string item)
        {
            if (item?.Contains("cargorack_") == true)
            {
                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return Convert.ToInt32(Math.Pow(2, size));
            }

            return 0;
        }

        public static string UpdatePowerPlant(string item, string data, bool remove)
        {
            if (item?.Contains("powerplant_") == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return size.ToString() + cl;
            }

            return data;
        }

        public static string UpdatePowerDistributor(string item, string data, bool remove)
        {
            if (item?.Contains("powerdistributor_") == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return size.ToString() + cl;
            }

            return data;
        }

        public static string UpdateLifeSupport(string item, string data, bool remove)
        {
            if (item?.Contains("_lifesupport_") == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return size.ToString() + cl;
            }

            return data;
        }

        public static string UpdateEngine(string item, string data, bool remove)
        {
            if (item?.Contains("_engine_") == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return size.ToString() + cl;
            }

            return data;
        }

        public static string UpdateFrameShiftDrive(string item, string data, bool remove)
        {
            if (item?.Contains("_hyperdrive_") == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return size.ToString() + cl;
            }

            return data;
        }
        
        public static string UpdateArmour(string item, string data, bool remove)
        {
            if (item?.Contains("_armour_") == true)
            {
                if (remove) return null;

                var grade = GetModuleArmourGrade(item);

                return grade;
            }

            return data;
        }

        public static string UpdateSensors(string item, string data, bool remove)
        {
            if (item?.Contains("_sensors_") == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return size.ToString() + cl;
            }

            return data;
        }
        
        public static string UpdateGuardianFSDBooster(string item, string data, bool remove)
        {
            if (item?.Contains("_guardianfsdbooster_") == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);

                return " + booster"; //size.ToString();
            }

            return data;
        }

        public static void HandleLoadout(LoadoutEvent.LoadoutEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                ship.HullValue = info.HullValue;
                ship.ModulesValue = info.ModulesValue;
                ship.HullHealth = info.HullHealth * 100.0;
                ship.UnladenMass = info.UnladenMass;

                ship.MaxJumpRange = info.MaxJumpRange;
                ship.Rebuy = info.Rebuy;
                ship.Hot = info.Hot;

                //ship.CargoCapacity = info.CargoCapacity;
                //ship.FuelCapacity = info.FuelCapacity.Main;

                ship.CargoCapacity = 0;
                ship.FuelCapacity = 0;
                ship.Bulkhead = null;
                ship.PowerPlant = null;
                ship.Engine = null;
                ship.PowerDistributor = null;
                ship.FrameShiftDrive = null;
                ship.LifeSupport = null;
                ship.Sensors = null;
                ship.GuardianFSDBooster = null;

                if (info.Modules != null)
                {
                    ship.Modules = info.Modules?.Select(m => m.Clone()).ToArray();

                    foreach (var m in info.Modules)
                    {
                        ship.CargoCapacity += UpdateCargoCapacity(m.Item);
                        ship.FuelCapacity += UpdateFuelCapacity(m.Item);

                        ship.Bulkhead = UpdateArmour(m.Item, ship.Bulkhead, false);
                        ship.PowerPlant = UpdatePowerPlant(m.Item, ship.PowerPlant, false);
                        ship.Engine = UpdateEngine(m.Item, ship.Engine, false);
                        ship.PowerDistributor = UpdatePowerDistributor(m.Item, ship.PowerDistributor, false);
                        ship.FrameShiftDrive = UpdateFrameShiftDrive(m.Item, ship.FrameShiftDrive, false);
                        ship.LifeSupport = UpdateLifeSupport(m.Item, ship.LifeSupport, false);
                        ship.Sensors = UpdateSensors(m.Item, ship.Sensors, false);
                        ship.GuardianFSDBooster = UpdateGuardianFSDBooster(m.Item, ship.GuardianFSDBooster, false);
                    }
                }
            }
        }

        public static void HandleModuleRetrieve(ModuleRetrieveEvent.ModuleRetrieveEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                ship.CargoCapacity += UpdateCargoCapacity(info.RetrievedItem);
                ship.FuelCapacity += UpdateFuelCapacity(info.RetrievedItem);

                ship.Bulkhead = UpdateArmour(info.RetrievedItem, ship.Bulkhead, false);
                ship.PowerPlant = UpdatePowerPlant(info.RetrievedItem, ship.PowerPlant, false);
                ship.Engine = UpdateEngine(info.RetrievedItem, ship.Engine, false);
                ship.PowerDistributor = UpdatePowerDistributor(info.RetrievedItem, ship.PowerDistributor, false);
                ship.FrameShiftDrive = UpdateFrameShiftDrive(info.RetrievedItem, ship.FrameShiftDrive, false);
                ship.LifeSupport = UpdateLifeSupport(info.RetrievedItem, ship.LifeSupport, false);
                ship.Sensors = UpdateSensors(info.RetrievedItem, ship.Sensors, false);
                ship.GuardianFSDBooster = UpdateGuardianFSDBooster(info.RetrievedItem, ship.GuardianFSDBooster, false);
            }
        }

        public static void HandleModuleBuy(ModuleBuyEvent.ModuleBuyEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                ship.CargoCapacity -= UpdateCargoCapacity(info.SellItem);
                ship.FuelCapacity -= UpdateFuelCapacity(info.SellItem);

                ship.Bulkhead = UpdateArmour(info.SellItem, ship.Bulkhead, true);
                ship.PowerPlant = UpdatePowerPlant(info.SellItem, ship.PowerPlant, true);
                ship.Engine = UpdateEngine(info.SellItem, ship.Engine, true);
                ship.PowerDistributor = UpdatePowerDistributor(info.SellItem, ship.PowerDistributor, true);
                ship.FrameShiftDrive = UpdateFrameShiftDrive(info.SellItem, ship.FrameShiftDrive, true);
                ship.LifeSupport = UpdateLifeSupport(info.SellItem, ship.LifeSupport, true);
                ship.Sensors = UpdateSensors(info.SellItem, ship.Sensors, true);
                ship.GuardianFSDBooster = UpdateGuardianFSDBooster(info.SellItem, ship.GuardianFSDBooster, true);

                ship.CargoCapacity += UpdateCargoCapacity(info.BuyItem);
                ship.FuelCapacity += UpdateFuelCapacity(info.BuyItem);

                ship.Bulkhead = UpdateArmour(info.BuyItem, ship.Bulkhead, false);
                ship.PowerPlant = UpdatePowerPlant(info.BuyItem, ship.PowerPlant, false);
                ship.Engine = UpdateEngine(info.BuyItem, ship.Engine, false);
                ship.PowerDistributor = UpdatePowerDistributor(info.BuyItem, ship.PowerDistributor, false);
                ship.FrameShiftDrive = UpdateFrameShiftDrive(info.BuyItem, ship.FrameShiftDrive, false);
                ship.LifeSupport = UpdateLifeSupport(info.BuyItem, ship.LifeSupport, false);
                ship.Sensors = UpdateSensors(info.BuyItem, ship.Sensors, false);
                ship.GuardianFSDBooster = UpdateGuardianFSDBooster(info.BuyItem, ship.GuardianFSDBooster, false);
            }
        }

        public static void HandleModuleSwap(ModuleSwapEvent.ModuleSwapEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                ship.CargoCapacity -= UpdateCargoCapacity(info.FromItem);
                ship.FuelCapacity -= UpdateFuelCapacity(info.FromItem);

                ship.Bulkhead = UpdateArmour(info.FromItem, ship.Bulkhead, true);
                ship.PowerPlant = UpdatePowerPlant(info.FromItem, ship.PowerPlant, true);
                ship.Engine = UpdateEngine(info.FromItem, ship.Engine, true);
                ship.PowerDistributor = UpdatePowerDistributor(info.FromItem, ship.PowerDistributor, true);
                ship.FrameShiftDrive = UpdateFrameShiftDrive(info.FromItem, ship.FrameShiftDrive, true);
                ship.LifeSupport = UpdateLifeSupport(info.FromItem, ship.LifeSupport, true);
                ship.Sensors = UpdateSensors(info.FromItem, ship.Sensors, true);
                ship.GuardianFSDBooster = UpdateGuardianFSDBooster(info.FromItem, ship.GuardianFSDBooster, true);

                ship.CargoCapacity += UpdateCargoCapacity(info.ToItem);
                ship.FuelCapacity += UpdateFuelCapacity(info.ToItem);

                ship.Bulkhead = UpdateArmour(info.ToItem, ship.Bulkhead, false);
                ship.PowerPlant = UpdatePowerPlant(info.ToItem, ship.PowerPlant, false);
                ship.Engine = UpdateEngine(info.ToItem, ship.Engine, false);
                ship.PowerDistributor = UpdatePowerDistributor(info.ToItem, ship.PowerDistributor, false);
                ship.FrameShiftDrive = UpdateFrameShiftDrive(info.ToItem, ship.FrameShiftDrive, false);
                ship.LifeSupport = UpdateLifeSupport(info.ToItem, ship.LifeSupport, false);
                ship.Sensors = UpdateSensors(info.ToItem, ship.Sensors, false);
                ship.GuardianFSDBooster = UpdateGuardianFSDBooster(info.ToItem, ship.GuardianFSDBooster, false);

            }
        }

        public static void HandleModuleSell(ModuleSellEvent.ModuleSellEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                ship.CargoCapacity -= UpdateCargoCapacity(info.SellItem);
                ship.FuelCapacity -= UpdateFuelCapacity(info.SellItem);

                ship.Bulkhead = UpdateArmour(info.SellItem, ship.Bulkhead, true);
                ship.PowerPlant = UpdatePowerPlant(info.SellItem, ship.PowerPlant, true);
                ship.Engine = UpdateEngine(info.SellItem, ship.Engine, true);
                ship.PowerDistributor = UpdatePowerDistributor(info.SellItem, ship.PowerDistributor, true);
                ship.FrameShiftDrive = UpdateFrameShiftDrive(info.SellItem, ship.FrameShiftDrive, true);
                ship.LifeSupport = UpdateLifeSupport(info.SellItem, ship.LifeSupport, true);
                ship.Sensors = UpdateSensors(info.SellItem, ship.Sensors, true);
                ship.GuardianFSDBooster = UpdateGuardianFSDBooster(info.SellItem, ship.GuardianFSDBooster, true);
            }
        }

        public static void HandleModuleSellRemote(ModuleSellRemoteEvent.ModuleSellRemoteEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                ship.CargoCapacity -= UpdateCargoCapacity(info.SellItem);
                ship.FuelCapacity -= UpdateFuelCapacity(info.SellItem);

                ship.Bulkhead = UpdateArmour(info.SellItem, ship.Bulkhead, true);
                ship.PowerPlant = UpdatePowerPlant(info.SellItem, ship.PowerPlant, true);
                ship.Engine = UpdateEngine(info.SellItem, ship.Engine, true);
                ship.PowerDistributor = UpdatePowerDistributor(info.SellItem, ship.PowerDistributor, true);
                ship.FrameShiftDrive = UpdateFrameShiftDrive(info.SellItem, ship.FrameShiftDrive, true);
                ship.LifeSupport = UpdateLifeSupport(info.SellItem, ship.LifeSupport, true);
                ship.Sensors = UpdateSensors(info.SellItem, ship.Sensors, true);
                ship.GuardianFSDBooster = UpdateGuardianFSDBooster(info.SellItem, ship.GuardianFSDBooster, true);
            }
        }

        public static void HandleModuleStore(ModuleStoreEvent.ModuleStoreEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                ship.CargoCapacity -= UpdateCargoCapacity(info.StoredItem);
                ship.FuelCapacity -= UpdateFuelCapacity(info.StoredItem);

                ship.Bulkhead = UpdateArmour(info.StoredItem, ship.Bulkhead, true);
                ship.PowerPlant = UpdatePowerPlant(info.StoredItem, ship.PowerPlant, true);
                ship.Engine = UpdateEngine(info.StoredItem, ship.Engine, true);
                ship.PowerDistributor = UpdatePowerDistributor(info.StoredItem, ship.PowerDistributor, true);
                ship.FrameShiftDrive = UpdateFrameShiftDrive(info.StoredItem, ship.FrameShiftDrive, true);
                ship.LifeSupport = UpdateLifeSupport(info.StoredItem, ship.LifeSupport, true);
                ship.Sensors = UpdateSensors(info.StoredItem, ship.Sensors, true);
                ship.GuardianFSDBooster = UpdateGuardianFSDBooster(info.StoredItem, ship.GuardianFSDBooster, true);
            }
        }

        public static void HandleMassModuleStore(MassModuleStoreEvent.MassModuleStoreEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                foreach (var i in info.Items)
                {
                    ship.CargoCapacity -= UpdateCargoCapacity(i.Name);
                    ship.FuelCapacity -= UpdateFuelCapacity(i.Name);

                    ship.Bulkhead = UpdateArmour(i.Name, ship.Bulkhead, true);
                    ship.PowerPlant = UpdatePowerPlant(i.Name, ship.PowerPlant, true);
                    ship.Engine = UpdateEngine(i.Name, ship.Engine, true);
                    ship.PowerDistributor = UpdatePowerDistributor(i.Name, ship.PowerDistributor, true);
                    ship.FrameShiftDrive = UpdateFrameShiftDrive(i.Name, ship.FrameShiftDrive, true);
                    ship.LifeSupport = UpdateLifeSupport(i.Name, ship.LifeSupport, true);
                    ship.Sensors = UpdateSensors(i.Name, ship.Sensors, true);
                    ship.GuardianFSDBooster = UpdateGuardianFSDBooster(i.Name, ship.GuardianFSDBooster, true);
                }
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

                                        HandleShipLocation(info.Docked, info.StarSystem, info.StationName,
                                            info.StarPos);
                                    }
                                    else if (json?.Contains("\"event\":\"Docked\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<DockedInfo>(json);

                                        HandleShipDocked(info.StarSystem, info.StationName);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardNew\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<ShipyardNewEvent.ShipyardNewEventArgs>(
                                                json);

                                        HandleShipyardNew(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardBuy\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<ShipyardBuyEvent.ShipyardBuyEventArgs>(
                                                json);

                                        HandleShipyardBuy(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardSell\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ShipyardSellEvent.ShipyardSellEventArgs>(json);

                                        HandleShipyardSell(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ShipyardSwap\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ShipyardSwapEvent.ShipyardSwapEventArgs>(json);

                                        HandleShipyardSwap(info);
                                    }
                                    else if (json?.Contains("\"event\":\"StoredShips\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<StoredShipsEvent.StoredShipsEventArgs>(
                                                json);

                                        HandleStoredShips(info);
                                    }
                                    /*else if (json?.Contains("\"event\":\"ShipyardTransfer\",") == true)
                                    {
                                        var info = JsonConvert.DeserializeObject<ShipyardTransferEvent.ShipyardTransferEventArgs>(json);
                                    }*/
                                    else if (json?.Contains("\"event\":\"Loadout\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<LoadoutEvent.LoadoutEventArgs>(json);

                                        HandleLoadout(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleBuy\",") == true)
                                    {
                                        var info =
                                            JsonConvert.DeserializeObject<ModuleBuyEvent.ModuleBuyEventArgs>(json);

                                        HandleModuleBuy(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleSell\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleSellEvent.ModuleSellEventArgs>(json);

                                        HandleModuleSell(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleSellRemote\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleSellRemoteEvent.ModuleSellRemoteEventArgs>(
                                                json);

                                        HandleModuleSellRemote(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleStore\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleStoreEvent.ModuleStoreEventArgs>(json);

                                        HandleModuleStore(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleSwap\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleSwapEvent.ModuleSwapEventArgs>(json);

                                        HandleModuleSwap(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ModuleRetrieve\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ModuleRetrieveEvent.ModuleRetrieveEventArgs>(json);

                                        HandleModuleRetrieve(info);
                                    }
                                    else if (json?.Contains("\"event\":\"MassModuleStore\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<MassModuleStoreEvent.MassModuleStoreEventArgs>(json);

                                        HandleMassModuleStore(info);
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

