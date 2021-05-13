using System;
using System.Collections.Generic;
using System.Linq;
using EliteJournalReader.Events;
using ModuleItem = EliteJournalReader.Module;
// ReSharper disable StringLiteralTypo

namespace Elite
{
    public static class Ships
    {
       
        public static readonly Dictionary<string, string> ShipsByEliteId = new Dictionary<string, string>
        {
            {"sidewinder", "Sidewinder"},
            {"eagle", "Eagle"},
            {"hauler", "Hauler"},
            {"adder", "Adder"},
            {"viper", "Viper Mk III"},
            {"cobramkiii", "Cobra Mk III"},
            {"type6", "Type-6 Transporter"},
            {"dolphin", "Dolphin"},
            {"type7", "Type-7 Transporter"},
            {"asp", "Asp Explorer"},
            {"vulture", "Vulture"},
            {"empire_trader", "Imperial Clipper"},
            {"federation_dropship", "Federal Dropship"},
            {"orca", "Orca"},
            {"type9", "Type-9 Heavy"},
            {"type9_military", "Type-10 Defender"},
            {"python", "Python"},
            {"belugaliner", "Beluga Liner"},
            {"ferdelance", "Fer-de-Lance"},
            {"anaconda", "Anaconda"},
            {"federation_corvette", "Federal Corvette"},
            {"cutter", "Imperial Cutter"},
            {"diamondback", "Diamondback Scout"},
            {"empire_courier", "Imperial Courier"},
            {"diamondbackxl", "Diamondback Explorer"},
            {"empire_eagle", "Imperial Eagle"},
            {"federation_dropship_mkii", "Federal Assault Ship"},
            {"federation_gunship", "Federal Gunship"},
            {"viper_mkiv", "Viper Mk IV"},
            {"cobramkiv", "Cobra Mk IV"},
            {"independant_trader", "Keelback"},
            {"asp_scout", "Asp Scout"},
            {"testbuggy", "SRV"},
            {"typex", "Alliance Chieftain"},
            {"typex_2", "Alliance Crusader"},
            {"typex_3", "Alliance Challenger"},
            {"krait_mkii", "Krait Mk II"},
            {"krait_light", "Krait Phantom"},
            {"mamba", "Mamba"},
            {"empire_fighter", "Imperial Fighter"},
            {"federation_fighter", "F63 Condor"},
            {"independent_fighter", "Taipan Fighter"},
            {"gdn_hybrid_fighter_v1", "Trident"},
            {"gdn_hybrid_fighter_v2", "Javelin"},
            {"gdn_hybrid_fighter_v3", "Lance"},
            {"unknown", "Unknown"},
            {"unknown (crew)", "Unknown (crew)"},
            {"unknown (captain)", "Unknown (captain)"}
        };


        public class ShipData
        {
            public bool Stored { get; set; }
            public string ShipID { get; set; }
            public string ShipType { get; set; }
            public string ShipTypeFull
            {
                get
                {
                    ShipsByEliteId.TryGetValue(ShipType.ToLower(), out var shipTypeFull);
                    return shipTypeFull?.Trim() ?? ShipType?.Trim();
                }
            }
            public string ShipImage => ShipTypeFull?.Trim() + ".png";

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
            public ModuleItem[] Modules { get; set; }

            public string Bulkhead { get; set; }
            public string PowerPlant { get; set; }
            public string Engine { get; set; }
            public string PowerDistributor { get; set; }
            public string ShieldGenerator { get; set; }

            public string FrameShiftDrive { get; set; }
            public double OptimalMass { get; set; }
            public double BoostConstant { get; set; }
            public double RatingConstant { get; set; }
            public double PowerConstant { get; set; }
            public double MaxFuelPerJump { get; set; }
            public string GuardianFSDBooster { get; set; }

            public int EconomyClassPassengerCabinCapacity { get; set; }
            public int BusinessClassPassengerCabinCapacity { get; set; }
            public int FirstClassPassengerCabinCapacity { get; set; }
            public int LuxuryClassPassengerCabinCapacity { get; set; }
            public string Cabins
            {
                get
                {
                    var c = new List<string>();
                    if (EconomyClassPassengerCabinCapacity > 0)
                    {
                        c.Add("E=" + EconomyClassPassengerCabinCapacity);
                    }

                    if (BusinessClassPassengerCabinCapacity > 0)
                    {
                        c.Add("B=" + BusinessClassPassengerCabinCapacity);
                    }

                    if (FirstClassPassengerCabinCapacity > 0)
                    {
                        c.Add("F=" + FirstClassPassengerCabinCapacity);
                    }

                    if (LuxuryClassPassengerCabinCapacity > 0)
                    {
                        c.Add("L=" + LuxuryClassPassengerCabinCapacity);
                    }

                    return string.Join("+", c);
                }
            }
            public Dictionary<string, List<string>> ModuleList { get; set; } = new Dictionary<string, List<string>>();


            //---------------------------------------------------------

            public double CurrentFuelMain { get; set; } // filled from StatusData, only valid for current ship, not for stored ships !
            public double CurrentFuelReservoir { get; set; } // filled from StatusData, only valid for current ship, not for stored ships !
            public double CurrentCargo { get; set; } // filled from StatusData, only valid for current ship, not for stored ships !

            public double CurrentJumpRange {
                get
                {
                    // Thanks to https://github.com/EDCD/EDDI for the formula

                    if (CurrentFuelMain > 0 && UnladenMass > 0 && RatingConstant > 0 && PowerConstant > 0)
                    {
                        var massRatio = OptimalMass / (UnladenMass + CurrentFuelMain + CurrentCargo);

                        var fuel = Math.Min(CurrentFuelMain, MaxFuelPerJump);

                        return massRatio * Math.Pow(1000.0 * fuel / RatingConstant,
                            1.0 / PowerConstant) + BoostConstant;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public double FuelPercent => FuelCapacity > 0 ? Convert.ToInt32(100.0 / FuelCapacity * CurrentFuelMain) : 0;
        }

        public static List<ShipData> ShipsList = new List<ShipData>();

        public static double GetFuelCostForNextJump(double jumpDistance, double fuelMain)
        {
            // Thanks to https://forums.frontier.co.uk/threads/the-science-of-the-guardian-fsd-booster-2-0.436365/ for the formula

            /* test data
            RatingConstant = 12;
            MaxFuelPerJump = 5;
            OptimalMass = 1050;
            UnladenMass = 308.12;
            CurrentFuelMain = 1;
            CurrentCargo = 0;
            PowerConstant = 2.45;
            BoostConstant = 7.75;*/


            var shipData = GetCurrentShip();

            if (shipData != null && fuelMain > 0 && shipData.UnladenMass > 0 && shipData.RatingConstant > 0 && shipData.PowerConstant > 0)
            {
                var massRatio = shipData.OptimalMass / (shipData.UnladenMass + fuelMain + shipData.CurrentCargo);

                var maxJumpRange = massRatio * Math.Pow(1000.0 * shipData.MaxFuelPerJump / shipData.RatingConstant, 1.0 / shipData.PowerConstant);

                var boostFactor = maxJumpRange / (maxJumpRange + shipData.BoostConstant);

                var fuel = shipData.RatingConstant * 0.001 * Math.Pow(jumpDistance / massRatio * boostFactor, shipData.PowerConstant);

                return fuel;
            }
            else
            {
                return 0;
            }
        }

        public static ShipData GetCurrentShip()
        {
            return ShipsList.FirstOrDefault(x => x.Stored == false);
        }

        private static void AddShip(string shipId, string shipType, string starSystem, string stationName,
            List<double> starPos, bool stored)
        {
            if (shipType == "testbuggy") return;

            if (shipType.Contains("suit")) return;

            if (string.IsNullOrEmpty(shipType)) return;

            if (starPos == null) return;

            if (!ShipsList.Any(x => x.ShipType == shipType?.ToLower() && x.ShipID == shipId))
            {
                ShipsList.Add(new ShipData
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

        private static void RemoveShip(string shipId, string shipType)
        {
            if (shipType == "testbuggy") return;

            if (shipType.Contains("suit")) return;

            var ship = ShipsList.FirstOrDefault(x =>
                x.ShipType == shipType?.ToLower() && x.ShipID == shipId);

            if (ship != null)
            {
                ShipsList.Remove(ship);
            }
        }

        private static void SetCurrentShip(string shipId, string shipType, string starSystem, string stationName,
            List<double> starPos)
        {
            if (shipType == "testbuggy") return;

            if (shipType.Contains("suit")) return;

            if (string.IsNullOrEmpty(shipType)) return;

            if (!ShipsList.Any(x => x.ShipType == shipType?.ToLower() && x.ShipID == shipId))
            {
                AddShip(shipId, shipType, starSystem, stationName, starPos, false);
            }

            foreach (var s in ShipsList)
            {
                s.Stored = s.ShipType != shipType?.ToLower() && s.ShipID != shipId;
            }
        }

        public static void HandleShipDistance(List<double> starPos)
        {
            if (ShipsList?.Any() == true && starPos?.Count == 3)
            {
                ShipsList.ForEach(item =>
                {
                    var xs = starPos[0];
                    var ys = starPos[1];
                    var zs = starPos[2];

                    var xd = item.StarPos[0];
                    var yd = item.StarPos[1];
                    var zd = item.StarPos[2];

                    var deltaX = xs - xd;
                    var deltaY = ys - yd;
                    var deltaZ = zs - zd;

                    item.Distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                });
            }
        }

        public static void HandleShipFsdJump(string starSystem, List<double> starPos)
        {
            if (!History.VisitedSystemList.ContainsKey(starSystem))
            {
                History.VisitedSystemList.Add(starSystem, starPos);
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
                RemoveShip(info.SellShipID, info.SellOldShip.ToLower());
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
                History.VisitedSystemList.TryGetValue(info.StarSystem, out var starPos);

                if (starPos != null)
                {
                    AddShip(s.ShipID, s.ShipType.ToLower(), info.StarSystem, info.StationName, starPos, true);
                }
            }
        }

        public static void HandleShipyardSwap(ShipyardSwapEvent.ShipyardSwapEventArgs info)
        {
            Station.MarketIdStations.TryGetValue(info.MarketID, out var station);

            if (station != null)
            {
                var starPos = new List<double> {station.X, station.Y, station.Z};

                if (info.SellShipID != null && !string.IsNullOrEmpty(info.SellOldShip))
                {
                    RemoveShip(info.SellShipID, info.SellOldShip.ToLower());
                }

                if (info.StoreShipID != null && !ShipsList.Any(x =>
                    x.ShipType == info.StoreOldShip.ToLower() && x.ShipID == info.StoreShipID))
                {
                    AddShip(info.StoreShipID, info.StoreOldShip.ToLower(), station.SystemName, station.Name,
                        starPos, true);
                }

                if (!ShipsList.Any(x => x.ShipType == info.ShipType.ToLower() && x.ShipID == info.ShipID))
                {
                    AddShip(info.ShipID, info.ShipType.ToLower(), station.SystemName, station.Name, starPos,
                        false);
                }

                SetCurrentShip(info.ShipID, info.ShipType.ToLower(), station.SystemName, station.Name,
                    starPos);
            }
        }

        public static void HandleShipyardNew(ShipyardNewEvent.ShipyardNewEventArgs info)
        {
            // follows ShipyardBuy, which removes the ship so GetCurrentShip returns null

            //var ship = GetCurrentShip();
            //if (ship != null)
            //{
            AddShip(info.NewShipID, info.ShipType.ToLower(), "?", "?", new List<double>{0,0,0}, false);

                // is followed by docked (and loadout) which adds the location again
            //}
        }


        public static void HandleShipDocked(string starSystem, string stationName)
        {
            History.VisitedSystemList.TryGetValue(starSystem, out var starPos);

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


        public static void HandleLoadGame(string shipId, string shipType, string name)
        {
            var ship = GetCurrentShip();
            if (ship != null && !string.IsNullOrEmpty(shipType))
            {
                SetCurrentShip(shipId, shipType.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos);

                ship.Name = name?.Trim();
            }
        }

        public static void HandleSetUserShipName(string shipId, string name, string shipType)
        {
            var ship = GetCurrentShip();
            if (ship != null)
            {
                SetCurrentShip(shipId, shipType?.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos);

                ship.Name = name?.Trim();
            }
        }


        public static void HandleLoadout(LoadoutEvent.LoadoutEventArgs info)
        {
            var ship = GetCurrentShip();

            if (ship != null)
            {
                SetCurrentShip(info.ShipID, info.Ship?.ToLower(), ship.StarSystem, ship.StationName, ship.StarPos);

            }
        }


    }
}