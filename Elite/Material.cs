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

            Engineer.EngineeringMaterials.TryGetValue(name, out var entry);
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

                    MaterialList.Add(e.Name, new MaterialItem{ Category = "Encoded" ,Name = name, Count = e.Count, MaximumCapacity = GetMaximumCapacity(name)});
                }
            }

            if (info.Manufactured?.Any() == true)
            {
                foreach (var e in info.Manufactured)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    MaterialList.Add(e.Name, new MaterialItem { Category = "Manufactured", Name = name, Count = e.Count, MaximumCapacity = GetMaximumCapacity(name) });
                }
            }

            if (info.Raw?.Any() == true)
            {
                foreach (var e in info.Raw)
                {
                    var name = (e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower())).Trim();

                    MaterialList.Add(e.Name, new MaterialItem { Category = "Raw", Name = name, Count = e.Count, MaximumCapacity = GetMaximumCapacity(name) });
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

                MaterialList.Add(info.Name, new MaterialItem { Category = info.Category, Name = name, Count = info.Count, MaximumCapacity = GetMaximumCapacity(name) });
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

                MaterialList.Add(info.Received.Material, new MaterialItem { Category = info.Received.Category_Localised ?? info.Received.Category, Name = name, Count = info.Received.Quantity, MaximumCapacity = GetMaximumCapacity(name) });
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
            if (info.MaterialReward?.Any() == true)
            {
                foreach (var i in info.MaterialReward)
                {
                    if (MaterialList.ContainsKey(i.Name))
                    {
                        MaterialList[i.Name].Count += i.Count;
                    }
                    else
                    {
                        var name = (i.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(i.Name.ToLower())).Trim();

                        MaterialList.Add(i.Name, new MaterialItem { Category = i.Category_Localised ?? i.Category, Name = name, Count = i.Count, MaximumCapacity = GetMaximumCapacity(name) });
                    }

                }

            }
        }


    }

}



