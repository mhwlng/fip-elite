using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using IniParser;
using IniParser.Model;

namespace Elite
{
    // copied from https://github.com/zipferot3000/HWiNFO-Shared-Memory-Dump/blob/master/Program.cs

    public static class HWInfo
    {
        public enum SENSOR_READING_TYPE
        {
            SENSOR_TYPE_NONE,
            SENSOR_TYPE_TEMP,
            SENSOR_TYPE_VOLT,
            SENSOR_TYPE_FAN,
            SENSOR_TYPE_CURRENT,
            SENSOR_TYPE_POWER,
            SENSOR_TYPE_CLOCK,
            SENSOR_TYPE_USAGE,
            SENSOR_TYPE_OTHER,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct _HWiNFO_SHARED_MEM
        {
            public uint Signature;
            public uint Version;
            public uint Revision;
            public long PollTime;
            public uint OffsetOfSensorSection;
            public uint SizeOfSensorElement;
            public uint NumSensorElements;
            public uint OffsetOfReadingSection;
            public uint SizeOfReadingElement;
            public uint NumReadingElements;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class _HWiNFO_SENSOR
        {
            public uint SensorId;
            public uint SensorInst;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public string SensorNameOrig;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public string SensorNameUser;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class _HWiNFO_ELEMENT
        {
            public SENSOR_READING_TYPE Reading;
            public uint SensorIndex;
            public uint SensorId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public string LabelOrig;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public string LabelUser;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HWINFO_UNIT_STRING_LEN)]
            public string Unit;
            public double Value;
            public double ValueMin;
            public double ValueMax;
            public double ValueAvg;
        }
        
        public class ElementObj
        {
            public SENSOR_READING_TYPE Reading;
            public uint SensorIndex;
            public uint SensorId;
            public string LabelOrig;
            public string LabelUser;
            public string Unit;
            public string Value;
            public string ValueMin;
            public string ValueMax;
            public string ValueAvg;
        }

        public class SensorObj
        {
            public uint SensorId;
            public uint SensorInst;
            public string SensorNameOrig;
            public string SensorNameUser;
            public List<ElementObj> Elements;
        }

        public static readonly object RefreshHWInfoLock = new object();

        private const string HWINFO_SHARED_MEM_FILE_NAME = "Global\\HWiNFO_SENS_SM2";
        private const int HWINFO_SENSORS_STRING_LEN = 128;
        private const int HWINFO_UNIT_STRING_LEN = 16;

        public static List<SensorObj> FullSensorData = new List<SensorObj>();
        public static List<SensorObj> SensorData = new List<SensorObj>();

        public static IniData IncData = null;
        
        private static string NumberFormat(SENSOR_READING_TYPE reading, string unit, double value)
        {
            string valstr = "?";

            switch (reading)
            {
                case SENSOR_READING_TYPE.SENSOR_TYPE_VOLT:
                    valstr = value.ToString("N3");
                    break;
                case SENSOR_READING_TYPE.SENSOR_TYPE_CURRENT:
                    valstr = value.ToString("N3");
                    break;
                case SENSOR_READING_TYPE.SENSOR_TYPE_POWER:
                    valstr = value.ToString("N3");
                    break;

                case SENSOR_READING_TYPE.SENSOR_TYPE_CLOCK:
                    valstr = value.ToString("N1");
                    break;
                case SENSOR_READING_TYPE.SENSOR_TYPE_USAGE:
                    valstr = value.ToString("N1");
                    break;
                case SENSOR_READING_TYPE.SENSOR_TYPE_TEMP:
                    valstr = value.ToString("N1");
                    break;

                case SENSOR_READING_TYPE.SENSOR_TYPE_FAN:
                    valstr = value.ToString("N0");
                    break;

                case SENSOR_READING_TYPE.SENSOR_TYPE_OTHER:

                    if (unit == "Yes/No")
                    {
                        return value == 0 ? "No" : "Yes";
                    }
                    else if (unit.EndsWith("GT/s") || unit == "x" || unit == "%")
                    {
                        valstr = value.ToString("N1");
                    }
                    else if (unit.EndsWith("/s"))
                    {
                        valstr = value.ToString("N3");
                    }
                    else if (unit.EndsWith("MB") || unit.EndsWith("GB") || unit == "T" || unit == "FPS")
                    {
                        valstr = value.ToString("N0");
                    }
                    else
                        valstr = value.ToString();

                    break;

                case SENSOR_READING_TYPE.SENSOR_TYPE_NONE:
                    valstr = value.ToString();
                    break;

            }

            return (valstr + " " + unit).Trim();
        }

        public static void ReadMem(string incPath)
        {
            lock (RefreshHWInfoLock)
            {
                try
                {
                    FullSensorData = new List<SensorObj>();
                    SensorData = new List<SensorObj>();

                    var mmf = MemoryMappedFile.OpenExisting(HWINFO_SHARED_MEM_FILE_NAME, MemoryMappedFileRights.Read);
                    var accessor = mmf.CreateViewAccessor(0L, Marshal.SizeOf(typeof(_HWiNFO_SHARED_MEM)), MemoryMappedFileAccess.Read);
                    
                    accessor.Read(0L, out _HWiNFO_SHARED_MEM hWiNFOMemory);
                    
                    ReadSensors(mmf, hWiNFOMemory);

                    if (IncData == null)
                    {
                        incPath = Path.Combine(App.ExePath, incPath);

                        if (File.Exists(incPath))
                        {
                            var parser = new FileIniDataParser();

                            IncData = parser.ReadFile(incPath);
                        }
                    }

                    ParseIncFile();

                }
                catch 
                {
                    // don nothing
                }
            }
        }

        private static void ReadSensors(MemoryMappedFile mmf, _HWiNFO_SHARED_MEM hWiNFOMemory)
        {
            for (uint index = 0; index < hWiNFOMemory.NumSensorElements; ++index)
            {
                using (var viewStream = mmf.CreateViewStream(hWiNFOMemory.OffsetOfSensorSection + index * hWiNFOMemory.SizeOfSensorElement, hWiNFOMemory.SizeOfSensorElement, MemoryMappedFileAccess.Read))
                {
                    var buffer = new byte[(int)hWiNFOMemory.SizeOfSensorElement];
                    viewStream.Read(buffer, 0, (int)hWiNFOMemory.SizeOfSensorElement);
                    var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    var structure = (_HWiNFO_SENSOR)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(_HWiNFO_SENSOR));
                    gcHandle.Free();
                    
                    var obj = new SensorObj
                    {
                        SensorId = structure.SensorId,
                        SensorInst = structure.SensorInst,
                        SensorNameOrig = structure.SensorNameOrig,
                        SensorNameUser = structure.SensorNameUser,
                        Elements = new List<ElementObj>()
                    };
                    FullSensorData.Add(obj);
                }
            }
            
            ReadElements(mmf, hWiNFOMemory);
        }

        private static void ReadElements(MemoryMappedFile mmf, _HWiNFO_SHARED_MEM hWiNFOMemory)
        {
            for (uint index = 0; index < hWiNFOMemory.NumReadingElements; ++index)
            {
                using (var viewStream = mmf.CreateViewStream(hWiNFOMemory.OffsetOfReadingSection + index * hWiNFOMemory.SizeOfReadingElement, hWiNFOMemory.SizeOfReadingElement, MemoryMappedFileAccess.Read))
                {
                    var buffer = new byte[(int)hWiNFOMemory.SizeOfReadingElement];
                    viewStream.Read(buffer, 0, (int)hWiNFOMemory.SizeOfReadingElement);
                    var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    var structure = (_HWiNFO_ELEMENT)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(_HWiNFO_ELEMENT));
                    gcHandle.Free();

                    var obj = new ElementObj
                    {
                        Reading = structure.Reading,
                        SensorIndex = structure.SensorIndex,
                        SensorId = structure.SensorId,
                        LabelOrig = structure.LabelOrig,
                        LabelUser = structure.LabelUser,
                        Unit = structure.Unit,
                        Value = NumberFormat(structure.Reading, structure.Unit, structure.Value),
                        ValueMin = NumberFormat(structure.Reading, structure.Unit, structure.ValueMin),
                        ValueMax = NumberFormat(structure.Reading, structure.Unit, structure.ValueMax),
                        ValueAvg = NumberFormat(structure.Reading, structure.Unit,structure.ValueAvg)
                    };
                    
                    FullSensorData[(int)structure.SensorIndex].Elements.Add(obj);
                }
            }
        }

        private static void ParseIncFile()
        {
            if (IncData != null && FullSensorData.Any())
            {
                foreach (SectionData section in IncData.Sections.Where(x => x.SectionName != "Variables"))
                {
                    var sectionName = Regex.Replace(section.SectionName, "HWINFO-CONFIG-", "", RegexOptions.IgnoreCase);

                    var obj = new SensorObj
                    {
                        SensorId = 0,
                        SensorInst = 0,
                        SensorNameOrig = sectionName,
                        SensorNameUser = sectionName,
                        Elements = new List<ElementObj>()
                    };

                    //Iterate through all the keys in the current section
                    //printing the values
                    foreach (KeyData key in section.Keys)
                    {
                        var elementName = key.Value;

                        var sensorIdStr = IncData["Variables"][key.KeyName + "-SensorId"];
                        var sensorInstanceStr = IncData["Variables"][key.KeyName + "-SensorInstance"];
                        var entryIdStr = IncData["Variables"][key.KeyName + "-EntryId"];

                        if (sensorIdStr?.StartsWith("0x") == true && sensorInstanceStr?.StartsWith("0x") == true &&
                            entryIdStr?.StartsWith("0x") == true)
                        {
                            var sensorId = Convert.ToUInt32(sensorIdStr.Replace("0x", ""), 16);
                            var sensorInstance = Convert.ToUInt32(sensorInstanceStr.Replace("0x", ""), 16);
                            var entryId = Convert.ToUInt32(entryIdStr.Replace("0x", ""), 16);

                            var sensor = FullSensorData.FirstOrDefault(x =>
                                x.SensorId == sensorId && x.SensorInst == sensorInstance);

                            if (sensor?.Elements.Any() == true)
                            {
                                var element =
                                    sensor.Elements.FirstOrDefault(x => x.SensorId == entryId);

                                if (element != null)
                                {
                                    var obj2 = new ElementObj
                                    {
                                        Reading = element.Reading,
                                        SensorIndex = element.SensorIndex,
                                        SensorId = element.SensorId,
                                        LabelOrig = elementName,
                                        LabelUser = elementName,
                                        Unit = element.Unit,
                                        Value = element.Value,
                                        ValueMin = element.ValueMin,
                                        ValueMax = element.ValueMax,
                                        ValueAvg = element.ValueAvg
                                    };
                                    obj.Elements.Add(obj2);
                                }
                            }
                        }

                    }

                    if (obj.Elements.Any())
                    {
                        SensorData.Add(obj);
                    }
                }
            }

        }

        public static void SaveDataToFile(string path)
        {
            path = Path.Combine(App.ExePath, path);

            using (var fs = File.Create(path))
            {
                var json = new UTF8Encoding(true).GetBytes(JsonConvert.SerializeObject(FullSensorData, Formatting.Indented));
                fs.Write(json, 0, json.Length);
            }
        }

    }
}
