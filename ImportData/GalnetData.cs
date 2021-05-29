using System.Collections.Generic;
using Newtonsoft.Json;

namespace ImportData
{

    public class GalnetRoot
    {
        [JsonProperty("data")]
        public List<GalnetDataItem> Data { get; set; }
    }

    public class GalnetDataItem
    {
        [JsonProperty("attributes")]
        public GalnetData Attributes { get; set; }

    }

    public class GalnetBodyItem
    {
        [JsonProperty("value")]
        public string Value { get; set; }

    }

    public class GalnetData
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("field_galnet_date")]
        public string Date { get; set; }

        [JsonProperty("field_galnet_image")]
        public string Image { get; set; }

        [JsonProperty("imageList")]
        public List<string> ImageList { get; set; }

        [JsonProperty("body")]
        public GalnetBodyItem BodyItem { get; set; }

        [JsonProperty("field_galnet_body")]
        public string Body { get; set; }

    }
}
