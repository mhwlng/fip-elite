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
    public static class History
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
                                    else if (json?.Contains("\"event\":\"Materials\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<MaterialsEvent.MaterialsEventArgs>(json);

                                        Material.HandleMaterialsEvent(info);
                                    }
                                    else if (json?.Contains("\"event\":\"MaterialCollected\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<MaterialCollectedEvent.MaterialCollectedEventArgs>(json);

                                        Material.HandleMaterialCollectedEvent(info);
                                    }
                                    else if (json?.Contains("\"event\":\"MaterialDiscarded\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<MaterialDiscardedEvent.MaterialDiscardedEventArgs>(json);

                                        Material.HandleMaterialDiscardedEvent(info);
                                    }
                                    else if (json?.Contains("\"event\":\"ScientificResearch\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<ScientificResearchEvent.ScientificResearchEventArgs>(json);

                                        Material.HandleScientificResearchEvent(info);
                                    }
                                    else if (json?.Contains("\"event\":\"MaterialTrade\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<MaterialTradeEvent.MaterialTradeEventArgs>(json);

                                        Material.HandleMaterialTradedEvent(info);
                                    }
                                    else if (json?.Contains("\"event\":\"Synthesis\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<SynthesisEvent.SynthesisEventArgs>(json);
                                        Material.HandleSynthesisedEvent(info);
                                    }
                                    else if (json?.Contains("\"event\":\"EngineerCraft\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<EngineerCraftEvent.EngineerCraftEventArgs>(json);

                                        Material.HandleEngineerCraftEvent(info);
                                    }
                                    else if (json?.Contains("\"event\":\"TechnologyBroker\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<TechnologyBrokerEvent.TechnologyBrokerEventArgs>(json);

                                        Material.HandleTechnologyBrokerEvent(info);
                                    }
                                    else if (json?.Contains("\"event\":\"MissionCompleted\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<MissionCompletedEvent.MissionCompletedEventArgs>(json);

                                        Material.HandleMissionCompletedEvent(info);
                                    }
                                    else if (json?.Contains("\"event\":\"EngineerContribution\",") == true)
                                    {
                                        var info = JsonConvert
                                            .DeserializeObject<EngineerContributionEvent.EngineerContributionEventArgs>(json);

                                        Material.HandleEngineerContributionEvent(info);
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

