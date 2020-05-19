# fip-elite
Logitech Flight Instrument Panel and VR application for Elite Dangerous

![Logitech Flight Instrument Panel with Elgato Stream Deck](https://i.imgur.com/bE2ODlF.jpg)

This application displays data from Elite Dangerous on Logitech Flight Instrument Panels or in a window for VR.

The menu area and each screen can be configured via razor (.cshtml) web page templates.

Use the right rotary encoder to scroll vertically on all tabs.

Use the left rotary encoder to show another card on various tabs or zoom into the galaxy map.

Use S1 to display the menu.

You can also control ONE Flight Instrument Panel with a HOTAS 4-way hat switch with pushbutton.
The joystick is configured via joysticksettings.config

```
<?xml version="1.0" encoding="utf-8" ?>
<joystickSettings>
  <add key="PID" value="0127" />
  <add key="VID" value="231D" />
  <add key="UpButton" value="21" />
  <add key="DownButton" value="23" />
  <add key="LeftButton" value="24" />
  <add key="RightButton" value="22" />
  <add key="PushButton" value="25" />
  <add key="FipSerialNumber" value="MZAAFAA883" />
</joystickSettings>
```

There is a Toggle Mirror Window menu option in the tray icon context menu.
This option mirrors the FIP display, that is being controlled by the HOTAS hat switch, to a window for use in VR, 
in combination with the OVR Toolkit application on steam.

If FipSerialNumber in joysticksettings.config has the special value "window", then a separate window is created 
that is NOT a mirror of a FIP display. 

So this window also works WITHOUT any FIP display.

You can then use a tool like [OVR Toolkit](https://store.steampowered.com/app/1068820/OVR_Toolkit/) to display this window in VR. 

![Screenshot 1](https://i.imgur.com/KA0iCcj.png)
![Screenshot 2](https://i.imgur.com/JTxiIBL.png)
![Screenshot 3](https://i.imgur.com/uXpUC6m.png)
![Screenshot 4](https://i.imgur.com/Fk62MmG.png)
![Screenshot 5](https://i.imgur.com/iJnHuOV.png)
![Screenshot 6](https://i.imgur.com/16pc2zo.png)
![Screenshot 7](https://i.imgur.com/Cgqdic6.png)
![Screenshot 8](https://i.imgur.com/WJHBVIX.png)
![Screenshot 9](https://i.imgur.com/SaMQ2H2.png)
![Screenshot 10](https://i.imgur.com/X5kL0fn.png)
![Screenshot 11](https://i.imgur.com/ZOcUfyI.png)
![Screenshot 12](https://i.imgur.com/v8aUFMT.png)
![Screenshot 13](https://i.imgur.com/iqZVk2Y.png)
![Screenshot 14](https://i.imgur.com/ofaRPKm.png)
![Screenshot 15](https://i.imgur.com/zm3Xrm9.png)
![Screenshot 16](https://i.imgur.com/wFsMD4t.png)

Works with these 64 bit Logitech Flight Instrument Panel Drivers (currently not with older saitek drivers) :

https://support.logi.com/hc/en-us/articles/360024848713--Downloads-Flight-Instrument-Panel

Software Version: 8.0.134.0
Last Update: 2018-01-05
64-bit

https://download01.logi.com/web/ftp/pub/techsupport/simulation/Flight_Instrument_Panel_x64_Drivers_8.0.134.0.exe

Also see companion plugin for Elgato stream deck :

https://github.com/mhwlng/streamdeck-elite

Thanks to :

https://github.com/EDCD/EDDI

https://github.com/MagicMau/EliteJournalReader

https://github.com/Filtik/EliteDangerousLCD

https://github.com/jdahlblom/DCSFIPS

DaftMav for POI list [see here](https://www.reddit.com/r/EliteDangerous/comments/9mfiug/edison_a_tool_which_helps_getting_to_planet/)

https://eddb.io/ and https://www.edsm.net/ for station and system data

https://www.edsm.net/ for the galaxy image

https://edassets.org/ CMDR Qohen Leth and CMDR Nuse for the ship images

http://edtools.ddns.net/
