using System;
using System.Collections.Generic;
using System.Linq;
using EliteJournalReader;
using EliteJournalReader.Events;

namespace Elite
{
    public static class Module
    {
        // Frame Shift Drive Constants
        private static Dictionary<string, double> BaseOptimalMass = new Dictionary<string, double>
        {
            {"2E", 48.0}, {"2D", 54.0}, {"2C", 60.0}, {"2B", 75.0}, {"2A", 90.0},
            {"3E", 80.0}, {"3D", 90.0}, {"3C", 100.0}, {"3B", 125.0}, {"3A", 150.0},
            {"4E", 280.0}, {"4D", 315.0}, {"4C", 350.0}, {"4B", 438.0}, {"4A", 525.0},
            {"5E", 560.0}, {"5D", 630.0}, {"5C", 700.0}, {"5B", 875.0}, {"5A", 1050.0},
            {"6E", 960.0}, {"6D", 1080.0}, {"6C", 1200.0}, {"6B", 1500.0}, {"6A", 1800.0},
            {"7E", 1440.0}, {"7D", 1620.0}, {"7C", 1800.0}, {"7B", 2250.0}, {"7A", 2700.0}
        };

        private static Dictionary<string, double> BaseMaxFuelPerJump = new Dictionary<string, double>
        {
            {"2E", 0.60}, {"2D", 0.60}, {"2C", 0.60}, {"2B", 0.80}, {"2A", 0.90},
            {"3E", 1.20}, {"3D", 1.20}, {"3C", 1.20}, {"3B", 1.50}, {"3A", 1.80},
            {"4E", 2.00}, {"4D", 2.00}, {"4C", 2.00}, {"4B", 2.50}, {"4A", 3.00},
            {"5E", 3.30}, {"5D", 3.30}, {"5C", 3.30}, {"5B", 4.10}, {"5A", 5.00},
            {"6E", 5.30}, {"6D", 5.30}, {"6C", 5.30}, {"6B", 6.60}, {"6A", 8.00},
            {"7E", 8.50}, {"7D", 8.50}, {"7C", 8.50}, {"7B", 10.60}, {"7A", 12.80}
        };


        private static Dictionary<int, double> GuardianBoostFSD = new Dictionary<int, double>
        {
            {1, 4.00}, {2, 6.00}, {3, 7.75}, {4, 9.25}, {5, 10.50}
        };

        private static Dictionary<string, double> RatingConstantFSD = new Dictionary<string, double>
        {
            {"A", 12.0}, {"B", 10.0}, {"C", 8.0}, {"D", 10.0}, {"E", 11.0}
        };

        private static Dictionary<int, double> PowerConstantFSD = new Dictionary<int, double>
        {
            {2, 2.00}, {3, 2.15}, {4, 2.30}, {5, 2.45}, {6, 2.60}, {7, 2.75}, {8, 2.90}
        };


        private enum ModuleType
        {
            Weapon,
            SingleModule,
            MultipleModules
        }

        private class ModuleDataItem
        {
            public string Key { get; set; }
            public List<string> ExcludeKeys { get; set; }

            public ModuleType ModuleType { get; set; }
        }

        private static readonly Dictionary<string, ModuleDataItem> ModuleData = new Dictionary<string, ModuleDataItem>
        {
            {"Fuel Scoop", new ModuleDataItem { Key = "_fuelscoop_", ModuleType = ModuleType.SingleModule} },
            {"Fighter Bay", new ModuleDataItem { Key = "_fighterbay_", ModuleType = ModuleType.SingleModule} },
            {"Refinery", new ModuleDataItem { Key = "_refinery_", ModuleType = ModuleType.SingleModule} },
            {"Life Support", new ModuleDataItem { Key = "_lifesupport_", ModuleType = ModuleType.SingleModule} },
            {"Sensors", new ModuleDataItem { Key = "_sensors_", ModuleType = ModuleType.SingleModule} },

            {"FSD Interdictor", new ModuleDataItem { Key = "_fsdinterdictor_", ModuleType = ModuleType.SingleModule} },
            {"Pulse Wave Scanner", new ModuleDataItem { Key = "_mrascanner_", ModuleType = ModuleType.SingleModule} },
            {"Manifest Scanner", new ModuleDataItem { Key = "_cargoscanner_", ModuleType = ModuleType.SingleModule} },
            {"K-Warrant Scanner", new ModuleDataItem { Key = "_crimescanner_", ModuleType = ModuleType.SingleModule} },

            {"Research Limpet Contr.", new ModuleDataItem { Key = "_dronecontrol_unkvesselresearch", ModuleType = ModuleType.MultipleModules} },
            {"Hatch Breaker Limpet Contr.", new ModuleDataItem { Key = "_dronecontrol_resourcesiphon_", ModuleType = ModuleType.MultipleModules} },
            {"Collector Limpet Contr.", new ModuleDataItem { Key = "_dronecontrol_collection_", ModuleType = ModuleType.MultipleModules} },
            {"Fuel Transfer Limpet Contr.", new ModuleDataItem { Key = "_dronecontrol_fueltransfer_", ModuleType = ModuleType.MultipleModules} },
            {"Prospector Limpet Contr.", new ModuleDataItem { Key = "_dronecontrol_prospector_", ModuleType = ModuleType.MultipleModules} },
            {"Repair Limpet Contr.", new ModuleDataItem { Key = "_dronecontrol_repair_", ModuleType = ModuleType.MultipleModules} },
            {"Decontamination Limpet Contr.", new ModuleDataItem { Key = "_dronecontrol_decontamination_", ModuleType = ModuleType.MultipleModules} },
            {"Recon Limpet Contr.", new ModuleDataItem { Key = "_dronecontrol_recon_", ModuleType = ModuleType.MultipleModules} },

            {"Vehicle Bay", new ModuleDataItem { Key = "_buggybay_", ModuleType = ModuleType.MultipleModules} },
            {"AFMU", new ModuleDataItem { Key = "_repairer_", ModuleType = ModuleType.MultipleModules} },
            {"Hull Reinf." , new ModuleDataItem { Key = "_hullreinforcement_", ModuleType = ModuleType.MultipleModules} },
            {"Module Reinf.", new ModuleDataItem { Key = "_modulereinforcement_", ModuleType = ModuleType.MultipleModules} },
            {"MetaAlloy Hull Reinf.", new ModuleDataItem { Key = "_metaalloyhullreinforcement_", ModuleType = ModuleType.MultipleModules} },
            {"Guardian Hull Reinf.", new ModuleDataItem { Key = "_guardianhullreinforcement_", ModuleType = ModuleType.MultipleModules} },
            {"Guardian Module Reinf.", new ModuleDataItem { Key = "_guardianmodulereinforcement_", ModuleType = ModuleType.MultipleModules} },
            {"Shield Cell Bank", new ModuleDataItem { Key = "_shieldcellbank_", ModuleType = ModuleType.MultipleModules} },
            {"Shield Booster", new ModuleDataItem { Key = "_shieldbooster_", ModuleType = ModuleType.MultipleModules} },

            {"Mining Laser", new ModuleDataItem { Key = "_mininglaser_", ExcludeKeys = new List<string> {"advanced"}, ModuleType = ModuleType.Weapon} },
            {"Mining Lance" , new ModuleDataItem { Key = "_mininglaser_fixed_small_advanced", ModuleType = ModuleType.Weapon} },

            {"Displacement Missile" , new ModuleDataItem { Key = "_mining_subsurfdispmisle_", ModuleType = ModuleType.Weapon} },
            {"Abrasion Blaster" , new ModuleDataItem { Key = "_mining_abrblstr_", ModuleType = ModuleType.Weapon} },
            {"Seismic Charge" , new ModuleDataItem { Key = "_mining_seismchrgwarhd_", ModuleType = ModuleType.Weapon} },

            {"Cannon" , new ModuleDataItem { Key = "_cannon_", ModuleType = ModuleType.Weapon} },

            {"Multi-Cannon" , new ModuleDataItem { Key = "_multicannon_", ExcludeKeys = new List<string> { "strong", "advanced" } , ModuleType = ModuleType.Weapon} },
            {"Adv. Multi-Cannon" , new ModuleDataItem { Key = "_multicannon_fixed_medium_advanced", ModuleType = ModuleType.Weapon} },
            {"Enforcer" , new ModuleDataItem { Key = "_multicannon_fixed_small_strong", ModuleType = ModuleType.Weapon} },
            {"AX Multi-Cannon" , new ModuleDataItem { Key = "_atmulticannon_", ModuleType = ModuleType.Weapon} },

            {"Pulse Laser" , new ModuleDataItem { Key = "_pulselaser_", ExcludeKeys = new List<string> { "disruptor" }, ModuleType = ModuleType.Weapon} },
            {"Disruptor" , new ModuleDataItem { Key = "_pulselaser_fixed_medium_disruptor", ModuleType = ModuleType.Weapon} },

            {"Burst Laser" , new ModuleDataItem { Key = "_pulselaserburst_", ExcludeKeys = new List<string> { "scatter" }, ModuleType = ModuleType.Weapon} },
            {"Cytoscrambler" , new ModuleDataItem { Key = "_pulselaserburst_fixed_small_scatter", ModuleType = ModuleType.Weapon} },

            {"Beam Laser" , new ModuleDataItem { Key = "_beamlaser_", ModuleType = ModuleType.Weapon} },

            {"Fragment Cannon" , new ModuleDataItem { Key = "_slugshot_", ExcludeKeys = new List<string> { "range" }, ModuleType = ModuleType.Weapon} },
            {"Pacifier" , new ModuleDataItem { Key = "_slugshot_fixed_large_range", ModuleType = ModuleType.Weapon} },

            {"Shard Cannon" , new ModuleDataItem { Key = "_guardian_shardcannon_", ModuleType = ModuleType.Weapon} },
            {"Gauss Cannon" , new ModuleDataItem { Key = "_guardian_gausscannon_", ModuleType = ModuleType.Weapon} },
            {"Shock Cannon" , new ModuleDataItem { Key = "_plasmashockcannon_", ModuleType = ModuleType.Weapon} },

            {"Rail Gun" , new ModuleDataItem { Key = "_railgun_", ExcludeKeys = new List<string> { "burst" }, ModuleType = ModuleType.Weapon} },
            {"Imperial Hammer" , new ModuleDataItem { Key = "_railgun_fixed_medium_burst", ModuleType = ModuleType.Weapon} },

            {"Missile Rack" , new ModuleDataItem { Key = "_dumbfiremissilerack_", ExcludeKeys = new List<string> { "advanced", "lasso" }, ModuleType = ModuleType.Weapon} },
            {"Adv. Missile Rack" , new ModuleDataItem { Key = "_dumbfiremissilerack_fixed_small_advanced", ModuleType = ModuleType.Weapon} },
            {"FSD Disruptor" , new ModuleDataItem { Key = "_dumbfiremissilerack_fixed_medium_lasso", ModuleType = ModuleType.Weapon} },
            {"B Missile Rack" , new ModuleDataItem { Key = "_basicmissilerack_", ModuleType = ModuleType.Weapon} },
            {"E Missile Rack" , new ModuleDataItem { Key = "_causticmissile_", ModuleType = ModuleType.Weapon} },
            {"Pack Hound" , new ModuleDataItem { Key = "_drunkmissilerack_", ModuleType = ModuleType.Weapon} },
            {"AX Missile Rack" , new ModuleDataItem { Key = "_atdumbfiremissile_", ModuleType = ModuleType.Weapon} },

            {"Mine Launcher" , new ModuleDataItem { Key = "_minelauncher_", ExcludeKeys = new List<string> { "impulse" }, ModuleType = ModuleType.Weapon} },
            {"Shock Mine Launcher" , new ModuleDataItem { Key = "_minelauncher_fixed_small_impulse", ModuleType = ModuleType.Weapon} },

            {"Torpedo Pylon" , new ModuleDataItem { Key = "_advancedtorppylon_", ModuleType = ModuleType.Weapon} },
            {"Flak Launcher" , new ModuleDataItem { Key = "_flakmortar_fixed_", ModuleType = ModuleType.Weapon} },
            {"Flechette Launcher" , new ModuleDataItem { Key = "_flechettelauncher_", ModuleType = ModuleType.Weapon} },

            {"Plasma Accelerator" , new ModuleDataItem { Key = "_plasmaaccelerator_", ExcludeKeys = new List<string> { "advanced" }, ModuleType = ModuleType.Weapon} },
            {"Adv. Plasma Accelerator" , new ModuleDataItem { Key = "_plasmaaccelerator_fixed_large_advanced", ModuleType = ModuleType.Weapon} },
            {"Plasma Launcher" , new ModuleDataItem { Key = "_guardian_plasmalauncher_", ModuleType = ModuleType.Weapon} },

            {"Chaff" , new ModuleDataItem { Key = "_chafflauncher_", ModuleType = ModuleType.Weapon} },
            {"ECM" , new ModuleDataItem { Key = "_electroniccountermeasure_", ModuleType = ModuleType.Weapon} },
            {"Heat Sink" , new ModuleDataItem { Key = "_heatsinklauncher_turret_", ModuleType = ModuleType.Weapon} },
            {"Point Defence" , new ModuleDataItem { Key = "_plasmapointdefence_turret_", ModuleType = ModuleType.Weapon} },
            {"Xeno Scanner" , new ModuleDataItem { Key = "_xenoscanner_basic_", ModuleType = ModuleType.Weapon} },
            {"Shutdown Field Neutraliser" , new ModuleDataItem { Key = "_antiunknownshutdown_", ModuleType = ModuleType.Weapon} },
            {"Det. Surface Scanner" , new ModuleDataItem { Key = "_detailedsurfacescanner_", ModuleType = ModuleType.Weapon} }

        };

        private static int GetModuleSize(string item)
        {
            var size = item.Substring(item.IndexOf("_size", StringComparison.OrdinalIgnoreCase) + 5);

            if (size.IndexOf("_", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                size = size.Substring(0, size.IndexOf("_", StringComparison.OrdinalIgnoreCase));
            }

            if (size == "0")
            {
                size = "1";
            }

            if (!int.TryParse(size, out var ms))
            {
                return 1;
            }

            return ms;
        }

        private static string GetModuleClass(string item)
        {
            var cl = item.Substring(item.IndexOf("_class", StringComparison.OrdinalIgnoreCase) + 6);

            if (cl.IndexOf("_", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                cl = cl.Substring(0, cl.IndexOf("_", StringComparison.OrdinalIgnoreCase));
            }

            switch (cl)
            {
                case "0": return "";
                case "1": return "E";
                case "2": return "D";
                case "3": return "C";
                case "4": return "B";
                case "5": return "A";
                default: return "?";
            }
        }

        public static string GetModulePlacement(string key, IList<string> moduleList)
        {
            if (moduleList == null)
                return "";

            ModuleData.TryGetValue(key, out var moduleDataItem);
            if (moduleDataItem != null)
            {
                switch (moduleDataItem.ModuleType)
                {
                    case ModuleType.SingleModule:
                        return moduleList.FirstOrDefault();
                    case ModuleType.Weapon:
                    case ModuleType.MultipleModules:
                        return string.Join(" + ",
                            moduleList.GroupBy(x => x).Select(x => x.Count() + "&#10799;" + x.Key));
                }
            }

            return "?";
        }

        private static string GetModuleArmourGrade(string item)
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

        private static int UpdateFuelCapacity(string item)
        {
            if (item?.Contains("_fueltank_") == true)
            {
                var size = GetModuleSize(item);
                //var cl = GetModuleClass(item);

                return Convert.ToInt32(Math.Pow(2, size));
            }

            return 0;
        }

        private static int UpdateCargoCapacity(string item)
        {
            if (item?.Contains("cargorack_") == true)
            {
                var size = GetModuleSize(item);
                //var cl = GetModuleClass(item);

                return Convert.ToInt32(Math.Pow(2, size));
            }

            return 0;
        }

        private static int UpdatePassengerCabinCapacity(string item, string cls)
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

        private static string UpdatePowerPlant(string item, string data, bool remove)
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

                return size + cl;
            }

            return data;
        }

        private static string UpdatePowerDistributor(string item, string data, bool remove)
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

                return size + cl;
            }

            return data;
        }

        private static string UpdateShieldGenerator(string item, string data, bool remove)
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

                return size + cl;
            }

            return data;
        }

        private static string UpdateEngine(string item, string data, bool remove)
        {
            if (item?.Contains("_engine_") == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                if (item?.Contains("fast") == true)
                {
                    cl += " enhanced";
                }

                return size + cl;
            }

            return data;
        }

        private static string UpdateGuardianFsdBooster(string item, string data, bool remove)
        {
            if (item?.Contains("_guardianfsdbooster_") == true)
            {
                if (remove) return null;

                //var size = GetModuleSize(item);

                return " + booster"; //size.ToString();
            }

            return data;
        }

        private static string UpdateArmour(string item, string data, bool remove)
        {
            if (item?.Contains("_armour_") == true)
            {
                if (remove) return null;

                var grade = GetModuleArmourGrade(item);

                return grade;
            }

            return data;
        }

        private static string UpdateSizeClass(string key, string item, string data, bool remove)
        {
            if (item?.Contains(key) == true)
            {
                if (remove) return null;

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                return size + cl;
            }

            return data;
        }

        private static void UpdateSizeClass(string key, List<string> excludeKeys, string item, Dictionary<string, List<string>> moduleList, bool remove, string moduleName)
        {
            if (item?.Contains(key) == true)
            {
                if (excludeKeys != null)
                {
                    if (excludeKeys.Any(item.Contains))
                    {
                        return;
                    }
                }

                var size = GetModuleSize(item);
                var cl = GetModuleClass(item);

                var c = size + cl;

                moduleList.TryGetValue(moduleName, out var data);
                if (data == null)
                {
                    moduleList.Add(moduleName, new List<string>());
                    data = moduleList[moduleName];
                }

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

        private static void UpdateWeapons(string key, List<string> excludeKeys, string item, Dictionary<string, List<string>> moduleList, bool remove, string moduleName)
        {
            if (item?.Contains(key) == true)
            {
                if (excludeKeys != null)
                {
                    if (excludeKeys.Any(item.Contains))
                    {
                        return;
                    }
                }

                var c = "";

                if (item.Contains("small")) c += "S";
                else if (item.Contains("medium")) c += "M";
                else if (item.Contains("large")) c += "L";
                else if (item.Contains("huge")) c += "H";
                else if (item.Contains("tiny")) c += "T";

                //if (item.Contains("fixed") == true) c += "-F";
                //else if (item.Contains("gimbal") == true) c += "-G";
                //else if (item.Contains("gimbal") == true) c += "-T";

                //if (item.Contains("strong") == true) c += "-S";

                moduleList.TryGetValue(moduleName, out var data);
                if (data == null)
                {
                    moduleList.Add(moduleName, new List<string>());
                    data = moduleList[moduleName];
                }

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

        private static void HandleJumpRange(Ships.ShipData ship, EliteJournalReader.Module module)
        {
            if (module.Item.Contains("_hyperdrive_"))
            {
                var size = GetModuleSize(module.Item);
                var cl = GetModuleClass(module.Item);

                RatingConstantFSD.TryGetValue(cl, out var ratingConstant);
                if (ratingConstant > 0)
                {
                    ship.RatingConstant = ratingConstant;
                }

                PowerConstantFSD.TryGetValue(size, out var powerConstant);
                if (powerConstant > 0)
                {
                    ship.PowerConstant = powerConstant;
                }

                BaseMaxFuelPerJump.TryGetValue(size + cl, out var maxFuelPerJump);
                if (maxFuelPerJump > 0)
                {
                    ship.MaxFuelPerJump = maxFuelPerJump;
                }

                BaseOptimalMass.TryGetValue(size + cl, out var optimalMass);

                if (module.Engineering?.Modifiers != null)
                {
                    var mod = module.Engineering.Modifiers.Where(x =>
                        x.Label == ModuleAttribute.FSDOptimalMass).ToList();

                    if (mod.Any())
                    {
                        optimalMass = mod.FirstOrDefault().Value;
                    }
                }

                if (optimalMass > 0)
                {
                    ship.OptimalMass = optimalMass;
                }
            }
            else if (module.Item.Contains("_guardianfsdbooster_"))
            {
                var moduleSize = Convert.ToInt32(GetModuleSize(module.Item));

                GuardianBoostFSD.TryGetValue(moduleSize, out var boostConstant);

                if (boostConstant > 0)
                {
                    ship.BoostConstant = boostConstant;
                }
            }


        }

        private static void HandleModules(Ships.ShipData ship, string item, bool remove)
        {
            if (remove)
            {
                ship.CargoCapacity -= UpdateCargoCapacity(item);
                ship.FuelCapacity -= UpdateFuelCapacity(item);

                ship.EconomyClassPassengerCabinCapacity -= UpdatePassengerCabinCapacity(item, "E");
                ship.BusinessClassPassengerCabinCapacity -= UpdatePassengerCabinCapacity(item, "D");
                ship.FirstClassPassengerCabinCapacity -= UpdatePassengerCabinCapacity(item, "C");
                ship.LuxuryClassPassengerCabinCapacity -= UpdatePassengerCabinCapacity(item, "B");
            }
            else
            {
                ship.CargoCapacity += UpdateCargoCapacity(item);
                ship.FuelCapacity += UpdateFuelCapacity(item);

                ship.EconomyClassPassengerCabinCapacity += UpdatePassengerCabinCapacity(item, "E");
                ship.BusinessClassPassengerCabinCapacity += UpdatePassengerCabinCapacity(item, "D");
                ship.FirstClassPassengerCabinCapacity += UpdatePassengerCabinCapacity(item, "C");
                ship.LuxuryClassPassengerCabinCapacity += UpdatePassengerCabinCapacity(item, "B");
            }

            ship.Bulkhead = UpdateArmour(item, ship.Bulkhead, remove);
            ship.PowerPlant = UpdatePowerPlant(item, ship.PowerPlant, remove);
            ship.PowerDistributor = UpdatePowerDistributor(item, ship.PowerDistributor, remove);
            ship.ShieldGenerator = UpdateShieldGenerator(item, ship.ShieldGenerator, remove);
            ship.Engine = UpdateEngine(item, ship.Engine, remove);

            ship.FrameShiftDrive = UpdateSizeClass("_hyperdrive_", item, ship.FrameShiftDrive, remove);
            ship.GuardianFSDBooster = UpdateGuardianFsdBooster(item, ship.GuardianFSDBooster, remove);

            //-----------

            foreach (var m in ModuleData)
            {
                switch (m.Value.ModuleType)
                {
                    case ModuleType.SingleModule:
                    case ModuleType.MultipleModules:
                        UpdateSizeClass(m.Value.Key, m.Value.ExcludeKeys, item, ship.ModuleList, remove, m.Key);
                        break;
                    case ModuleType.Weapon:
                        UpdateWeapons(m.Value.Key, m.Value.ExcludeKeys, item, ship.ModuleList, remove, m.Key);
                        break;
                }
            }

            /* TODO:
            "int_dockingcomputer_standard", "Std. Docking Computer", "E"
            "int_dockingcomputer_advanced", "Adv. Docking Computer", "E"
            "int_stellarbodydiscoveryscanner_standard", "Basic Discovery Scanner", "E"
            "int_stellarbodydiscoveryscanner_intermediate", "Interm. Discovery Scanner", "D"
            "int_stellarbodydiscoveryscanner_advanced", "Adv. Discovery Scanner", "C"
            "int_planetapproachsuite", "Plan. Approach Suite", "I"
            "int_supercruiseassist", "Supercr. Assist", "E"
            "modularcargobaydoorfdl", "Cargo Hatch", "H"
            "modularcargobaydoor", "Cargo Hatch", "H"
            "...._cockpit", "Cockpit", "I"
            "ext_emitter_standard", "Shield Generator", "I"
            "ext_emitter_guardian", "Shield Generator", "I"

            */

        }

        public static void HandleModuleRetrieve(ModuleRetrieveEvent.ModuleRetrieveEventArgs info)
        {
            var ship = Ships.GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.RetrievedItem, false);
            }
        }

        public static void HandleModuleBuy(ModuleBuyEvent.ModuleBuyEventArgs info)
        {
            var ship = Ships.GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.SellItem, true);

                HandleModules(ship, info.BuyItem, false);
            }
        }

        public static void HandleModuleSwap(ModuleSwapEvent.ModuleSwapEventArgs info)
        {
            var ship = Ships.GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.FromItem, true);

                HandleModules(ship, info.ToItem, false);
            }
        }

        public static void HandleModuleSell(ModuleSellEvent.ModuleSellEventArgs info)
        {
            var ship = Ships.GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.SellItem, true);
            }
        }

        public static void HandleModuleSellRemote(ModuleSellRemoteEvent.ModuleSellRemoteEventArgs info)
        {
            var ship = Ships.GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.SellItem, true);
            }
        }

        public static void HandleModuleStore(ModuleStoreEvent.ModuleStoreEventArgs info)
        {
            var ship = Ships.GetCurrentShip();

            if (ship != null)
            {
                HandleModules(ship, info.StoredItem, true);
            }
        }

        public static void HandleMassModuleStore(MassModuleStoreEvent.MassModuleStoreEventArgs info)
        {
            var ship = Ships.GetCurrentShip();

            if (ship != null)
            {
                foreach (var i in info.Items)
                {
                    HandleModules(ship, i.Name, true);
                }
            }
        }

        public static void HandleLoadout(LoadoutEvent.LoadoutEventArgs info)
        {
            var ship = Ships.GetCurrentShip();

            if (ship != null)
            {
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
                ship.ShieldGenerator = null;

                ship.FrameShiftDrive = null;
                ship.OptimalMass = 0;
                ship.BoostConstant = 0;
                ship.RatingConstant = 0;
                ship.PowerConstant = 0;
                ship.GuardianFSDBooster = null;

                ship.ModuleList = new Dictionary<string, List<string>>();

                if (info.Modules != null)
                {
                    ship.Modules = info.Modules?.Select(m => m.Clone()).ToArray();

                    foreach (var m in info.Modules)
                    {
                        HandleModules(ship, m.Item, false);

                        HandleJumpRange(ship, m);
                    }
                }

                ship.HullHealth = info.HullHealth * 100.0;
            }
        }


    }
}
