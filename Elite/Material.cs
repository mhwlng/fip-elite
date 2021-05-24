using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using EliteJournalReader.Events;

namespace Elite
{
    public static class Material
    {

        public class MaterialItem
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public int Count { get; set; }
            public int MaximumCapacity { get; set; }

            public string MissionID { get; set; }

            public string MissionName { get; set; } // only filled in FipPanel.cs
            public string System { get; set; } // only filled in FipPanel.cs
            public string Station { get; set; } // only filled in FipPanel.cs

        }

        public class MaterialHistoryItem
        {
            public string Name { get; set; }

            public string System { get; set; }
            public int Count { get; set; }
        }

        public static Dictionary<string, MaterialItem> ShipLockerList = new Dictionary<string, MaterialItem>();

        public static Dictionary<string, MaterialItem> BackPackList = new Dictionary<string, MaterialItem>();

        public static Dictionary<string, MaterialItem> MaterialList = new Dictionary<string, MaterialItem>();

        public static Dictionary<string, Dictionary<string,MaterialHistoryItem>> MaterialHistoryList = new Dictionary<string, Dictionary<string,MaterialHistoryItem>>();

        public static void AddHistory(string name, string system, int count)
        {
            MaterialHistoryList.TryGetValue(name, out var materialData);
            if (materialData == null)
            {
                var m = new Dictionary<string, MaterialHistoryItem>();
                var mhi = new MaterialHistoryItem
                {
                    Name = name, 
                    System = system, 
                    Count = count
                };
                m.Add(system, mhi);
                MaterialHistoryList.Add(name, m);
            }
            else
            {
                materialData.TryGetValue(system, out var systemData);
                if (systemData == null)
                {
                    var mhi = new MaterialHistoryItem
                    {
                        Name = name,
                        System = system,
                        Count = count
                    };
                    materialData.Add(system, mhi);
                }
                else
                {
                    systemData.Count += count;
                }
            }
        }

        private static int GetMaximumCapacity(string name)
        {
            var maximumCapacity = 0;

            Engineer.EngineeringMaterialsByKey.TryGetValue(name, out var entry);
            if (entry != null)
            {
                maximumCapacity = entry.MaximumCapacity;
            }

            return maximumCapacity;
        }

        public static void HandleMaterialsEvent(MaterialsEvent.MaterialsEventArgs info)
        {
            MaterialList = new Dictionary<string, MaterialItem>();

            if (info.Encoded?.Any() == true)
            {
                foreach (var e in info.Encoded)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    MaterialList.Add(idxName, new MaterialItem{ Category = "Encoded" ,Name = name, Count = e.Count, MaximumCapacity = GetMaximumCapacity(idxName) });
                }
            }

            if (info.Manufactured?.Any() == true)
            {
                foreach (var e in info.Manufactured)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    MaterialList.Add(idxName, new MaterialItem { Category = "Manufactured", Name = name, Count = e.Count, MaximumCapacity = GetMaximumCapacity(idxName) });
                }
            }

            if (info.Raw?.Any() == true)
            {
                foreach (var e in info.Raw)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    MaterialList.Add(idxName, new MaterialItem { Category = "Raw", Name = name, Count = e.Count, MaximumCapacity = GetMaximumCapacity(idxName) });
                }
            }

        }

        public static void HandleMaterialCollectedEvent(MaterialCollectedEvent.MaterialCollectedEventArgs info)
        {
            var idxName = info.Name.ToLower();

            if (MaterialList.ContainsKey(idxName))
            {
                MaterialList[idxName].Count += info.Count;
            }
            else
            {
                var name = (info.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                MaterialList.Add(idxName, new MaterialItem { Category = info.Category, Name = name, Count = info.Count, MaximumCapacity = GetMaximumCapacity(idxName) });
            }

        }

        public static void HandleMaterialDiscardedEvent(MaterialDiscardedEvent.MaterialDiscardedEventArgs info)
        {
            var idxName = info.Name.ToLower();

            if (MaterialList.ContainsKey(idxName))
            {
                MaterialList[idxName].Count -= info.Count;
            }
        }

        public static void HandleScientificResearchEvent(ScientificResearchEvent.ScientificResearchEventArgs info)
        {
            var idxName = info.Name.ToLower();

            if (MaterialList.ContainsKey(idxName))
            {
                MaterialList[idxName].Count -= info.Count;
            }
        }

        public static void HandleMaterialTradedEvent(MaterialTradeEvent.MaterialTradeEventArgs info)
        {
            var idxPaidName = info.Paid.Material.ToLower();

            if (MaterialList.ContainsKey(idxPaidName))
            {
                MaterialList[idxPaidName].Count -= info.Paid.Quantity;
            }

            var idxRecName = info.Received.Material.ToLower();

            if (MaterialList.ContainsKey(idxRecName))
            {
                MaterialList[idxRecName].Count += info.Received.Quantity;
            }
            else
            {
                var name = (info.Received.Material_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxRecName)).Trim();

                MaterialList.Add(idxRecName, new MaterialItem { Category = info.Received.Category_Localised ?? info.Received.Category, Name = name, Count = info.Received.Quantity, MaximumCapacity = GetMaximumCapacity(idxRecName) });
            }
        }

        public static void HandleSynthesisedEvent(SynthesisEvent.SynthesisEventArgs info)
        {
            if (info.Materials?.Any() == true)
            {
                foreach (var i in info.Materials)
                {
                    var idxName = i.Name.ToLower();

                    if (MaterialList.ContainsKey(idxName))
                    {
                        MaterialList[idxName].Count -= i.Count;
                    }
                }
            }
        }

        public static void HandleEngineerCraftEvent(EngineerCraftEvent.EngineerCraftEventArgs info)
        {

            if (info.Ingredients?.Any() == true)
            {
                foreach (var i in info.Ingredients)
                {
                    var idxName = i.Name.ToLower();

                    if (MaterialList.ContainsKey(idxName))
                    {
                        MaterialList[idxName].Count -= i.Count;
                    }
                }
            }
        }

        public static void HandleTechnologyBrokerEvent(TechnologyBrokerEvent.TechnologyBrokerEventArgs info)
        {
            if (info.Materials?.Any() == true)
            {
                foreach (var i in info.Materials)
                {
                    var idxName = i.Name.ToLower();

                    if (MaterialList.ContainsKey(idxName))
                    {
                        MaterialList[idxName].Count -= i.Count;
                    }
                }
            }
        }

        public static void HandleEngineerContributionEvent(EngineerContributionEvent.EngineerContributionEventArgs info)
        {

            if (info.Type == "Materials" && info.Material != null)
            {
                var idxName = info.Material.ToLower();

                if (MaterialList.ContainsKey(idxName))
                {
                    MaterialList[idxName].Count -= info.Quantity;
                }
            }

        }

        //{ "timestamp":"2021-05-21T14:32:02Z", "event":"MissionCompleted", "Faction":"Future of Arro Naga", "Name":"Mission_OnFoot_Collect_MB_name", "MissionID":770352328,
        //"Commodity":"$PersonalDocuments_Name;", "Commodity_Localised":"Personal Documents", "Count":1,
        //"Reward":36674,
        //"MaterialsReward":[ { "Name":"MiningAnalytics", "Name_Localised":"Mining Analytics", "Category":"$MICRORESOURCE_CATEGORY_Data;", "Category_Localised":"Data", "Count":2 } ],
        //"FactionEffects":[ { "Faction":"Future of Arro Naga", "Effects":[  ], "Influence":[ { "SystemAddress":3932277478106, "Trend":"UpGood", "Influence":"+" } ], "ReputationTrend":"UpGood", "Reputation":"+" } ] }


        public static void HandleMissionCompletedEvent(MissionCompletedEvent.MissionCompletedEventArgs info)
        {
            if (!string.IsNullOrEmpty(info.Commodity_Localised) && ShipLockerList.Any(x => x.Value?.Name == info.Commodity_Localised))
            {
                var key = ShipLockerList.FirstOrDefault(x => x.Value.Name == info.Commodity_Localised).Key;

                ShipLockerList[key].Count -= info.Count ?? 0;

                if (ShipLockerList[key].Count <= 0)
                {
                    ShipLockerList.Remove(key);
                }
            }

            if (info.MaterialsReward?.Any() == true)
            {
                foreach (var i in info.MaterialsReward)
                {
                    var idxName = i.Name.ToLower();

                    if (i.Category.StartsWith("$MICRORESOURCE_CATEGORY_"))
                    {
                        var category = i.Category.Replace("$MICRORESOURCE_CATEGORY_", "").Replace(";", ""); ;

                        if (ShipLockerList.ContainsKey(idxName))
                        {
                            ShipLockerList[idxName].Count += i.Count;
                        }
                        else
                        {
                            var name = (i.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                            ShipLockerList.Add(idxName, new MaterialItem { Category = category, Name = name, Count = i.Count, MissionID = null });
                        }

                    }
                    else
                    {
                        if (MaterialList.ContainsKey(idxName))
                        {
                            MaterialList[idxName].Count += i.Count;
                        }
                        else
                        {
                            var name = (i.Name_Localised ??
                                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();
                            MaterialList.Add(i.Name,
                                new MaterialItem
                                {
                                    Category = i.Category_Localised ?? i.Category, Name = name, Count = i.Count,
                                    MaximumCapacity = GetMaximumCapacity(idxName)
                                });
                        }
                    }

                }

            }
        }

        public static void HandleBackPackEvent(BackPackEvent.BackPackEventArgs info)
        {
            BackPackList = new Dictionary<string, MaterialItem>();

            if (info.Items?.Any() == true)
            {
                foreach (var e in info.Items)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    BackPackList.Add(idxName, new MaterialItem { Category = "Item", Name = name, Count = e.Count, MissionID = e.MissionID });
                }
            }

            if (info.Components?.Any() == true)
            {
                foreach (var e in info.Components)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    BackPackList.Add(idxName, new MaterialItem { Category = "Component", Name = name, Count = e.Count, MissionID = e.MissionID });
                }
            }

            if (info.Consumables?.Any() == true)
            {
                foreach (var e in info.Consumables)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    BackPackList.Add(idxName, new MaterialItem { Category = "Consumable", Name = name, Count = e.Count, MissionID = e.MissionID });
                }
            }

            if (info.Data?.Any() == true)
            {
                foreach (var e in info.Data)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    BackPackList.Add(idxName, new MaterialItem { Category = "Data", Name = name, Count = e.Count, MissionID = e.MissionID });
                }
            }

        }

        public static void HandleShipLockerMaterialsEvent(ShipLockerMaterialsEvent.ShipLockerMaterialsEventArgs info)
        {
            ShipLockerList = new Dictionary<string, MaterialItem>();

            if (info.Items?.Any() == true)
            {
                foreach (var e in info.Items)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    ShipLockerList.Add(idxName, new MaterialItem { Category = "Item", Name = name, Count = e.Count, MissionID = e.MissionID});
                }
            }

            if (info.Components?.Any() == true)
            {
                foreach (var e in info.Components)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    ShipLockerList.Add(idxName, new MaterialItem { Category = "Component", Name = name, Count = e.Count, MissionID = e.MissionID });
                }
            }

            if (info.Consumables?.Any() == true)
            {
                foreach (var e in info.Consumables)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    ShipLockerList.Add(idxName, new MaterialItem { Category = "Consumable", Name = name, Count = e.Count, MissionID = e.MissionID });
                }
            }

            if (info.Data?.Any() == true)
            {
                foreach (var e in info.Data)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    ShipLockerList.Add(idxName, new MaterialItem { Category = "Data", Name = name, Count = e.Count, MissionID = e.MissionID });
                }
            }

        }

        //"BuyMicroResources", "Name":"healthpack", "Name_Localised":"Medkit", "Category":"Consumable", "Count":1, "Price":1000, "MarketID":3221524992 }

        public static void HandleBuyMicroResourcesEvent(BuyMicroResourcesEvent.BuyMicroResourcesEventArgs info)
        {
            var idxName = info.Name.ToLower();

            if (ShipLockerList.ContainsKey(idxName))
            {
                ShipLockerList[idxName].Count += info.Count;
            }
            else
            {
                var name = (info.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                ShipLockerList.Add(idxName, new MaterialItem { Category = info.Category, Name = name, Count = info.Count, MissionID = null });
            }

        }

        public static void HandleSellMicroResourcesEvent(SellMicroResourcesEvent.SellMicroResourcesEventArgs info)
        {
            foreach (var e in info.MicroResources)
            {
                var idxName = e.Name.ToLower();

                if (ShipLockerList.ContainsKey(idxName))
                {
                    ShipLockerList[idxName].Count -= e.Count;

                    if (ShipLockerList[idxName].Count <= 0)
                    {
                        ShipLockerList.Remove(idxName);
                    }
                }
            }
        }

        public static void HandleTradeMicroResourcesEvent(TradeMicroResourcesEvent.TradeMicroResourcesEventArgs info)
        {
            foreach (var e in info.Offered)
            {
                var idxName = e.Name.ToLower();

                if (ShipLockerList.ContainsKey(idxName))
                {
                    ShipLockerList[idxName].Count -= e.Count;

                    if (ShipLockerList[idxName].Count <= 0)
                    {
                        ShipLockerList.Remove(idxName);
                    }
                }
            }

            var idxRecName = info.Received.ToLower();

            //????????????????var name = (info.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxRecName)).Trim();

            ShipLockerList.Add(idxRecName, new MaterialItem { Category = info.Category, Name = info.Received, Count = info.Count, MissionID = null });

            //???????????????

        }


        // "Transfers":[ { "Name":"healthpack", "Name_Localised":"Medkit", "Category":"Consumable", "Count":1, "Direction":"ToShipLocker" }, { "Name":"energycell", "Name_Localised":"Energy Cell", "Category":"Consumable", "Count":1, "Direction":"ToShipLocker" } ] }
        public static void HandleTransferMicroResourcesEvent(TransferMicroResourcesEvent.TransferMicroResourcesEventArgs info)
        {
            foreach (var e in info.Transfers)
            {
                var idxName = e.Name.ToLower();

                if (e.Direction == "ToShipLocker")
                {
                    if (ShipLockerList.ContainsKey(idxName))
                    {
                        ShipLockerList[idxName].Count += e.Count;
                    }
                    else
                    {
                        var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                        string missionID = null;

                        if (!string.IsNullOrEmpty(name) && BackPackList.Any(x => x.Value?.Name == name))
                        {
                            var key = BackPackList.FirstOrDefault(x => x.Value.Name == name).Key;

                            missionID = BackPackList[key].MissionID;
                        }
                        
                        ShipLockerList.Add(idxName, new MaterialItem { Category = e.Category, Name = name, Count = e.Count, MissionID = missionID });
                    }

                    //-------------------------

                    // not handled by backpack.json !!!!!!!!!!!!!!!

                    if (BackPackList.ContainsKey(idxName))
                    {
                        BackPackList[idxName].Count -= e.Count;

                        if (BackPackList[idxName].Count <= 0)
                        {
                            BackPackList.Remove(idxName);
                        }
                    } 

                }
                else if (e.Direction == "ToBackpack")
                {
                    if (ShipLockerList.ContainsKey(idxName))
                    {
                        ShipLockerList[idxName].Count -= e.Count;

                        if (ShipLockerList[idxName].Count <= 0)
                        {
                            ShipLockerList.Remove(idxName);
                        }
                    }

                    // -----------------------

                    // not needed, handled by backpack.json ?????????

                    /*

                    if (BackPackList.ContainsKey(idxName))
                    {
                        BackPackList[idxName].Count += e.Count;
                    }
                    else
                    {
                        var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                        string missionID = null;

                        if (!string.IsNullOrEmpty(name) && ShipLockerList.Any(x => x.Value?.Name == name))
                        {
                            var key = ShipLockerList.FirstOrDefault(x => x.Value.Name == name).Key;

                            missionID = ShipLockerList[key].MissionID;
                        }

                        BackPackList.Add(idxName, new MaterialItem { Category = e.Category, Name = name, Count = e.Count, MissionID = missionID });
                    } */

                }
            }

        }

        /* already handled in backpack.json
        public static void HandleBackPackChangeEvent(BackPackChangeEvent.BackPackChangeEventArgs info)
        {
            if (info.Added?.Any() == true)
            {
                foreach (var e in info.Added)
                {
                   var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    if (BackPackList.ContainsKey(idxName))
                    {
                        BackPackList[idxName].Count += e.Count;
                    }
                    else
                    {
                        BackPackList.Add(idxName, new MaterialItem { Category = e.Type, Name = name, Count = e.Count });
                    }

                }
            }

            if (info.Removed?.Any() == true)
            {
                foreach (var e in info.Removed)
                {
                    var idxName = e.Name.ToLower();

                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                    if (BackPackList.ContainsKey(idxName))
                    {
                        BackPackList[idxName].Count -= e.Count;

                        if (BackPackList[idxName].Count <= 0)
                        {
                            BackPackList.Remove(idxName);
                        }
                    }
                }
            }

        } 

        public static void HandleDropItemsEvent(DropItemsEvent.DropItemsEventArgs info)
        {
            var idxName = info.Name.ToLower();

            if (BackPackList.ContainsKey(idxName))
            {
                BackPackList[idxName].Count -= info.Count;

                if (BackPackList[idxName].Count == 0)
                {
                    BackPackList.Remove(idxName);
                }
            }

        }

        public static void HandleCollectItemsEvent(CollectItemsEvent.CollectItemsEventArgs info)
        {
            var idxName = info.Name.ToLower();

            if (BackPackList.ContainsKey(idxName))
            {
                BackPackList[idxName].Count += info.Count;
            }
            else
            {
                var name = (info.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(idxName)).Trim();

                BackPackList.Add(idxName, new MaterialItem { Category = info.Type, Name = name, Count = info.Count, MissionID = null });
            }
        }

        public static void HandleUseConsumableEvent(UseConsumableEvent.UseConsumableEventArgs info)
        {
            var idxName = info.Name.ToLower();

            if (BackPackList.ContainsKey(idxName))
            {
                BackPackList[idxName].Count -= 1;

                if (BackPackList[idxName].Count == 0)
                {
                    BackPackList.Remove(idxName);
                }
            }

        } */



    }

}



