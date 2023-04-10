using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace ImportData
{

    public class PopulatedSystemEDSMCoords
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class PopulatedSystemEDSMControllingfaction
    {
        public long id { get; set; }
        public string name { get; set; }
        public string allegiance { get; set; }
        public string government { get; set; }
        public bool isPlayer { get; set; }
    }

    public class PopulatedSystemEDSMControllingfaction1
    {
        public long id { get; set; }
        public string name { get; set; }
    }
    public class PopulatedSystemEDSMStation
    {
        public long id { get; set; }
        public long marketId { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public float distanceToArrival { get; set; }
        public string allegiance { get; set; }
        public string government { get; set; }
        public string economy { get; set; }
        public string secondEconomy { get; set; }
        public bool haveMarket { get; set; }
        public bool haveShipyard { get; set; }
        public bool haveOutfitting { get; set; }
        public List<string> otherServices { get; set; }
        public PopulatedSystemEDSMUpdatetime updateTime { get; set; }
        public Body body { get; set; }
        public PopulatedSystemEDSMControllingfaction1 controllingFaction { get; set; }
    }

    public class PopulatedSystemEDSMUpdatetime
    {
        public string information { get; set; }
        public string market { get; set; }
        public string shipyard { get; set; }
        public string outfitting { get; set; }
    }

    public class PopulatedSystemEDSMBody
    {
        public long id { get; set; }
        public string name { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
    }
    /*
    public class PopulatedSystemEDSMBody1
    {
        public long id { get; set; }
        public long id64 { get; set; }
        public long bodyId { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string subType { get; set; }
        public List<PopulatedSystemEDSMParent> parents { get; set; }
        public long distanceToArrival { get; set; }
        public bool isMainStar { get; set; }
        public bool isScoopable { get; set; }
        public long age { get; set; }
        public string spectralClass { get; set; }
        public string luminosity { get; set; }
        public float absoluteMagnitude { get; set; }
        public float solarMasses { get; set; }
        public float solarRadius { get; set; }
        public long surfaceTemperature { get; set; }
        public float? orbitalPeriod { get; set; }
        public float? semiMajorAxis { get; set; }
        public float? orbitalEccentricity { get; set; }
        public float? orbitalInclination { get; set; }
        public float? argOfPeriapsis { get; set; }
        public float rotationalPeriod { get; set; }
        public bool rotationalPeriodTidallyLocked { get; set; }
        public float? axialTilt { get; set; }
        public string updateTime { get; set; }
        public bool isLandable { get; set; }
        public float gravity { get; set; }
        public float earthMasses { get; set; }
        public float radius { get; set; }
        public float? surfacePressure { get; set; }
        public string volcanismType { get; set; }
        public string atmosphereType { get; set; }
        public PopulatedSystemEDSMAtmospherecomposition atmosphereComposition { get; set; }
        public PopulatedSystemEDSMSolidcomposition solidComposition { get; set; }
        public string terraformingState { get; set; }
        public PopulatedSystemEDSMMaterials PopulatedSystemEdsmMaterials { get; set; }
        public List<PopulatedSystemEDSMBelt> belts { get; set; }
    }
    
    public class PopulatedSystemEDSMAtmospherecomposition
    {
        public float Argon { get; set; }
        public float Nitrogen { get; set; }
        public float Oxygen { get; set; }
        public float Carbondioxide { get; set; }
        public float Ammonia { get; set; }
        public float Sulphurdioxide { get; set; }
        public float Neon { get; set; }
        public float Water { get; set; }
        public float Hydrogen { get; set; }
        public float Helium { get; set; }
    }

    public class PopulatedSystemEDSMSolidcomposition
    {
        public float Rock { get; set; }
        public float Metal { get; set; }
        public float Ice { get; set; }
    }

    public class PopulatedSystemEDSMMaterials
    {
        public float Iron { get; set; }
        public float Sulphur { get; set; }
        public float Nickel { get; set; }
        public float Carbon { get; set; }
        public float Chromium { get; set; }
        public float Phosphorus { get; set; }
        public float Vanadium { get; set; }
        public float Germanium { get; set; }
        public float Molybdenum { get; set; }
        public float Tungsten { get; set; }
        public float Technetium { get; set; }
        public float Manganese { get; set; }
        public float Selenium { get; set; }
        public float Niobium { get; set; }
        public float Tin { get; set; }
        public float Antimony { get; set; }
        public float Zinc { get; set; }
        public float Tellurium { get; set; }
        public float Yttrium { get; set; }
        public float Zirconium { get; set; }
        public float Cadmium { get; set; }
        public float Polonium { get; set; }
        public float Ruthenium { get; set; }
        public float Arsenic { get; set; }
    }

    public class PopulatedSystemEDSMParent
    {
        public long Star { get; set; }
        public long Planet { get; set; }
        public long Null { get; set; }
    }

    public class PopulatedSystemEDSMBelt
    {
        public string name { get; set; }
        public string type { get; set; }
        public long mass { get; set; }
        public long innerRadius { get; set; }
        public long outerRadius { get; set; }
    } */

    public class PopulatedSystemEDSMFaction
    {
        public long id { get; set; }
        public string name { get; set; }
        public string allegiance { get; set; }
        public string government { get; set; }
        public float influence { get; set; }
        public string state { get; set; }
        //public object[] activeStates { get; set; }
        //public object[] recoveringStates { get; set; }
        //public object[] pendingStates { get; set; }
        public string happiness { get; set; }
        public bool isPlayer { get; set; }
        public long lastUpdate { get; set; }
    }

    public class PopulatedSystemEDSM
    {
        public long id { get; set; }
        public long id64 { get; set; }
        public string name { get; set; }
        public PopulatedSystemEDSMCoords Coords { get; set; }
        public string allegiance { get; set; }
        public string government { get; set; }
        public string state { get; set; }
        public string economy { get; set; }
        public string security { get; set; }
        public long population { get; set; }
        public PopulatedSystemEDSMControllingfaction controllingFaction { get; set; }
        public List<PopulatedSystemEDSMStation> stations { get; set; }
        //public List<PopulatedSystemEDSMBody1> bodies { get; set; }
        public string date { get; set; }
        public List<PopulatedSystemEDSMFaction> factions { get; set; }
    }
}