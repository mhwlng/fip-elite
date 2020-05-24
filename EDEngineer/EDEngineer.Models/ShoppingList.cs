using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EDEngineer.Models.Utils;
using Newtonsoft.Json;

namespace EDEngineer.Models
{
    public class IngredientShoppingListItem
    {
        public string Name { get; set; }
        public int RequiredCount { get; set; }
        public int Inventory { get; set; }
        public EntryData EntryData { get; set; }

        public List<string> BestSystems { get; set; } = new List<string>();
    }

    public class BlueprintShoppingListItem
    {
        [JsonProperty("Blueprint")]

        public BlueprintDetail Blueprint { get; set; }
        public int Count { get; set; }

        public Blueprint BluePrintData { get; set; }
    }

    public class BlueprintDetail
    {
        public string BlueprintName { get; set; }
        public string Type { get; set; }
        public int? Grade { get; set; }
    }

}