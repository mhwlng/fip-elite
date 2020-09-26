using System.Collections.Generic;
using Newtonsoft.Json;

namespace ImportData
{
    public class GalnetData
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("nid")]
        public string Nid { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("imageList")]
        public List<string> ImageList { get; set; }


        [JsonProperty("slug")]
        public string Slug { get; set; }

    }
}
