using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
// ReSharper disable IdentifierTypo

namespace Elite
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


        [JsonProperty("imageList")]
        public List<string> ImageList { get; set; }


        [JsonProperty("slug")]
        public string Slug { get; set; }

    }

    public static class Galnet
    {
        public static Dictionary<string,List<GalnetData>>GalnetList = new Dictionary<string, List<GalnetData>>();

        public static Dictionary<string, List<GalnetData>> GetGalnet(string path)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<List<GalnetData>>(File.ReadAllText(path))
                        .GroupBy(x => x.Date)
                        .ToDictionary(x => x.Key, x => x.ToList())
                        .Take(10).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new Dictionary<string, List<GalnetData>>();

        }

        public static void GetGalnetImages(Dictionary<string, List<GalnetData>> GalnetList)
        {
            try
            {
                var directory = Path.Combine(App.ExePath, "templates/images/galnet");

                foreach (var l in GalnetList)
                {
                    foreach (var n in l.Value)
                    {
                        for (var i = 0; i < n.ImageList.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(n.ImageList[i]))
                            {
                                var imageName = n.ImageList[i] + ".png";

                                var imgPath = Path.Combine(directory, imageName);

                                if (!File.Exists(imgPath))
                                {
                                    try
                                    {
                                        using (var client = new WebClient())
                                        {
                                            client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                                            var data = client.DownloadData("https://hosting.zaonce.net/elite-dangerous/galnet/" + imageName);

                                            File.WriteAllBytes(imgPath, data);
                                        }
                                    }
                                    catch
                                    {
                                        App.Log.Error("galnet image not found " + imageName);

                                        n.ImageList[i] = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

        }


    }
}
