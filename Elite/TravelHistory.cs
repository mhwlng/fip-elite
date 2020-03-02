using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EliteAPI.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elite
{
    public static class TravelHistory
    {
        public const double SpaceMinX = -41715.0;
        public const double SpaceMaxX = 41205.0;
        public const double SpaceMinZ = -19737.0;
        public const double SpaceMaxZ = 68073.0;

        private class FSDJumpInfo
        {
            public List<double> StarPos { get; set; }
        }
        
        public static List<PointF> TravelHistoryPoints = new List<PointF>();


        public static double ImageWidth  { get; set; }
        public static double ImageHeight { get; set; }

        private class UnsafeNativeMethods
        {
            [DllImport("Shell32.dll")]
            public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
        }


        /// <summary>
        /// The standard Directory of the Player Journal files (C:\Users\%username%\Saved Games\Frontier Developments\Elite Dangerous).
        /// </summary>
        public static DirectoryInfo StandardDirectory
        {
            get
            {
                int result = UnsafeNativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out IntPtr path);
                if (result >= 0)
                {
                    try { return new DirectoryInfo(Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous"); }
                    catch { return new DirectoryInfo(Directory.GetCurrentDirectory()); }
                }
                else
                {
                    return new DirectoryInfo(Directory.GetCurrentDirectory());
                }
            }
        }


        public static void AddTravelPos(List<double> starPos)
        {
            if (starPos?.Count == 3)
            {
                var x = starPos[0];
                var y = starPos[1];
                var z = starPos[2];

                var imgX = (x - SpaceMinX) / (SpaceMaxX - SpaceMinX) * ImageWidth;
                var imgY = (SpaceMaxZ - z) / (SpaceMaxZ - SpaceMinZ) * ImageHeight;

                TravelHistoryPoints.Add(new PointF
                {
                    X = (float)imgX,
                    Y = (float)imgY
                });
            }
        }

        public static void GetTravelHistory()
        {
            var journalDirectory = StandardDirectory;

            if (!Directory.Exists(journalDirectory.FullName))
            {
                App.log.Error($"Directory {journalDirectory.FullName} not found.");
                return;
            }

            try
            {
                var image = Image.FromFile("Templates\\images\\galaxy.png");
                ImageWidth = image.Width;
                ImageHeight = image.Height;

                var journalFiles = journalDirectory.GetFiles("Journal.*").OrderBy(x => x.LastWriteTime);

                foreach (var journalFile in journalFiles)
                {
                    using (FileStream fileStream =
                        journalFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            while (!streamReader.EndOfStream)
                            {
                                try
                                {
                                    string json = streamReader.ReadLine();

                                    if (json?.Contains("\"event\":\"FSDJump\",") == true)
                                    {
                                        FSDJumpInfo info = JsonConvert.DeserializeObject<FSDJumpInfo>(json);

                                        if (info.StarPos?.Count == 3)
                                        {
                                            AddTravelPos(info.StarPos);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    App.log.Error(ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }

        }

    }
}
