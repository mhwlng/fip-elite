using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using Microsoft.Win32;
using Somfic.Logging;

namespace Elite
{
    class FipHandler
    {
        private List<FipPanel> _fipPanels = new List<FipPanel>();

        private DirectOutputClass.DeviceCallback _deviceCallback;
        private DirectOutputClass.EnumerateCallback _enumerateCallback;
        private bool _initOk;

        //private const String directOutputKey = "SOFTWARE\\Saitek\\DirectOutput";

        private const String directOutputKey = "SOFTWARE\\Logitech\\DirectOutput"; 

        public bool Initialize()
        {
            _initOk = false;
            try
            {
                _deviceCallback = DeviceCallback;
                _enumerateCallback = EnumerateCallback;

                RegistryKey key = Registry.LocalMachine.OpenSubKey(directOutputKey);

                var value = key?.GetValue("DirectOutput");
                if (value != null && (value is string))
                {

                    var retVal = DirectOutputClass.Initialize("ABC");
                    if (retVal != ReturnValues.S_OK)
                    {
                        App.log.Error("FIPHandler failed to init DirectOutputClass. " + retVal);
                        return false;
                    }

                    retVal = DirectOutputClass.RegisterDeviceCallback(_deviceCallback);

                    retVal = DirectOutputClass.Enumerate(_enumerateCallback);
                    if (retVal != ReturnValues.S_OK)
                    {
                        App.log.Error("FIPHandler failed to Enumerate DirectOutputClass. " + retVal);
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
                
                return false;
            }
            _initOk = true;
            return true;
        }

        public void Close()
        {
            try
            {
                foreach (var fipPanel in _fipPanels)
                {
                    fipPanel.Shutdown();
                }
                if (_initOk)
                {
                    //No need to deinit if init never worked. (e.g. missing Saitek Drivers)
                    DirectOutputClass.Deinitialize();
                    _initOk = false;
                }
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }
        }

        private bool IsFipDevice(IntPtr device)
        {
            var mGuid = Guid.Empty;

            DirectOutputClass.GetDeviceType(device, ref mGuid);

            return (string.Compare(mGuid.ToString(), "3E083CD8-6A37-4A58-80A8-3D6A2C07513E", true, CultureInfo.InvariantCulture) == 0);
        }

        private void EnumerateCallback(IntPtr device, IntPtr context)
        {
            try
            {
                var mGuid = Guid.Empty;

                DirectOutputClass.GetDeviceType(device, ref mGuid);

                App.log.Info($"Adding new DirectOutput device {device} of type: {mGuid.ToString()}");

                //Called initially when enumerating FIPs.

                if (!IsFipDevice(device))
                {
                    return;
                }
                var fipPanel = new FipPanel(device);
                _fipPanels.Add(fipPanel);
                fipPanel.Initalize();
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }
        }

        private void DeviceCallback(IntPtr device, bool added, IntPtr context)
        {
            try
            {
                //Called whenever a DirectOutput device is added or removed from the system.
                App.log.Info("DeviceCallback(): 0x" + device.ToString("x") + (added ? " Added" : " Removed"));

                if (!IsFipDevice(device))
                {
                    return;
                }

                if (!added && _fipPanels.Count == 0)
                {
                    return;
                }

                var i = _fipPanels.Count - 1;
                var found = false;
                do
                {
                    if (_fipPanels[i].FipDevicePointer == device)
                    {
                        found = true;
                        var fip = _fipPanels[i];
                        if (!added)
                        {
                            fip.Shutdown();
                            _fipPanels.Remove(fip);
                        }
                    }
                    i--;
                } while (i >= 0);

                if (added && !found)
                {
                    App.log.Info("DeviceCallback() Spawning FipPanel. " + device);
                    var fipPanel = new FipPanel(device);
                    _fipPanels.Add(fipPanel);
                    fipPanel.Initalize();
                }
            }
            catch (Exception ex)
            {
                App.log.Error(ex);
            }

        }

    }
}
