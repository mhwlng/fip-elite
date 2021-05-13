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
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    MaterialList.Add(e.Name, new MaterialItem{ Category = "Encoded" ,Name = name, Count = e.Count, MaximumCapacity = GetMaximumCapacity(e.Name.ToLower()) });
                }
            }

            if (info.Manufactured?.Any() == true)
            {
                foreach (var e in info.Manufactured)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    MaterialList.Add(e.Name, new MaterialItem { Category = "Manufactured", Name = name, Count = e.Count, MaximumCapacity = GetMaximumCapacity(e.Name.ToLower()) });
                }
            }

            if (info.Raw?.Any() == true)
            {
                foreach (var e in info.Raw)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    MaterialList.Add(e.Name, new MaterialItem { Category = "Raw", Name = name, Count = e.Count, MaximumCapacity = GetMaximumCapacity(e.Name.ToLower()) });
                }
            }

        }

        public static void HandleMaterialCollectedEvent(MaterialCollectedEvent.MaterialCollectedEventArgs info)
        {
            if (MaterialList.ContainsKey(info.Name))
            {
                MaterialList[info.Name].Count += info.Count;
            }
            else
            {
                var name = (info.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.Name.ToLower())).Trim();

                MaterialList.Add(info.Name, new MaterialItem { Category = info.Category, Name = name, Count = info.Count, MaximumCapacity = GetMaximumCapacity(info.Name.ToLower()) });
            }

        }

        public static void HandleMaterialDiscardedEvent(MaterialDiscardedEvent.MaterialDiscardedEventArgs info)
        {
            if (MaterialList.ContainsKey(info.Name))
            {
                MaterialList[info.Name].Count -= info.Count;
            }
        }

        public static void HandleScientificResearchEvent(ScientificResearchEvent.ScientificResearchEventArgs info)
        {
            if (MaterialList.ContainsKey(info.Name))
            {
                MaterialList[info.Name].Count -= info.Count;
            }
        }

        public static void HandleMaterialTradedEvent(MaterialTradeEvent.MaterialTradeEventArgs info)
        {
            if (MaterialList.ContainsKey(info.Paid.Material))
            {
                MaterialList[info.Paid.Material].Count -= info.Paid.Quantity;
            }

            if (MaterialList.ContainsKey(info.Received.Material))
            {
                MaterialList[info.Received.Material].Count += info.Received.Quantity;
            }
            else
            {
                var name = (info.Received.Material_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.Received.Material.ToLower())).Trim();

                MaterialList.Add(info.Received.Material, new MaterialItem { Category = info.Received.Category_Localised ?? info.Received.Category, Name = name, Count = info.Received.Quantity, MaximumCapacity = GetMaximumCapacity(info.Received.Material.ToLower()) });
            }
        }

        public static void HandleSynthesisedEvent(SynthesisEvent.SynthesisEventArgs info)
        {
            if (info.Materials?.Any() == true)
            {
                foreach (var i in info.Materials)
                {
                    if (MaterialList.ContainsKey(i.Name))
                    {
                        MaterialList[i.Name].Count -= i.Count;
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
                    if (MaterialList.ContainsKey(i.Name))
                    {
                        MaterialList[i.Name].Count -= i.Count;
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
                    if (MaterialList.ContainsKey(i.Name))
                    {
                        MaterialList[i.Name].Count -= i.Count;
                    }
                }
            }
        }

        public static void HandleEngineerContributionEvent(EngineerContributionEvent.EngineerContributionEventArgs info)
        {

            if (info.Type == "Materials" && info.Material != null)
            {
                if (MaterialList.ContainsKey(info.Material))
                {
                    MaterialList[info.Material].Count -= info.Quantity;
                }
            }

        }

        public static void HandleMissionCompletedEvent(MissionCompletedEvent.MissionCompletedEventArgs info)
        {
            if (info.MaterialsReward?.Any() == true)
            {
                foreach (var i in info.MaterialsReward)
                {
                    if (MaterialList.ContainsKey(i.Name))
                    {
                        MaterialList[i.Name].Count += i.Count;
                    }
                    else
                    {
                        var name = (i.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(i.Name.ToLower())).Trim();

                        MaterialList.Add(i.Name, new MaterialItem { Category = i.Category_Localised ?? i.Category, Name = name, Count = i.Count, MaximumCapacity = GetMaximumCapacity(i.Name.ToLower()) });
                    }

                }

            }
        }

        public static void HandleBackPackMaterialsEvent(BackPackMaterialsEvent.BackPackMaterialsEventArgs info)
        {
            BackPackList = new Dictionary<string, MaterialItem>();

            if (info.Items?.Any() == true)
            {
                foreach (var e in info.Items)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    BackPackList.Add(e.Name, new MaterialItem { Category = "Item", Name = name, Count = e.Count });
                }
            }

            if (info.Components?.Any() == true)
            {
                foreach (var e in info.Components)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    BackPackList.Add(e.Name, new MaterialItem { Category = "Component", Name = name, Count = e.Count });
                }
            }

            if (info.Consumables?.Any() == true)
            {
                foreach (var e in info.Consumables)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    BackPackList.Add(e.Name, new MaterialItem { Category = "Consumable", Name = name, Count = e.Count });
                }
            }

            if (info.Data?.Any() == true)
            {
                foreach (var e in info.Data)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    BackPackList.Add(e.Name, new MaterialItem { Category = "Data", Name = name, Count = e.Count });
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
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    ShipLockerList.Add(e.Name, new MaterialItem { Category = "Item", Name = name, Count = e.Count });
                }
            }

            if (info.Components?.Any() == true)
            {
                foreach (var e in info.Components)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    ShipLockerList.Add(e.Name, new MaterialItem { Category = "Component", Name = name, Count = e.Count });
                }
            }

            if (info.Consumables?.Any() == true)
            {
                foreach (var e in info.Consumables)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    ShipLockerList.Add(e.Name, new MaterialItem { Category = "Consumable", Name = name, Count = e.Count});
                }
            }

            if (info.Data?.Any() == true)
            {
                foreach (var e in info.Data)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    ShipLockerList.Add(e.Name, new MaterialItem { Category = "Data", Name = name, Count = e.Count });
                }
            }

        }

        //"BuyMicroResources", "Name":"healthpack", "Name_Localised":"Medkit", "Category":"Consumable", "Count":1, "Price":1000, "MarketID":3221524992 }

        public static void HandleBuyMicroResourcesEvent(BuyMicroResourcesEvent.BuyMicroResourcesEventArgs info)
        {
            if (ShipLockerList.ContainsKey(info.Name))
            {
                ShipLockerList[info.Name].Count += info.Count;
            }
            else
            {
                var name = (info.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.Name.ToLower())).Trim();

                ShipLockerList.Add(info.Name, new MaterialItem { Category = info.Category, Name = name, Count = info.Count });
            }

        }

        public static void HandleSellMicroResourcesEvent(SellMicroResourcesEvent.SellMicroResourcesEventArgs info)
        {
            foreach (var e in info.MicroResources)
            {
                if (ShipLockerList.ContainsKey(e.Name))
                {
                    ShipLockerList[e.Name].Count -= e.Count;

                    if (ShipLockerList[e.Name].Count == 0)
                    {
                        ShipLockerList.Remove(e.Name);
                    }
                }
            }
        }

        // "Transfers":[ { "Name":"healthpack", "Name_Localised":"Medkit", "Category":"Consumable", "Count":1, "Direction":"ToShipLocker" }, { "Name":"energycell", "Name_Localised":"Energy Cell", "Category":"Consumable", "Count":1, "Direction":"ToShipLocker" } ] }
        public static void HandleTransferMicroResourcesEvent(TransferMicroResourcesEvent.TransferMicroResourcesEventArgs info)
        {
            foreach (var e in info.Transfers)
            {
                if (e.Direction == "ToShipLocker")
                {
                    if (ShipLockerList.ContainsKey(e.Name))
                    {
                        ShipLockerList[e.Name].Count += e.Count;
                    }
                    else
                    {
                        var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                        ShipLockerList.Add(e.Name, new MaterialItem { Category = e.Category, Name = name, Count = e.Count });
                    }

                    //-------------------------

                    if (BackPackList.ContainsKey(e.Name))
                    {
                        BackPackList[e.Name].Count -= e.Count;
                    }

                    if (BackPackList[e.Name].Count == 0)
                    {
                        BackPackList.Remove(e.Name);
                    }

                }
                else if (e.Direction == "ToBackpack")
                {
                    if (ShipLockerList.ContainsKey(e.Name))
                    {
                        ShipLockerList[e.Name].Count -= e.Count;
                    }

                    if (ShipLockerList[e.Name].Count == 0)
                    {
                        ShipLockerList.Remove(e.Name);
                    }

                    // -----------------------

                    if (BackPackList.ContainsKey(e.Name))
                    {
                        BackPackList[e.Name].Count += e.Count;
                    }
                    else
                    {
                        var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                        BackPackList.Add(e.Name, new MaterialItem { Category = e.Category, Name = name, Count = e.Count });
                    }

                }
            }

        }

        public static void HandleDropItemsEvent(DropItemsEvent.DropItemsEventArgs info)
        {
            if (BackPackList.ContainsKey(info.Name))
            {
                BackPackList[info.Name].Count -= info.Count;

                if (BackPackList[info.Name].Count == 0)
                {
                    BackPackList.Remove(info.Name);
                }
            }

        }

        public static void HandleCollectItemsEvent(CollectItemsEvent.CollectItemsEventArgs info)
        {
            if (BackPackList.ContainsKey(info.Name))
            {
                BackPackList[info.Name].Count += info.Count;
            }
            else
            {
                var name = (info.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.Name.ToLower())).Trim();

                BackPackList.Add(info.Name, new MaterialItem { Category = info.Type, Name = name, Count = info.Count });
            }


        }

        public static void HandleUseConsumableEvent(UseConsumableEvent.UseConsumableEventArgs info)
        {
            if (BackPackList.ContainsKey(info.Name))
            {
                BackPackList[info.Name].Count -= 1;

                if (BackPackList[info.Name].Count == 0)
                {
                    BackPackList.Remove(info.Name);
                }
            }

        }

        public static void HandleTradeMicroResourcesEvent(TradeMicroResourcesEvent.TradeMicroResourcesEventArgs info)
        {
            //????????????????????????????????
        }


    }

}



