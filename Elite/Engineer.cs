using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static List<Blueprint> BlueprintList;

        public static Dictionary<string,EntryData> EngineeringMaterials;

        public static Dictionary<string,EntryData> GetAllEngineeringMaterials(string path)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<List<EntryData>>(File.ReadAllText(path))
                        .ToDictionary(x => x.Name, x => x);
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new Dictionary<string, EntryData>();
        }

        public static List<Blueprint> GetAllBlueprints(string path, Dictionary<string, EntryData> engineeringMaterials)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    var blueprintConverter = new BlueprintConverter(engineeringMaterials);

                    return JsonConvert.DeserializeObject<List<Blueprint>>(File.ReadAllText(path), blueprintConverter)
                        .Where(b => b.Ingredients.Any())
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new List<Blueprint>();
          

        }

    }
}
