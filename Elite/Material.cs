using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        public static Dictionary<string, MaterialItem> MaterialList = new Dictionary<string, MaterialItem>();

        public static void HandleMaterialsEvent(MaterialsEvent.MaterialsEventArgs info)
        {
            MaterialList = new Dictionary<string, MaterialItem>();

            if (info.Encoded?.Any() == true)
            {
                foreach (var e in info.Encoded)
                {
                    MaterialList.Add(e.Name, new MaterialItem{ Category = "Encoded" ,Name = e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower()), Count = e.Count});
                }
            }

            if (info.Manufactured?.Any() == true)
            {
                foreach (var e in info.Manufactured)
                {
                    MaterialList.Add(e.Name, new MaterialItem { Category = "Manufactured", Name = e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower()), Count = e.Count });
                }
            }

            if (info.Raw?.Any() == true)
            {
                foreach (var e in info.Raw)
                {
                    MaterialList.Add(e.Name, new MaterialItem { Category = "Raw", Name = e.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e.Name.ToLower()), Count = e.Count });
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
                MaterialList.Add(info.Name, new MaterialItem { Category = info.Category, Name = info.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.Name.ToLower()), Count = info.Count });
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
                MaterialList.Add(info.Received.Material, new MaterialItem { Category = info.Received.Category_Localised ?? info.Received.Category, Name = info.Received.Material_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.Received.Material.ToLower()), Count = info.Received.Quantity });
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
                        MaterialList.Add(i.Name, new MaterialItem { Category = i.Category_Localised ?? i.Category, Name = i.Name_Localised ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(i.Name.ToLower()), Count = i.Count });
                    }

                }

            }
        }


    }

}



