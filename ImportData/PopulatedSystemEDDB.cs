using System.Collections.Generic;
using Newtonsoft.Json;

namespace ImportData
{
    public class State
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    /*public class ActiveState
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class PendingState
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RecoveringState
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }*/

    /*public class MinorFactionPresence
    {

        [JsonProperty("happiness_id")]
        public int? HappinessId { get; set; }

        [JsonProperty("minor_faction_id")]
        public int MinorFactionId { get; set; }

        [JsonProperty("influence")]
        public double? Influence { get; set; }

        [JsonProperty("active_states")]
        public IList<ActiveState> ActiveStates { get; set; }

        [JsonProperty("pending_states")]
        public IList<PendingState> PendingStates { get; set; }

        [JsonProperty("recovering_states")]
        public IList<RecoveringState> RecoveringStates { get; set; }
    } */

    public class PopulatedSystemEDDB
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("edsm_id")]
        public int? EdsmId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }

        [JsonProperty("population")]
        public long? Population { get; set; }

        //[JsonProperty("is_populated")]
        //public bool IsPopulated { get; set; }

        //[JsonProperty("government_id")]
        //public int? GovernmentId { get; set; }

        [JsonProperty("government")]
        public string Government { get; set; }

        //[JsonProperty("allegiance_id")]
        //public int? AllegianceId { get; set; }

        [JsonProperty("allegiance")]
        public string Allegiance { get; set; }

        [JsonProperty("states")]
        public IList<State> States { get; set; }

        //[JsonProperty("security_id")]
        //public int? SecurityId { get; set; }

        [JsonProperty("security")]
        public string Security { get; set; }

        //[JsonProperty("primary_economy_id")]
        //public int? PrimaryEconomyId { get; set; }

        [JsonProperty("primary_economy")]
        public string PrimaryEconomy { get; set; }

        [JsonProperty("power")]
        public string Power { get; set; }

        [JsonProperty("power_state")]
        public string PowerState { get; set; }

        //[JsonProperty("power_state_id")]
        //public int? PowerStateId { get; set; }

        //[JsonProperty("needs_permit")]
        //public bool NeedsPermit { get; set; }

        //[JsonProperty("updated_at")]
        //public int UpdatedAt { get; set; }

        //[JsonProperty("simbad_ref")]
        //public string SimbadRef { get; set; }

        //[JsonProperty("controlling_minor_faction_id")]
        //public int? ControllingMinorFactionId { get; set; }

        [JsonProperty("controlling_minor_faction")]
        public string ControllingMinorFaction { get; set; }

        //[JsonProperty("reserve_type_id")]
        //public int? ReserveTypeId { get; set; }

        //[JsonProperty("reserve_type")]
        //public string ReserveType { get; set; }

        //[JsonProperty("minor_faction_presences")]
        //public IList<MinorFactionPresence> MinorFactionPresences { get; set; }
    }
}