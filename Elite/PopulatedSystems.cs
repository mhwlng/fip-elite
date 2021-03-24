using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

// ReSharper disable IdentifierTypo

namespace Elite
{

    public static class PopulatedSystems
    {
        public static Dictionary<string, PopulatedSystem> SystemList = null;

        public class State
        {

            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class PopulatedSystem
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("states")]
            public IList<State> States { get; set; }
        }

        public static List<PopulatedSystem> GetAllPopupulatedSystems(string path)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<List<PopulatedSystem>>(File.ReadAllText(path));
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new List<PopulatedSystem>();
        }


        public static string GetSystemState(string name)
        {
            SystemList.TryGetValue(name, out var value);

            if (value != null && value.States?.Any() == true)
            {
                return string.Join(",", value.States.Select(x => x.Name));
            }

            return "";

        }


    }
}
