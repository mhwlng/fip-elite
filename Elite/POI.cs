using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

namespace Elite
{
    public class PoiItem
    {
        [Name("Location Name")] public string LocationName { get; set; }

        [Name("System")] public string System { get; set; }
        [Name("Body")] public string Body { get; set; }
        [Name("Category")] public string Category { get; set; }
        [Name("Sub-Category")] public string SubCategory { get; set; }
        [Name("Latitude")] public double Latitude { get; set; }

        [Name("Longitude")] public double Longitude { get; set; }
        [Name("Radius")] public int Radius { get; set; }
        [Name("Gravity")] public double Gravity { get; set; }
        [Name("Distance to Sol")] public double DistanceToSol { get; set; }
        [Name("Distance to arrival")] public double DistanceToarrival { get; set; }
        [Name("Distance to Colonia")] public double DistanceToColonia { get; set; }
        [Name("Notes")] public string Notes { get; set; }
        [Name("Notable loot / lore")] public string Notable { get; set; }
        [Name("Yttrium")] public double? Yttrium { get; set; }
        [Name("Technetium")] public double? Technetium { get; set; }
        [Name("Ruthenium")] public double? Ruthenium { get; set; }
        [Name("Selenium")] public double? Selenium { get; set; }
        [Name("Tellurium")] public double? Tellurium { get; set; }
        [Name("Polonium")] public double? Polonium { get; set; }
        [Name("Antimony")] public double? Antimony { get; set; }
        [Name("Niobium")] public double? Niobium { get; set; }
        [Name("Molybdenum")] public double? Molybdenum { get; set; }
        [Name("Cadmium")] public double? Cadmium { get; set; }
        [Name("Tin")] public double? Tin { get; set; }
        [Name("Tungsten")] public double? Tungsten { get; set; }
        [Name("Mercury")] public double? Mercury { get; set; }
        [Name("Found by CMDR")] public string FoundByCmdr { get; set; }
        [Name("EDDB")] public string EDDB { get; set; }
        [Name("EDSM")] public string EDSM { get; set; }
        [Name("Canonn")] public string Canonn { get; set; }
        [Name("Forums")] public string Forums { get; set; }
        [Name("Link misc")] public string Link { get; set; }
        [Name("Screenshot")] public string Screenshot { get; set; }
        [Name("Date added")] public DateTime DateAdded { get; set; }
        [Name("Checked/verified?")] public string Checked { get; set; }
        [Name("Galactic X")] public double GalacticX { get; set; }
        [Name("Galactic Y")] public double GalacticY { get; set; }
        [Name("Galactic Z")] public double GalacticZ { get; set; }
        [Name("X/Y/Z")] public string XYZ { get; set; }

        public double Distance { get; set; }
    }

    public class CustomPercentConverter : DoubleConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return default(double);
            }

            try
            {
                return base.ConvertFromString(text.Replace("%", ""), row, memberMapData);
            }
            catch (TypeConverterException)
            {
                return default(double);
            }
            catch
            {
                throw;
            }
        }
    }

    public class CustomDateTimeConverter : DateTimeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return default(DateTime);
            }

            try
            {
                return base.ConvertFromString(text, row, memberMapData);
            }
            catch (TypeConverterException)
            {
                return default(DateTime);
            }
            catch
            {
                throw;
            }
        }
    }

    public sealed class PoiItemMap : ClassMap<PoiItem>
    {
        public PoiItemMap()
        {
            Map(m => m.LocationName).Index(0);
            Map(m => m.System).Index(1);
            Map(m => m.Body).Index(2);
            Map(m => m.Category).Index(3);
            Map(m => m.SubCategory).Index(4);
            Map(m => m.Latitude).Index(5);
            Map(m => m.Longitude).Index(6);
            Map(m => m.Radius).Index(7);
            Map(m => m.Gravity).Index(8);
            Map(m => m.DistanceToSol).Index(9);
            Map(m => m.DistanceToarrival).Index(10);
            Map(m => m.DistanceToColonia).Index(11);
            Map(m => m.Notes).Index(12);
            Map(m => m.Notable).Index(13);

            Map(m => m.Yttrium).Index(14).TypeConverter<CustomPercentConverter>();
            Map(m => m.Technetium).Index(15).TypeConverter<CustomPercentConverter>();
            Map(m => m.Ruthenium).Index(16).TypeConverter<CustomPercentConverter>();
            Map(m => m.Selenium).Index(17).TypeConverter<CustomPercentConverter>();
            Map(m => m.Tellurium).Index(18).TypeConverter<CustomPercentConverter>();
            Map(m => m.Polonium).Index(19).TypeConverter<CustomPercentConverter>();
            Map(m => m.Antimony).Index(20).TypeConverter<CustomPercentConverter>();
            Map(m => m.Niobium).Index(21).TypeConverter<CustomPercentConverter>();
            Map(m => m.Molybdenum).Index(22).TypeConverter<CustomPercentConverter>();
            Map(m => m.Cadmium).Index(23).TypeConverter<CustomPercentConverter>();
            Map(m => m.Tin).Index(24).TypeConverter<CustomPercentConverter>();
            Map(m => m.Tungsten).Index(25).TypeConverter<CustomPercentConverter>();
            Map(m => m.Mercury).Index(26).TypeConverter<CustomPercentConverter>();

            Map(m => m.FoundByCmdr).Index(27);
            Map(m => m.EDDB).Index(28);
            Map(m => m.EDSM).Index(29);
            Map(m => m.Canonn).Index(30);
            Map(m => m.Forums).Index(31);
            Map(m => m.Link).Index(32);
            Map(m => m.Screenshot).Index(33);
            Map(m => m.DateAdded).Index(34).TypeConverter<CustomDateTimeConverter>();
            Map(m => m.Checked).Index(35);
            Map(m => m.GalacticX).Index(36);
            Map(m => m.GalacticY).Index(37);
            Map(m => m.GalacticZ).Index(38);
            Map(m => m.XYZ).Index(39);
        }
    }


    public static class Poi
    {
        public static List<PoiItem> NearbyPoiList = new List<PoiItem>();
        public static List<PoiItem> FullPoiList = null;

        // see https://www.reddit.com/r/EliteDangerous/comments/9mfiug/edison_a_tool_which_helps_getting_to_planet/

        // see https://docs.google.com/spreadsheets/d/11E05a-hLGyOQ84B9dQUu0Z53ow1Xt-uqJ-xXJmtYq5A/edit#gid=594549382

        public const string PoiSpreadsheet =
            @"https://docs.google.com/spreadsheets/d/11E05a-hLGyOQ84B9dQUu0Z53ow1Xt-uqJ-xXJmtYq5A/export?format=csv&gid=594549382";


        public static List<PoiItem> GetAllPois()
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(PoiSpreadsheet);
                req.KeepAlive = false;
                req.ProtocolVersion = HttpVersion.Version10;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                using (StreamReader streamReader = new StreamReader(resp.GetResponseStream()))
                {
                    var csvread = new CsvReader(streamReader, new CultureInfo("en-US"));
                    csvread.Configuration.HasHeaderRecord = true;
                    //csv.Configuration.PrepareHeaderForMatch = header => header?.Trim();
                    csvread.Configuration.TrimOptions = TrimOptions.Trim;
                    csvread.Configuration.Delimiter = ",";
                    csvread.Configuration.RegisterClassMap<PoiItemMap>();
                    csvread.Configuration.MissingFieldFound = null;
                    //csvread.Configuration.HeaderValidated = null;
                    csvread.Configuration.IgnoreBlankLines = true;
                    csvread.Configuration.ShouldSkipRecord = (records) => string.IsNullOrEmpty(records[0]);
                    csvread.Configuration.TypeConverterOptionsCache.GetOptions<DateTime>().Formats =
                        new[] { "yyyy-MM-dd" };


                    return csvread.GetRecords<PoiItem>().Where(x => !x.LocationName.Trim().EndsWith("[X]")).ToList();

                }
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }

            return new List<PoiItem>();

        }

        public static List<PoiItem> GetNearestPois(List<double> starPos)
        {
            if (FullPoiList?.Any() == true && starPos?.Count == 3)
            {
                FullPoiList.ForEach(poiItem =>
                {
                    var Xs = starPos[0];
                    var Ys = starPos[1];
                    var Zs = starPos[2];

                    var Xd = poiItem.GalacticX;
                    var Yd = poiItem.GalacticY;
                    var Zd = poiItem.GalacticZ;

                    double deltaX = Xs - Xd;
                    double deltaY = Ys - Yd;
                    double deltaZ = Zs - Zd;

                    poiItem.Distance = (double)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                });

                return Enumerable.Where<PoiItem>(FullPoiList, x => x.Distance >= 0).OrderBy(x => x.Distance).Take(5).ToList();
            }

            return new List<PoiItem>();

        }
    }
}
