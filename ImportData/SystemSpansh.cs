using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ImportData
{
    public class SystemSpansh
    {
        [JsonProperty("id64")]
        public long Id64 { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("coords")]
        public SystemSpanshCoords Coords { get; set; }
        [JsonProperty("allegiance")]
        public string Allegiance { get; set; }
        [JsonProperty("government")]
        public string Government { get; set; }
        [JsonProperty("primaryEconomy")]
        public string PrimaryEconomy { get; set; }
        [JsonProperty("secondaryEconomy")]
        public string SecondaryEconomy { get; set; }
        [JsonProperty("security")]
        public string Security { get; set; }
        [JsonProperty("population")]
        public long Population { get; set; }
        //[JsonProperty("bodyCount")]
        //public int BodyCount { get; set; }
        [JsonProperty("controllingFaction")]
        public SystemSpanshControllingFaction ControllingFaction { get; set; }
        [JsonProperty("factions")]
        public List<SystemSpanshFaction> Factions { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        //[JsonProperty("bodies")]
        //public List<BodySpansh> Bodies { get; set; }
        [JsonProperty("stations")]
        public List<SystemSpanshStation> Stations { get; set; }
    }

    public class SystemSpanshCoords
    {
        [JsonProperty("x")]
        public float X { get; set; }
        [JsonProperty("y")]
        public float Y { get; set; }
        [JsonProperty("z")]
        public float Z { get; set; }
    }

    public class SystemSpanshControllingFaction
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class SystemSpanshFaction
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("allegiance")]
        public string Allegiance { get; set; }
        [JsonProperty("government")]
        public string Government { get; set; }
        [JsonProperty("influence")]
        public float Influence { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
    }
    /*
    public class BodySpansh
    {
        public long id64 { get; set; }
        public long bodyId { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string subType { get; set; }
        public float distanceToArrival { get; set; }
        public bool mainStar { get; set; }
        public int age { get; set; }
        public string spectralClass { get; set; }
        public string luminosity { get; set; }
        public float absoluteMagnitude { get; set; }
        public float solarMasses { get; set; }
        public float solarRadius { get; set; }
        public float surfaceTemperature { get; set; }
        public float rotationalPeriod { get; set; }
        public float axialTilt { get; set; }
        public List<SystemSpanshBelt> belts { get; set; }
        public List<SystemSpanshStation> stations { get; set; }
        public string updateTime { get; set; }
        public float orbitalPeriod { get; set; }
        public float semiMajorAxis { get; set; }
        public float orbitalEccentricity { get; set; }
        public float orbitalInclination { get; set; }
        public float argOfPeriapsis { get; set; }
        public bool rotationalPeriodTidallyLocked { get; set; }
        public bool isLandable { get; set; }
        public float gravity { get; set; }
        public float earthMasses { get; set; }
        public float radius { get; set; }
        public float surfacePressure { get; set; }
        public string volcanismType { get; set; }
        public string atmosphereType { get; set; }
        public List<SystemSpanshParent> parents { get; set; }
        public SystemSpanshSolidcomposition solidComposition { get; set; }
        public string terraformingState { get; set; }
        public SystemSpanshMaterials Materials { get; set; }
        public SystemSpanshSignals Signals { get; set; }
        public SystemSpanshAtmospherecomposition atmosphereComposition { get; set; }
        public string reserveLevel { get; set; }
        public List<SystemSpanshRing> rings { get; set; }
    }

    public class SystemSpanshParent
    {
        public long Star { get; set; }
        public long Null { get; set; }
        public long Planet { get; set; }
    }

    public class SystemSpanshRing
    {
        public string name { get; set; }
        public string type { get; set; }
        public float mass { get; set; }
        public float innerRadius { get; set; }
        public float outerRadius { get; set; }
        public SystemSpanshSignals Signals { get; set; }
    }

    public class SystemSpanshSolidcomposition
    {
        public float Ice { get; set; }
        public float Metal { get; set; }
        public float Rock { get; set; }
    }

    public class SystemSpanshMaterials
    {
        public float Arsenic { get; set; }
        public float Carbon { get; set; }
        public float Germanium { get; set; }
        public float Iron { get; set; }
        public float Manganese { get; set; }
        public float Molybdenum { get; set; }
        public float Nickel { get; set; }
        public float Niobium { get; set; }
        public float Phosphorus { get; set; }
        public float Polonium { get; set; }
        public float Sulphur { get; set; }
        public float Cadmium { get; set; }
        public float Yttrium { get; set; }
        public float Zinc { get; set; }
        public float Chromium { get; set; }
        public float Selenium { get; set; }
        public float Tin { get; set; }
        public float Tungsten { get; set; }
        public float Mercury { get; set; }
        public float Tellurium { get; set; }
        public float Zirconium { get; set; }
        public float Technetium { get; set; }
        public float Ruthenium { get; set; }
        public float Vanadium { get; set; }
        public float Antimony { get; set; }
    }

    public class SystemSpanshSignals
    {
        public SystemSpanshSignalsDetails Signals { get; set; }
        public string updateTime { get; set; }
    }
    
    public class SystemSpanshSignalsDetails
    {
        public int SAA_SignalType_Human { get; set; }
        public int SAA_SignalType_Other { get; set; }
        public int Alexandrite { get; set; }
        public int Bromellite { get; set; }
        public int Grandidierite { get; set; }
        public int LowTemperatureDiamond { get; set; }
        public int Opal { get; set; }
        public int Tritium { get; set; }
        public int SAA_SignalType_Biological { get; set; }
        public int SAA_SignalType_Geological { get; set; }
    }

    public class SystemSpanshAtmospherecomposition
    {
        public float Carbondioxide { get; set; }
        public float Sulphurdioxide { get; set; }
        public float Nitrogen { get; set; }
        public float Oxygen { get; set; }
        public float Helium { get; set; }
        public float Hydrogen { get; set; }
        public float Methane { get; set; }
        public float Ammonia { get; set; }
        public float Argon { get; set; }
    }

    public class SystemSpanshBelt
    {
        public string name { get; set; }
        public string type { get; set; }
        public float mass { get; set; }
        public float innerRadius { get; set; }
        public float outerRadius { get; set; }
    } */
    
    public class SystemSpanshStation
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("controllingFaction")]
        public string ControllingFaction { get; set; }
        [JsonProperty("controllingFactionState")]
        public object ControllingFactionState { get; set; }
        [JsonProperty("distanceToArrival")]
        public float DistanceToArrival { get; set; }
        [JsonProperty("primaryEconomy")]
        public string PrimaryEconomy { get; set; }
        [JsonProperty("secondaryEconomy")]
        public string SecondaryEconomy { get; set; }
        [JsonProperty("government")]
        public string Government { get; set; }
        [JsonProperty("services")]
        public List<string> Services { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("landingPads")]
        public SystemSpanshLandingpadsDetails LandingPads { get; set; }
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }
        //[JsonProperty("market")]
        //public SystemSpanshMarket Market { get; set; }
        //[JsonProperty("outfitting")]
        //public SystemSpanshOutfittingDetails Outfitting { get; set; }
        [JsonProperty("allegiance")]
        public string Allegiance { get; set; }
        //[JsonProperty("shipyard")]
        //public SystemSpanshShipyardDetails Shipyard { get; set; }
    }


    public class SystemSpanshLandingpadsDetails
    {
        [JsonProperty("large")]
        public int Large { get; set; }
        [JsonProperty("medium")]
        public int Medium { get; set; }
        [JsonProperty("small")]
        public int Small { get; set; }
    }

    
    /*public class SystemSpanshMarket
    {
        public List<SystemSpanshCommodity> commodities { get; set; }
        public List<string> prohibitedCommodities { get; set; }
        public string updateTime { get; set; }
    }

    public class SystemSpanshCommodity
    {
        public string name { get; set; }
        public string symbol { get; set; }
        public string category { get; set; }
        public long commodityId { get; set; }
        public int demand { get; set; }
        public int supply { get; set; }
        public int buyPrice { get; set; }
        public int sellPrice { get; set; }
    }
    
    public class SystemSpanshOutfittingDetails
    {
        public List<SystemSpanshModule> modules { get; set; }
        public string updateTime { get; set; }
    }
    
    public class SystemSpanshModule
    {
        public string name { get; set; }
        public string symbol { get; set; }
        public long moduleId { get; set; }
        public int _class { get; set; }
        public string rating { get; set; }
        public string category { get; set; }
        public string ship { get; set; }
    }
    
    public class SystemSpanshShipyardDetails
    {
        public List<SystemSpanshShip> ships { get; set; }
        public string updateTime { get; set; }
    }

    public class SystemSpanshShip
    {
        public string name { get; set; }
        public string symbol { get; set; }
        public long shipId { get; set; }
    }*/

    
}
