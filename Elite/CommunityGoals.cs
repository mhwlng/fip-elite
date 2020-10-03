using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
// ReSharper disable IdentifierTypo

namespace Elite
{
    public class CommunityGoal
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("expiry")]
        public string Expiry { get; set; }
        [JsonProperty("market_name")]
        public string MarketName { get; set; }
        [JsonProperty("starsystem_name")]
        public string StarsystemName { get; set; }
        [JsonProperty("activityType")]
        public string ActivityType { get; set; }
        [JsonProperty("target_commodity_list")]
        public string TargetCommodityList { get; set; }
        [JsonProperty("target_qty")]
        public string TargetQty { get; set; }
        [JsonProperty("qty")]
        public string Qty { get; set; }
        [JsonProperty("objective")]
        public string Objective { get; set; }
        [JsonProperty("news")]
        public string News { get; set; }
        [JsonProperty("bulletin")]
        public string Bulletin { get; set; }
    }


    public static class CommunityGoals
    {
        public static Dictionary<string, List<GalnetData>> GetCommunityGoals(string path)
        {
            try
            {
                path = Path.Combine(App.ExePath, path);

                if (File.Exists(path))
                {
                    var cgList =  JsonConvert.DeserializeObject<List<CommunityGoal>>(File.ReadAllText(path));


                    if (cgList?.Any() == true)
                    {
                        var galnet = new Dictionary<string, List<GalnetData>>();

                        for (var i = 0; i < cgList.Count; i++)
                        {
                            var cg = cgList[i];

                            var body = new StringBuilder();

                            try
                            {
                                body.Append("<table><tbody>");
                                
                                body.Append("<tr><td class=\"caption\">");
                                body.Append("System</td><td class=\"data\">");
                                body.Append(cg.StarsystemName);
                                body.Append("</td></tr>");

                                body.Append("<tr><td class=\"caption\">");
                                body.Append("Station</td><td class=\"data\">");
                                body.Append(cg.MarketName);
                                body.Append("</td></tr>");

                                body.Append("<tr><td class=\"caption\">");
                                body.Append("Objective</td><td class=\"data\">");
                                body.Append(cg.Objective);
                                body.Append("</td></tr>");

                                body.Append("<tr><td class=\"caption\">");
                                body.Append("Commodity List</td><td class=\"data\">");
                                body.Append(cg.TargetCommodityList);
                                body.Append("</td></tr>");

                                body.Append("<tr><td class=\"caption\">");
                                body.Append("Progress</td><td class=\"data\">");
                                body.Append(
                                    (Convert.ToDouble(cg.Qty) / Convert.ToDouble(cg.TargetQty) * 100.0)
                                    .ToString("0.#") + "%");
                                body.Append("</td></tr>");

                                body.Append("<tr><td class=\"caption\">");
                                body.Append("Expiry</td><td class=\"data\">");
                                var expiry = DateTime.ParseExact(cg.Expiry, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                var ts = expiry - DateTime.Now.ToUniversalTime();
                                body.Append(ts.ToHumanReadableString());
                                body.Append("</td></tr>");

                                body.Append("<tr><td class=\"divider\" colspan=\"2\">&nbsp;</td></tr>");

                                body.Append("</tbody></table><br>");

                                body.Append(cg.Bulletin.Replace("\n", "<br>").Replace("<br><br>", "<br>")
                                    .Replace("<br> <br>", "<br>"));

                                var gd = new GalnetData
                                {
                                    Date = "Community Goal " + (i + 1) + " of " + cgList.Count,
                                    Title = cg.Title,
                                    Body = body.ToString()
                                };

                                galnet.Add("cg" + i, new List<GalnetData> {gd});
                            }
                            catch
                            {
                                // do nothing
                            }

                        }

                        return galnet;
                    }



                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex);
            }

            return new Dictionary<string, List<GalnetData>>();

        }



    }
}
