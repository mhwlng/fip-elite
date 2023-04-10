using System.Collections.Generic;
using Newtonsoft.Json;

namespace ImportData
{

    public class PowerplayEDSM
    {
        public string power { get; set; }
        public string powerState { get; set; }
        public int id { get; set; }
        public long id64 { get; set; }
        public string name { get; set; }
        public Coords coords { get; set; }
        public string allegiance { get; set; }
        public string government { get; set; }
        public string state { get; set; }
        public string date { get; set; }
    }

    public class Coords
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }


}
