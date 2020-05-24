# fip-elite
Information Display for Logitech Flight Instrument Panel and for VR for Elite Dangerous

You don't need a Flight Instrument Panel, if you only want to use this for VR.

![Logitech Flight Instrument Panel with Elgato Stream Deck](https://i.imgur.com/bE2ODlF.jpg)

![VR](https://i.imgur.com/qhICACC.jpg)

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
  <add key="WindowWidth" value="320" />
  <add key="WindowHeight" value="240" />
</joystickSettings>
```

There is a Toggle Window menu option in the tray icon context menu.
This option mirrors the FIP display, that is being controlled by the HOTAS hat switch, to a window for use in VR.

If FipSerialNumber in joysticksettings.config has the special value "window", then a separate window is created, 
that is NOT a mirror of a FIP display. This window can be resized.

**This window will also work WITHOUT any connected FIP display.**

WindowWidth/WindowHeight in joysticksettings.config can only be adjusted if the window is NOT a mirror of a FIP display. 
Otherwise these values are ignored.

You can then use a tool like [OVR Toolkit](https://store.steampowered.com/app/1068820/OVR_Toolkit/) to display this window in VR. 

The 'Engineer' tab is integrated with the material shopping list of the [EDengineer](https://github.com/msarilar/EDEngineer) application.

The local api must be active in EDengineer and listening on port 44405

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
![Screenshot 12](https://i.imgur.com/LyfdMTh.png)
![Screenshot 13](https://i.imgur.com/iqZVk2Y.png)
![Screenshot 14](https://i.imgur.com/ofaRPKm.png)
![Screenshot 15](https://i.imgur.com/zm3Xrm9.png)
![Screenshot 16](https://i.imgur.com/wFsMD4t.png)
![Screenshot 17](https://i.imgur.com/zGm6qOR.png)
![Screenshot 18](https://i.imgur.com/ncHyT8X.png)
![Screenshot 19](https://i.imgur.com/1ngN8cF.png)

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

https://github.com/msarilar/EDEngineer

DaftMav for POI list [see here](https://www.reddit.com/r/EliteDangerous/comments/9mfiug/edison_a_tool_which_helps_getting_to_planet/)

https://eddb.io/ and https://www.edsm.net/ for station and system data

https://www.edsm.net/ for the galaxy image

https://edassets.org/ CMDR Qohen Leth and CMDR Nuse for the ship images

http://edtools.ddns.net/
