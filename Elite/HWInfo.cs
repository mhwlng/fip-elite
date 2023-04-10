﻿using System;
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
using System.Xml;
using IniParser;
using IniParser.Model;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace Elite
{
    // copied from https://github.com/zipferot3000/HWiNFO-Shared-Memory-Dump/blob/master/Program.cs

    public static class HWInfo
    {
        public enum SENSOR_TYPE
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
            public uint SensorInstance;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public byte[] SensorNameOrig;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public byte[] SensorNameUser;

            // Version 2+ new:
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public byte[] UtfSensorNameUser; // Sensor name displayed, which might be translated or renamed by user [UTF-8 string]

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class _HWiNFO_ELEMENT
        {
            public SENSOR_TYPE SensorType;
            public uint SensorIndex;
            public uint ElementId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public byte[] LabelOrig;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public byte[] LabelUser;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HWINFO_UNIT_STRING_LEN)]
            public byte[] Unit;
            public double Value;
            public double ValueMin;
            public double ValueMax;
            public double ValueAvg;

            // Version 2+ new:
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HWINFO_SENSORS_STRING_LEN)]
            public byte[] UtfLabelUser; // Label displayed, which might be translated or renamed by user [UTF-8 string]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HWINFO_UNIT_STRING_LEN)]
            public byte[] UtfUnit;          // e.g. "RPM" [UTF-8 string]
        }
        
        public class ElementObj
        {
            [JsonIgnore]
            public string ElementKey;

            public SENSOR_TYPE SensorType;

            public uint ElementId;
            public string LabelOrig;
            public string LabelUser;
            public string Unit;
            [JsonIgnore]
            public float NumericValue;
            public string Value;
            public string ValueMin;
            public string ValueMax;
            public string ValueAvg;
        }

        public class MQTTObj
        {
            public float Value;
            public string Unit;
        }


        public class SensorObj
        {
            public uint SensorId;
            public uint SensorInstance;

            public string SensorNameOrig;
            public string SensorNameUser;
            public Dictionary<string,ElementObj> Elements;
        }

        public static readonly object RefreshHWInfoLock = new object();

        private const string HWINFO_SHARED_MEM_FILE_NAME = "Global\\HWiNFO_SENS_SM2";
        private const int HWINFO_SENSORS_STRING_LEN = 128;
        private const int HWINFO_UNIT_STRING_LEN = 16;

        public static Dictionary<int,SensorObj> FullSensorData = new Dictionary<int,SensorObj>();
        public static Dictionary<int, SensorObj> SensorData = new Dictionary<int,SensorObj>();

        public static Dictionary<string, ChartCircularBuffer> SensorTrends = new Dictionary<string, ChartCircularBuffer>();

        public static IniData IncData = null;
        
        public static string NumberFormat(SENSOR_TYPE sensorType, string unit, double value)
        {
            string valstr = "?";

            switch (sensorType)
            {
                case SENSOR_TYPE.SENSOR_TYPE_VOLT:
                    valstr = value.ToString("N3");
                    break;
                case SENSOR_TYPE.SENSOR_TYPE_CURRENT:
                    valstr = value.ToString("N3");
                    break;
                case SENSOR_TYPE.SENSOR_TYPE_POWER:

                    if (unit == "W")
                    {
                        valstr = value.ToString("N1");
                    }
                    else
                    {
                        valstr = value.ToString("N3");
                    }
                    
                    break;

                case SENSOR_TYPE.SENSOR_TYPE_CLOCK:
                    valstr = value.ToString("N1");
                    break;
                case SENSOR_TYPE.SENSOR_TYPE_USAGE:
                    valstr = value.ToString("N1");
                    break;
                case SENSOR_TYPE.SENSOR_TYPE_TEMP:
                    valstr = value.ToString("N1");
                    break;

                case SENSOR_TYPE.SENSOR_TYPE_FAN:
                    valstr = value.ToString("N0");
                    break;

                case SENSOR_TYPE.SENSOR_TYPE_OTHER:

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

                case SENSOR_TYPE.SENSOR_TYPE_NONE:
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
                    var mmf = MemoryMappedFile.OpenExisting(HWINFO_SHARED_MEM_FILE_NAME,
                        MemoryMappedFileRights.Read);
                    var accessor = mmf.CreateViewAccessor(0L, Marshal.SizeOf(typeof(_HWiNFO_SHARED_MEM)),
                        MemoryMappedFileAccess.Read);

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
            for (var index = 0; index < hWiNFOMemory.NumSensorElements; ++index)
            {
                using (var viewStream = mmf.CreateViewStream(hWiNFOMemory.OffsetOfSensorSection + index * hWiNFOMemory.SizeOfSensorElement, hWiNFOMemory.SizeOfSensorElement, MemoryMappedFileAccess.Read))
                {
                    var buffer = new byte[(int)hWiNFOMemory.SizeOfSensorElement];
                    viewStream.Read(buffer, 0, (int)hWiNFOMemory.SizeOfSensorElement);
                    var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    var structure = (_HWiNFO_SENSOR)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(_HWiNFO_SENSOR));
                    gcHandle.Free();
                    
                    if (!FullSensorData.ContainsKey(index))
                    {
                        //var sensorNameOrig = Encoding.GetEncoding(1252).GetString(structure.SensorNameOrig).TrimEnd((char)0);
                        var sensorNameOrig = Encoding.UTF8.GetString(structure.SensorNameOrig).TrimEnd((char)0);

                        var sensorName = ""; 

                        if (hWiNFOMemory.Version > 1)
                        {
                            sensorName = Encoding.UTF8.GetString(structure.UtfSensorNameUser).TrimEnd((char)0);
                        }
                        else
                        {
                            //sensorName = Encoding.GetEncoding(1252).GetString(structure.SensorNameUser).TrimEnd((char)0);
                            sensorName = Encoding.UTF8.GetString(structure.SensorNameUser).TrimEnd((char)0);
                        }

                        var sensor = new SensorObj
                        {
                            SensorId = structure.SensorId,
                            SensorInstance = structure.SensorInstance,
                            SensorNameOrig = sensorNameOrig,
                            SensorNameUser = sensorName,
                            Elements = new Dictionary<string, ElementObj>()
                        };

                        FullSensorData.Add(index,sensor);
                    }
                    
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

                    var sensor = FullSensorData[(int) structure.SensorIndex];

                    var elementKey = sensor.SensorId + "-" + sensor.SensorInstance + "-" + structure.ElementId;

                    //var labelOrig = Encoding.GetEncoding(1252).GetString(structure.LabelOrig).TrimEnd((char)0);
                    var labelOrig = Encoding.UTF8.GetString(structure.LabelOrig).TrimEnd((char)0);

                    var unit = "";

                    if (hWiNFOMemory.Version > 1)
                    {
                        unit = Encoding.UTF8.GetString(structure.UtfUnit).TrimEnd((char)0); 
                    } else
                    {
                        //unit = Encoding.GetEncoding(1252).GetString(structure.Unit).TrimEnd((char)0);
                        unit = Encoding.UTF8.GetString(structure.Unit).TrimEnd((char)0);
                    }

                    var label = "?";

                    if (hWiNFOMemory.Version > 1)
                    {
                        label = Encoding.UTF8.GetString(structure.UtfLabelUser).TrimEnd((char)0); 
                    }
                    else
                    {
                        //label = System.Text.Encoding.GetEncoding(1252).GetString(structure.LabelUser).TrimEnd((char)0);
                        label = System.Text.Encoding.UTF8.GetString(structure.LabelUser).TrimEnd((char)0);
                    }

                    var element = new ElementObj
                    {
                        ElementKey = elementKey,

                        SensorType = structure.SensorType,
                        ElementId = structure.ElementId,
                        LabelOrig = labelOrig,
                        LabelUser = label,
                        Unit = unit,
                        NumericValue = (float)structure.Value,
                        Value = NumberFormat(structure.SensorType, unit, structure.Value),
                        ValueMin = NumberFormat(structure.SensorType, unit, structure.ValueMin),
                        ValueMax = NumberFormat(structure.SensorType, unit, structure.ValueMax),
                        ValueAvg = NumberFormat(structure.SensorType, unit,structure.ValueAvg)
                    };

                    sensor.Elements[elementKey] = element;
                }
            }
        }

        private static void ParseIncFile()
        {
            if (IncData != null && FullSensorData.Any())
            {
                var serverName = Environment.MachineName;

                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };


                int index = -1;

                foreach (var section in IncData.Sections.Where(x => x.SectionName != "Variables"))
                {
                    index++;

                    var sectionName = Regex.Replace(section.SectionName, "HWINFO-CONFIG-", "", RegexOptions.IgnoreCase);

                    foreach (KeyData key in section.Keys)
                    {
                        var elementName = key.Value;

                        var sensorIdStr = IncData["Variables"][key.KeyName + "-SensorId"];
                        var sensorInstanceStr = IncData["Variables"][key.KeyName + "-SensorInstance"];
                        var elementIdStr = IncData["Variables"][key.KeyName + "-EntryId"];

                        if (sensorIdStr?.StartsWith("0x") == true && sensorInstanceStr?.StartsWith("0x") == true &&
                            elementIdStr?.StartsWith("0x") == true)
                        {
                            var sensorId = Convert.ToUInt32(sensorIdStr.Replace("0x", ""), 16);
                            var sensorInstance = Convert.ToUInt32(sensorInstanceStr.Replace("0x", ""), 16);
                            var elementId = Convert.ToUInt32(elementIdStr.Replace("0x", ""), 16);

                            var fullSensorDataSensor = FullSensorData.Values.FirstOrDefault(x =>
                                x.SensorId == sensorId && x.SensorInstance == sensorInstance);

                            var elementKey = sensorId + "-" + sensorInstance + "-" + elementId;

                            if (fullSensorDataSensor?.Elements.ContainsKey(elementKey) == true)
                            {
                                var fullSensorDataElement = fullSensorDataSensor.Elements[elementKey];

                                var element = new ElementObj
                                {
                                    ElementKey = elementKey,

                                    SensorType = fullSensorDataElement.SensorType,
                                    ElementId = fullSensorDataElement.ElementId,
                                    LabelOrig = elementName,
                                    LabelUser = elementName,
                                    Unit = fullSensorDataElement.Unit,
                                    NumericValue = fullSensorDataElement.NumericValue,
                                    Value = fullSensorDataElement.Value,
                                    ValueMin = fullSensorDataElement.ValueMin,
                                    ValueMax = fullSensorDataElement.ValueMax,
                                    ValueAvg = fullSensorDataElement.ValueAvg
                                };

                                if (!SensorData.ContainsKey(index))
                                {
                                    var sensor = new SensorObj
                                    {
                                        SensorId = 0,
                                        SensorInstance = 0,
                                        SensorNameOrig = sectionName,
                                        SensorNameUser = sectionName,
                                        Elements = new Dictionary<string, ElementObj>()
                                    };

                                    SensorData.Add(index, sensor);
                                }
                                
                                SensorData[index].Elements[elementKey] = element;

                                var t1 = serverName.Replace(' ', '_');
                                var t2 = SensorData[index].SensorNameUser.Replace(' ', '_');
                                var t3 = element.LabelUser.Replace(' ', '_');

                                var task = Task.Run<bool>(async () => await MQTT.Publish($"HWINFO/{t1}/{t2}/{t3}", 
                                    JsonConvert.SerializeObject(new MQTTObj
                                        {
                                            Value = element.NumericValue,
                                            Unit = element.Unit
                                        }, 
                                        new JsonSerializerSettings
                                        {
                                            ContractResolver = contractResolver
                                        })));

                                if (!SensorTrends.ContainsKey(elementKey))
                                {
                                    SensorTrends.Add(elementKey, new ChartCircularBuffer(fullSensorDataElement.SensorType, fullSensorDataElement.Unit));
                                }

                                SensorTrends[elementKey].Put(fullSensorDataElement.NumericValue);

                            }
                        }

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
