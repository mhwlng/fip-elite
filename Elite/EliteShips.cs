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
    public static class EliteShips
    {
        public static readonly Dictionary<string, string> ShipsByEliteID = new Dictionary<string, string>()
        {
            {
                "sidewinder", "Sidewinder"
            },
            {
                "eagle", "Eagle"
            },
            {
                "hauler", "Hauler"
            },
            {
                "adder", "Adder"
            },
            {
                "viper", "Viper Mk III"
            },
            {
                "cobramkiii", "Cobra Mk III"
            },
            {
                "type6", "Type-6 Transporter"
            },
            {
                "dolphin", "Dolphin"
            },
            {
                "type7", "Type-7 Transporter"
            },
            {
                "asp", "Asp Explorer"
            },
            {
                "vulture", "Vulture"
            },
            {
                "empire_trader", "Imperial Clipper"
            },
            {
                "federation_dropship", "Federal Dropship"
            },
            {
                "orca", "Orca"
            },
            {
                "type9", "Type-9 Heavy"
            },
            {
                "type9_military", "Type-10 Defender"
            },
            {
                "python", "Python"
            },
            {
                "belugaliner", "Beluga Liner"
            },
            {
                "ferdelance", "Fer-de-Lance"
            },
            {
                "anaconda", "Anaconda"
            },
            {
                "federation_corvette", "Federal Corvette"
            },
            {
                "cutter", "Imperial Cutter"
            },
            {
                "diamondback", "Diamondback Scout"
            },
            {
                "empire_courier", "Imperial Courier"
            },
            {
                "diamondbackxl", "Diamondback Explorer"
            },
            {
                "empire_eagle", "Imperial Eagle"
            },
            {
                "federation_dropship_mkii", "Federal Assault Ship"
            },
            {
                "federation_gunship", "Federal Gunship"
            },
            {
                "viper_mkiv", "Viper Mk IV"
            },
            {
                "cobramkiv", "Cobra Mk IV"
            },
            {
                "independant_trader", "Keelback"
            },
            {
                "asp_scout", "Asp Scout"
            },
            {
                "testbuggy", "SRV"
            },
            {
                "typex", "Alliance Chieftain"
            },
            {
                "typex_2", "Alliance Crusader"
            },
            {
                "typex_3", "Alliance Challenger"
            },
            {
                "krait_mkii", "Krait Mk II"
            },
            {
                "krait_light", "Krait Phantom"
            },
            {
                "mamba", "Mamba"
            },
            {
                "empire_fighter", "Imperial Fighter"
            },
            {
                "federation_fighter", "F63 Condor"
            },
            {
                "independent_fighter", "Taipan Fighter"
            },
            {
                "gdn_hybrid_fighter_v1", "Trident"
            },
            {
                "gdn_hybrid_fighter_v2", "Javelin"
            },
            {
                "gdn_hybrid_fighter_v3", "Lance"
            },
            {
                "unknown", "Unknown"
            },
            {
                "unknown (crew)", "Unknown (crew)"
            },
            {
                "unknown (captain)", "Unknown (captain)"
            }
        };

        public class ShipData
        {
            public bool Stored { get; set; }
            public int ShipID { get; set; }
            public string ShipType { get; set; }
            public string ShipTypeFull
            {
                get
                {
                    ShipsByEliteID.TryGetValue(this.ShipType.ToLower(), out var shipTypeFull);
                    return shipTypeFull?.Trim() ?? this.ShipType?.Trim();
                }
            }
            public string ShipImage => this.ShipTypeFull?.Trim() + ".png";

            public string Name { get; set; } = "";

            public bool AutomaticDocking { get; set; }
            public string StationName { get; set; }
            public string StarSystem { get; set; }
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

            public string Bulkhead { get; set; }
            public string PowerPlant { get; set; }
            public string Engine { get; set; }
            public string PowerDistributor { get; set; }
            public string FrameShiftDrive { get; set; }
            public string FuelScoop { get; set; }
            public string FighterHangar { get; set; }
            public string Refinery { get; set; }
            public string LifeSupport { get; set; }
            public string Sensors { get; set; }
            public string GuardianFSDBooster { get; set; }
            public string ShieldGenerator { get; set; }

            public int EconomyClassPassengerCabinCapacity { get; set; }
            public int BusinessClassPassengerCabinCapacity { get; set; }
            public int FirstClassPassengerCabinCapacity { get; set; }
            public int LuxuryClassPassengerCabinCapacity { get; set; }
            public string Cabins
            {
                get
                {
                    var c = new List<string>();
                    if (this.EconomyClassPassengerCabinCapacity > 0)
                    {
                        c.Add("E=" + this.EconomyClassPassengerCabinCapacity);
                    }

                    if (this.BusinessClassPassengerCabinCapacity > 0)
                    {
                        c.Add("B=" + this.BusinessClassPassengerCabinCapacity);
                    }

                    if (this.FirstClassPassengerCabinCapacity > 0)
                    {
                        c.Add("F=" + this.FirstClassPassengerCabinCapacity);
                    }

                    if (this.LuxuryClassPassengerCabinCapacity > 0)
                    {
                        c.Add("L=" + this.LuxuryClassPassengerCabinCapacity);
                    }

                    return string.Join("+", c);
                }
            }

            public List<string> VehicleBaysList { get; set; } = new List<string>();
            public string VehicleBays => GetPlacement(this.VehicleBaysList);

            public List<string> CannonsList { get; set; } = new List<string>();
            public List<string> MultiCannonsList { get; set; } = new List<string>();
            public List<string> PulseLasersList { get; set; } = new List<string>();
            public List<string> BeamLasersList { get; set; } = new List<string>();
            public List<string> BurstLasersList { get; set; } = new List<string>();
            public string Cannons => GetPlacement(this.CannonsList);
            public string MultiCannons => GetPlacement(this.MultiCannonsList);
            public string PulseLasers => GetPlacement(this.PulseLasersList);
            public string BeamLasers => GetPlacement(this.BeamLasersList);
            public string BurstLasers => GetPlacement(this.BurstLasersList);
        }

        public static List<ShipData> ShipsList = new List<ShipData>();

        public static ShipData GetCurrentShip()
        {
            return ShipsList.FirstOrDefault(x => x.Stored == false);
        }

        public static void AddShip(int shipId, string shipType, string starSystem, string stationName,
            List<double> starPos, bool stored)
        {
            if (shipType == "testbuggy") return;

            if (starPos == null) return;

            if (!ShipsList.Any(x => x.ShipType == shipType?.ToLower() && x.ShipID == shipId))
            {
                ShipsList.Add(new ShipData()
                {
                    ShipID = shipId,
                    ShipType = shipType.ToLower(),

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
                x.ShipType == shipType?.ToLower() && x.ShipID == shipId);

            if (ship != null)
            {
                ShipsList.Remove(ship);
            }
        }

        public static void SetCurrentShip(int shipId, string shipType, string starSystem, string stationName,
            List<double> starPos)
        {
            if (shipType == "testbuggy") return;

            if (!ShipsList.Any(x => x.ShipType == shipType?.ToLower() && x.ShipID == shipId))
            {
                AddShip(shipId, shipType, starSystem, stationName, starPos, false);
            }

            foreach (var s in ShipsList)
            {
                s.Stored = (s.ShipType != shipType?.ToLower() && s.ShipID != shipId);
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
            if (!EliteHistory.VisitedSystemList.ContainsKey(starSystem))
            {
                EliteHistory.VisitedSystemList.Add(starSystem, starPos);
            }
        }

        public static void HandleShipyardSell(ShipyardSellEvent.ShipyardSellEventArgs info)
        {
            if (!String.IsNullOrEmpty(info.ShipType))
            {
                RemoveShip(info.SellShipId, info.ShipType.ToLower());
            }
        }

        public static void HandleShipyardBuy(ShipyardBuyEvent.ShipyardBuyEventArgs info)
        {
            if (info.SellShipID != null && !String.IsNullOrEmpty(info.SellOldShip))
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
                EliteHistory.VisitedSystemList.TryGetValue(info.StarSystem, out var starPos);

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
                if (info.SellShipID != null && !String.IsNullOrEmpty(info.SellOldShip))
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

        public static string GetPlacement(List<string> weaponsList)
        {
            return string.Join(" + ",
                weaponsList.GroupBy(x => x).Select(x => x.Count() + x.Key));

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

        public static int UpdatePassengerCabinCapacity(string item, string cls)
        {
            if (item?.Contains("_passengercabin_") == true)
            {
                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                if (cls == cl)
                {
                    if (cl == "E")
                    {
                        switch (size)
                        {
                            case 2: return 2;
                            case 3: return 4;
                            case 4: return 8;
                            case 5: return 16;
                            case 6: return 32;
                        }
                    }
                    else if (cl == "D")
                    {
                        switch (size)
                        {
                            case 3: return 3;
                            case 4: return 6;
                            case 5: return 10;
                            case 6: return 16;
                        }
                    }
                    else if (cl == "C")
                    {
                        switch (size)
                        {
                            case 4: return 3;
                            case 5: return 6;
                            case 6: return 12;
                        }
                    }
                    else if (cl == "B")
                    {
                        switch (size)
                        {
                            case 5: return 4;
                            case 6: return 8;
                        }
                    }
                }
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

                if (item?.Contains("guardian") == true)
                {
                    cl += " guardian";
                }

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

                if (item?.Contains("guardian") == true)
                {
                    cl += " guardian";
                }

                return size.ToString() + cl;
            }

            return data;
        }

        public static string UpdateShieldGenerator(string item, string data, bool remove)
        {
            if (item?.Contains("_shieldgenerator_") == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                if (item?.Contains("strong") == true)
                {
                    cl += " prismatic";
                }
                else if (item?.Contains("fast") == true)
                {
                    cl += " bi-weave";
                }

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

        public static string UpdateSizeClass(string key, string item, string data, bool remove)
        {
            if (item?.Contains(key) == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return size.ToString() + cl;
            }

            return data;
        }

        public static void UpdateVehicleBays(string item, List<string> data, bool remove)
        {
            if (item?.Contains("_buggybay_") == true)
            {
                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                var c = size.ToString() + cl;

                if (remove && data.Contains(c))
                {
                    data.Remove(c);
                }
                else
                {
                    data.Add(c);
                }
            }
        }

        public static void UpdateWeapons(string key, string item, List<string> data, bool remove)
        {
            if (item?.Contains(key) == true)
            {
                var c = "";

                if (item.Contains("small") == true) c += "S";
                else if (item.Contains("medium") == true) c += "M";
                else if (item.Contains("large") == true) c += "L";
                else if (item.Contains("huge") == true) c += "H";

                //if (item.Contains("fixed") == true) c += "-F";
                //else if (item.Contains("gimbal") == true) c += "-G";
                //else if (item.Contains("gimbal") == true) c += "-T";

                //if (item.Contains("strong") == true) c += "-S";

                if (remove && data.Contains(c))
                {
                    data.Remove(c);
                }
                else
                {
                    data.Add(c);
                }
            }
        }

        public static void HandleShipDocked(string starSystem, string stationName)
        {
            EliteHistory.VisitedSystemList.TryGetValue(starSystem, out var starPos);

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

        public static void HandleHullDamage(double health)
        {
            var ship = GetCurrentShip();
            if (ship != null)
            {
                ship.HullHealth = health * 100.0;
            }
        }


        public static void HandleLoadGame(int shipId, string shipType, string name)
        {
            var ship = GetCurrentShip();
            if (ship != null)
            {
                SetCurrentShip(shipId, shipType?.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos);

                ship.Name = name?.Trim();
            }
        }

        public static void HandleSetUserShipName(int shipId, string name, string shipType)
        {
            var ship = GetCurrentShip();
            if (ship != null)
            {
                SetCurrentShip(shipId, shipType?.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos);

                ship.Name = name?.Trim();
            }
        }

        public static void HandleModules(ShipData ship, string item, bool remove)
        {
            if (remove)
            {
                ship.CargoCapacity -= UpdateCargoCapacity(item);
                ship.FuelCapacity -= UpdateFuelCapacity(item);

                ship.EconomyClassPassengerCabinCapacity -= UpdatePassengerCabinCapacity(item,"E");
                ship.BusinessClassPassengerCabinCapacity -= UpdatePassengerCabinCapacity(item,"D");
                ship.FirstClassPassengerCabinCapacity -= UpdatePassengerCabinCapacity(item,"C");
                ship.LuxuryClassPassengerCabinCapacity -= UpdatePassengerCabinCapacity(item,"B");
            }
            else
            {
                ship.CargoCapacity += UpdateCargoCapacity(item);
                ship.FuelCapacity += UpdateFuelCapacity(item);

                ship.EconomyClassPassengerCabinCapacity += UpdatePassengerCabinCapacity(item,"E");
                ship.BusinessClassPassengerCabinCapacity += UpdatePassengerCabinCapacity(item,"D");
                ship.FirstClassPassengerCabinCapacity += UpdatePassengerCabinCapacity(item,"C");
                ship.LuxuryClassPassengerCabinCapacity += UpdatePassengerCabinCapacity(item,"B");
            }

            ship.FrameShiftDrive = UpdateSizeClass("_hyperdrive_", item, ship.FrameShiftDrive, remove);
            ship.FuelScoop = UpdateSizeClass("_fuelscoop_",item, ship.FuelScoop, remove);
            ship.FighterHangar = UpdateSizeClass("_fighterbay_",item, ship.FighterHangar, remove);
            ship.Refinery = UpdateSizeClass("_refinery_",item, ship.Refinery, remove);
            ship.LifeSupport = UpdateSizeClass("_lifesupport_",item, ship.LifeSupport, remove);
            ship.Sensors = UpdateSizeClass("_sensors_",item, ship.Sensors, remove);


            ship.Bulkhead = UpdateArmour(item, ship.Bulkhead, remove);
            ship.PowerPlant = UpdatePowerPlant(item, ship.PowerPlant, remove);
            ship.Engine = UpdateSizeClass("_engine_", item, ship.Engine, remove);
            ship.PowerDistributor = UpdatePowerDistributor(item, ship.PowerDistributor, remove);
            ship.ShieldGenerator = UpdateShieldGenerator(item, ship.ShieldGenerator, remove);
            ship.GuardianFSDBooster = UpdateGuardianFSDBooster(item, ship.GuardianFSDBooster, remove);

            UpdateVehicleBays(item, ship.VehicleBaysList, remove);

            UpdateWeapons("_cannon_", item, ship.CannonsList, remove);
            UpdateWeapons("_multicannon_", item, ship.MultiCannonsList, remove);
            UpdateWeapons("_pulselaser_", item, ship.PulseLasersList, remove);
            UpdateWeapons("_beamlaser_", item, ship.BeamLasersList, remove);
            UpdateWeapons("_pulselaserburst_", item, ship.BurstLasersList, remove);

            /*
            _MultiCannon_Fixed_Small_Strong"    "Enforcer Cannon"

            _PulseLaser_Fixed_Medium_Disruptor" "Disruptor"

            _basicmissilerack_fixed_small"	"missilerack"
            _causticmissile_fixed_medium"	"enzymemissilerack"
            _drunkmissilerack_fixed_medium", 1480, "pack_hound"

            _railgun_fixed_medium_burst"	"imperialhammer"

            _advancedtorppylon_fixed_small"	"torpedopylon"
            _dumbfiremissilerack_fixed_medium_lasso"	"rocketpropelledfsddisruptor"
            _flakmortar_fixed_medium"	"remotereleaseflaklauncher"
            _flechettelauncher_fixed_medium"	"flechettelauncher"
            _plasmaaccelerator_fixed_huge"	"plasmaaccelerator"
            _plasmashockcannon_fixed_large"	"shockcannon"
            _slugshot_fixed_large_range"	"pacifier"

            _xenoscanner_basic_tiny"	"xenoscanner"

            _minelauncher_fixed_small"	"minelauncher"

            _mininglaser_fixed_small"	"mininglaser"

            _heatsinklauncher_turret_tiny"	"heatsinklauncher"

            _electroniccountermeasure_tiny"	"electroniccountermeasure"
            _plasmapointdefence_turret_tiny"	"pointdefence"
            _antiunknownshutdown_tiny"	"shutdownfieldneutraliser"
            _chafflauncher_tiny"	"chafflauncher"

            _atdumbfiremissile_fixed_large"	"axmissilerack"
            _atmulticannon_fixed_large"	"ax multi-cannon"

            _guardian_gausscannon_fixed_medium"	"guardiangausscannon"
            _guardian_plasmalauncher_fixed_large"	"guardianplasmacharger"
            _guardian_shardcannon_fixed_large"	"shardcannon"
            */

        }


        public static void HandleLoadout(LoadoutEvent.LoadoutEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                SetCurrentShip(info.ShipID, info.Ship?.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos);

                ship.Name = info.ShipName?.Trim();

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
                ship.EconomyClassPassengerCabinCapacity = 0;
                ship.BusinessClassPassengerCabinCapacity = 0;
                ship.FirstClassPassengerCabinCapacity = 0;
                ship.LuxuryClassPassengerCabinCapacity = 0;

                ship.Bulkhead = null;
                ship.PowerPlant = null;
                ship.Engine = null;
                ship.PowerDistributor = null;
                ship.FrameShiftDrive = null;
                ship.FuelScoop = null;
                ship.FighterHangar = null;
                ship.Refinery = null;
                ship.LifeSupport = null;
                ship.Sensors = null;
                ship.GuardianFSDBooster = null;
                ship.ShieldGenerator = null;

                ship.VehicleBaysList = new List<string>();

                ship.CannonsList = new List<string>();
                ship.MultiCannonsList = new List<string>();
                ship.PulseLasersList = new List<string>();
                ship.BeamLasersList = new List<string>();
                ship.BurstLasersList = new List<string>();

                if (info.Modules != null)
                {
                    ship.Modules = info.Modules?.Select(m => m.Clone()).ToArray();

                    foreach (var m in info.Modules)
                    {
                        HandleModules(ship, m.Item, false);
                    }
                }

                ship.HullHealth = info.HullHealth * 100.0;
            }
        }

        public static void HandleModuleRetrieve(ModuleRetrieveEvent.ModuleRetrieveEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.RetrievedItem, false);
            }
        }

        public static void HandleModuleBuy(ModuleBuyEvent.ModuleBuyEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.SellItem, true);

                HandleModules(ship, info.BuyItem, false);
            }
        }

        public static void HandleModuleSwap(ModuleSwapEvent.ModuleSwapEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.FromItem, true);

                HandleModules(ship, info.ToItem, false);
            }
        }

        public static void HandleModuleSell(ModuleSellEvent.ModuleSellEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.SellItem, true);
            }
        }

        public static void HandleModuleSellRemote(ModuleSellRemoteEvent.ModuleSellRemoteEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.SellItem, true);
            }
        }

        public static void HandleModuleStore(ModuleStoreEvent.ModuleStoreEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.StoredItem, true);
            }
        }

        public static void HandleMassModuleStore(MassModuleStoreEvent.MassModuleStoreEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                foreach (var i in info.Items)
                {
                    HandleModules(ship, i.Name, true);
                }
            }
        }
    }
}