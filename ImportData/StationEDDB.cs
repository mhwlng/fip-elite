using System.Collections.Generic;
using Newtonsoft.Json;

namespace ImportData
{

    public class StationEDDB
    {

        //[JsonProperty("id")]
        //public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("system_id")]
        public int SystemId { get; set; }

        //[JsonProperty("updated_at")]
        //public int UpdatedAt { get; set; }

        [JsonProperty("max_landing_pad_size")]
        public string MaxLandingPadSize { get; set; }

        //[JsonProperty("distance_to_star")]
        //public int? DistanceToStar { get; set; }

        //[JsonProperty("government_id")]
        //public int? GovernmentId { get; set; }

        //[JsonProperty("government")]
        //public string Government { get; set; }

        //[JsonProperty("allegiance_id")]
        //public int? AllegianceId { get; set; }

        //[JsonProperty("allegiance")]
        //public string Allegiance { get; set; }

        //[JsonProperty("states")]
        //public IList<State> States { get; set; }

        //[JsonProperty("type_id")]
        //public int? TypeId { get; set; }

        //[JsonProperty("type")]
        //public string Type { get; set; }

        //[JsonProperty("has_blackmarket")]
        //public bool HasBlackmarket { get; set; }

        //[JsonProperty("has_market")]
        //public bool HasMarket { get; set; }

        //[JsonProperty("has_refuel")]
        //public bool HasRefuel { get; set; }

        //[JsonProperty("has_repair")]
        //public bool HasRepair { get; set; }

        //[JsonProperty("has_rearm")]
        //public bool HasRearm { get; set; }

        //[JsonProperty("has_outfitting")]
        //public bool HasOutfitting { get; set; }

        //[JsonProperty("has_shipyard")]
        //public bool HasShipyard { get; set; }

        //[JsonProperty("has_docking")]
        //public bool HasDocking { get; set; }

        //[JsonProperty("has_commodities")]
        //public bool HasCommodities { get; set; }

        //[JsonProperty("import_commodities")]
        //public IList<string> ImportCommodities { get; set; }

        //[JsonProperty("export_commodities")]
        //public IList<string> ExportCommodities { get; set; }

        //[JsonProperty("prohibited_commodities")]
        //public IList<string> ProhibitedCommodities { get; set; }

        [JsonProperty("economies")]
        public IList<string> Economies { get; set; }

        //[JsonProperty("shipyard_updated_at")]
        //public int? ShipyardUpdatedAt { get; set; }

        //[JsonProperty("outfitting_updated_at")]
        //public int? OutfittingUpdatedAt { get; set; }

        //[JsonProperty("market_updated_at")]
        //public int? MarketUpdatedAt { get; set; }

        [JsonProperty("is_planetary")]
        public bool IsPlanetary { get; set; }

        //[JsonProperty("selling_ships")]
        //public IList<string> SellingShips { get; set; }

        //[JsonProperty("selling_modules")]
        //public IList<int> SellingModules { get; set; }

        //[JsonProperty("settlement_size_id")]
        //public int? SettlementSizeId { get; set; }

        //[JsonProperty("settlement_size")]
        //public string SettlementSize { get; set; }

        //[JsonProperty("settlement_security_id")]
        //public int? SettlementSecurityId { get; set; }

        //[JsonProperty("settlement_security")]
        //public string SettlementSecurity { get; set; }

        //[JsonProperty("body_id")]
        //public long? BodyId { get; set; }

        [JsonProperty("controlling_minor_faction_id")]
        public int? ControllingMinorFactionId { get; set; }

        [JsonIgnore]
        public string SystemName { get; set; }

    }

}
