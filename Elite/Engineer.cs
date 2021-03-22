using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EDEngineer.Models;
using EDEngineer.Utils;
using Newtonsoft.Json;

namespace Elite
{
    public static class Engineer
    {
        public static Dictionary<(string, string, int?), Blueprint> Blueprints;

        public static Dictionary<string, List<Blueprint>> EngineerBlueprints;

        public static Dictionary<string,EntryData> EngineeringMaterials;

        public static Dictionary<string, EntryData> EngineeringMaterialsByKey;

        public static string CommanderName;

        public static List<BlueprintShoppingListItem> BlueprintShoppingList = new List<BlueprintShoppingListItem>();

        public static List<IngredientShoppingListItem> IngredientShoppingList = new List<IngredientShoppingListItem>();

        public static (Dictionary<string,EntryData>,Dictionary<string, EntryData>) GetAllEngineeringMaterials(string path)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    var json = JsonConvert.DeserializeObject<List<EntryData>>(File.ReadAllText(path));

                    return (json.ToDictionary(x => x.Name, x => x),
                            json.ToDictionary(x => x.FormattedName.ToLower(), x => x));
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return (new Dictionary<string, EntryData>(), new Dictionary<string, EntryData>());
        }

        public static Dictionary<(string, string, int?), Blueprint> GetAllBlueprints(string path, Dictionary<string, EntryData> engineeringMaterials)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    var blueprintConverter = new BlueprintConverter(engineeringMaterials);

                    return JsonConvert.DeserializeObject<List<Blueprint>>(File.ReadAllText(path), blueprintConverter)
                        .Where(b => b.Ingredients.Any())
                        .ToDictionary(x => (x.BlueprintName, x.Type, x.Grade), x => x);
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new Dictionary<(string, string, int?), Blueprint>();
          

        }

        public static Dictionary<string, List<Blueprint>> GetEngineerBlueprints(string path, Dictionary<string, EntryData> engineeringMaterials)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {

                    var blueprintConverter = new BlueprintConverter(engineeringMaterials);

                    var blueprints = JsonConvert
                        .DeserializeObject<List<Blueprint>>(File.ReadAllText(path), blueprintConverter)
                        .Where(b => b.Ingredients.Any()).ToList();


                    var engineerBlueprints = blueprints
                        .Where(x => x.Category !=  BlueprintCategory.Experimental && x.Category != BlueprintCategory.Unlock)
                        .SelectMany(blueprint => blueprint.Engineers, (blueprint, engineer) => new {blueprint, engineer})
                        .Where(x => !x.engineer.StartsWith("@"))
                        .GroupBy(x => x.engineer)
                        .ToDictionary(x => x.Key, 
                            x => x.Select( z => (Blueprint)z.blueprint)
                                .GroupBy(a => a.Type).Select(b => b.First(c => c.Grade == b.Max(d => d.Grade)))
                                .OrderByDescending(e => e.Grade).ThenBy(e => e.Type).ToList())
                        ;

                    return engineerBlueprints;

                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new Dictionary<string, List<Blueprint>>();


        }

        private static string GetJson(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadString(url);
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public static void GetCommanderName()
        {
            var commanderData  = GetJson("http://localhost:44405/commanders");

            if (string.IsNullOrEmpty(commanderData)) return;

            CommanderName = JsonConvert.DeserializeObject<List<string>>(commanderData)
                .FirstOrDefault();
        }

        public static void RefreshMaterialList()
        {
            if (IngredientShoppingList?.Any() == true && Material.MaterialList?.Any() == true)
            {
                foreach (var i in IngredientShoppingList)
                {
                    var materialData = Material.MaterialList.FirstOrDefault(x => x.Value.Name == i.Name).Value;

                    i.Inventory = materialData?.Count ?? 0;
                }
            }
        }

        public static void GetBestSystems()
        {
            if (string.IsNullOrEmpty(CommanderName)) return;

            if (IngredientShoppingList?.Any() == true)
            {
                foreach (var i in IngredientShoppingList)
                {
                    Material.MaterialHistoryList.TryGetValue(i.EntryData.Name, out var materialHistoryData);

                    if (materialHistoryData != null)
                    {
                        i.BestSystems = materialHistoryData.Values
                            .OrderByDescending(x => x.Count)
                            .Select(x => x.System + " [" + x.Count + "]")
                            .Take(5)
                            .ToList();
                    }
                }
            }
        }

        public static void GetShoppingList()
        {
            if (string.IsNullOrEmpty(CommanderName)) return;

            var shoppingListData = GetJson("http://localhost:44405/" + CommanderName + "/shopping-list");

            if (string.IsNullOrEmpty(shoppingListData)) return;

            var bluePrintList =
                JsonConvert.DeserializeObject<List<BlueprintShoppingListItem>>(shoppingListData);

            foreach (var item in bluePrintList)
            {
                Blueprints.TryGetValue(
                    (item.Blueprint.BlueprintName, item.Blueprint.Type, item.Blueprint.Grade),
                    out var bluePrintData);

                item.BluePrintData = bluePrintData;
            }

            BlueprintShoppingList = bluePrintList;

            IngredientShoppingList = bluePrintList.Select(
                    x => new
                    {
                        x.Blueprint.BlueprintName, x.Blueprint.Type, x.Blueprint.Grade,
                        Ingredients = x.BluePrintData.Ingredients.Select(y => new
                            BlueprintIngredient(y.EntryData, y.Size * x.Count))
                    })

                .SelectMany(x => x.Ingredients)
                .GroupBy(x => x.EntryData.Name)
                .Select(x => new IngredientShoppingListItem
                {
                    Name = x.Key,
                    RequiredCount = x.Sum(y => y.Size),
                    EntryData = x.First().EntryData

                }).ToList();
        }

    }
}
