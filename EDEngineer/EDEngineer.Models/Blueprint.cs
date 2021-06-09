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
    public class Blueprint
    {
        private static readonly Dictionary<string, string> technicalTypes = new Dictionary<string, string>
        {
            ["Frame Shift Drive"] = "hyperdrive",
            ["Heat Sink Launcher"] = "heatsinklauncher",
            ["Collector Limpet Controller"] = "dronecontrol_collection",
            ["Prospector Limpet Controller"] = "dronecontrol_prospector",
            ["Fuel Transfer Limpet Controller"] = "dronecontrol_fuel",
            ["Hatch Breaker Limpet Controller"] = "dronecontrol_resourcesiphon",
            ["Thrusters"] = "engine",
            ["Pulse Laser"] = "pulselaser",
            ["Beam Laser"] = "beamlaser",
            ["Shield Booster"] = "shieldbooster",
            ["Shield Generator"] = "shieldgenerator",
            ["Sensors"] = "sensors",
            ["Plasma Accelerator"] = "plasmaaccelerator",
            ["Life Support"] = "lifesupport",
            ["Hull Reinforcement Package"] = "hullreinforcement",
            ["Power Distributor"] = "powerdistributor",
            ["Power Plant"] = "powerplant",
            ["Multi-cannon"] = "multicannon",
            ["Kill Warrant Scanner"] = "crimescanner",
            ["Rail Gun"] = "railgun",
            ["Burst Laser"] = "pulselaserburst"
        };

        public string Type { get; }

        [JsonIgnore]
        public string TechnicalType => technicalTypes.TryGetValue(Type, out var result) ? result : Type;

        public string ShortenedType
        {
            get
            {
                switch (Type)
                {
                    case "Electronic Countermeasure":
                        return "ECM";
                    case "Hull Reinforcement Package":
                        return "Hull";
                    case "Frame Shift Drive Interdictor":
                        return "FSD Interdictor";
                    case "Prospector Limpet Controller":
                        return "Prospector LC";
                    case "Fuel Transfer Limpet Controller":
                        return "Fuel Transfer LC";
                    case "Hatch Breaker Limpet Controller":
                        return "Hatch Breaker LC";
                    case "Collector Limpet Controller":
                        return "Collector LC";
                    case "Auto Field-Maintenance Unit":
                        return "AFMU";
                    default:
                        return Type;
                }
            }
        }
        public string BlueprintName { get; }
        public IReadOnlyCollection<string> Engineers { get; }
        public IReadOnlyCollection<BlueprintIngredient> Ingredients { get; }
        public int? Grade { get; }
        public IReadOnlyCollection<BlueprintEffect> Effects { get; }
        public Guid? CoriolisGuid { get; set; }


        [JsonIgnore]
        public BlueprintCategory Category
        {
            get
            {
                if (Engineers.FirstOrDefault() == "@Synthesis")
                {
                    return BlueprintCategory.Synthesis;
                }

                if (Type == "Unlock")
                {
                    return BlueprintCategory.Unlock;
                }

                if(Engineers.FirstOrDefault() == "@Merchant")
                {
                    return BlueprintCategory.Merchant;
                }

                if (Engineers.FirstOrDefault() == "@Technology")
                {
                    return BlueprintCategory.Technology;
                }

                if (Grade == null)
                {
                    return BlueprintCategory.Experimental;
                }

                return BlueprintCategory.Module;
            }
        }

        public Blueprint(
            string type,
            string blueprintName,
            int? grade,
            IReadOnlyCollection<BlueprintIngredient> ingredients,
            IReadOnlyCollection<string> engineers,
            IReadOnlyCollection<BlueprintEffect> effects,
            Guid? coriolisGuid)
        {
            Type = type;
            BlueprintName = blueprintName;
            Grade = grade;
            Engineers = engineers;
            Ingredients = ingredients;
            Effects = effects;
            CoriolisGuid = coriolisGuid;

        }

        private int ComputeCraftCount(Func<BlueprintIngredient, int> countExtractor)
        {
            return Ingredients.Any() ? Ingredients.Min(i => countExtractor(i)/i.Size) : 0;
        }

        private double ComputeProgress(Func<BlueprintIngredient, int> countExtractor)
        {
            return Ingredients.Sum(
                ingredient =>
                    Math.Min(1, countExtractor(ingredient)/(double) ingredient.Size)/Ingredients.Count)*100;
        }

        public override string ToString()
        {
            return $"G{Grade} [{Type}] {BlueprintName}";
        }

        public string GradeString => Grade != null ? $"G{Grade}" : "";

        private string Prefix
        {
            get
            {
                switch (Category)
                {
                    case BlueprintCategory.Synthesis:
                        return $"SYN ";
                    case BlueprintCategory.Experimental:
                        return $"EXP ";
                    case BlueprintCategory.Technology:
                        return $"TEC ";
                    case BlueprintCategory.Unlock:
                        return $"ULK ";
                    default:
                        return $"";
                }
            }
        }


        public object ToSerializable()
        {
            return new
            {
                BlueprintName,
                Type,
                Grade
            };
        }

        public bool HasSameIngredients(IReadOnlyCollection<BlueprintIngredient> ingredients)
        {
            return ingredients.Count == Ingredients.Count && ingredients.All(Ingredients.Contains);
        }
    }
}