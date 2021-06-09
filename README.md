# fip-elite
Information Display for Logitech Flight Instrument Panel and for VR for Elite Dangerous

You don't need a Flight Instrument Panel, if you only want to use this for VR.

![Logitech Flight Instrument Panel with Elgato Stream Deck](https://i.imgur.com/bE2ODlF.jpg)

![VR](https://i.imgur.com/qhICACC.jpg)

The menu area and each screen can be configured via razor (.cshtml) web page templates.

Use the right rotary encoder to scroll vertically on all tabs.

Use the left rotary encoder to show another card on various tabs or zoom into the galaxy map.
Also, the S5 button shows the next card and the S6 button shows the previous card.

Press the S1 button to display the menu.

You can also control ONE Flight Instrument Panel with a (virtual) Joystick 4-way hat switch with pushbutton.

The 4-way hat switch up-, down-, left-, right- buttons are 4 normal joystick buttons. 
An 8-way hat switch (POV) is not supported.

You can also designate separate (virtual) joystick buttons as shortcuts to specific screens.

The (virtual) joystick is configured via joysticksettings.config
The button id's must be numeric.

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
  <add key="NavigationButton" value="0" />
  <add key="TargetButton" value="0" />
  <add key="CommanderButton" value="0" />
  <add key="GalnetButton" value="0" />
  <add key="MissionsButton" value="0" />
  <add key="ChatButton" value="0" />
  <add key="HWInfoButton" value="0" />
  <add key="ShipButton" value="0" />
  <add key="MaterialsButton" value="0" />
  <add key="CargoButton" value="0" />
  <add key="EngineerButton" value="0" />
  <add key="ShipLockerButton" value="0" />
  <add key="BackPackButton" value="0" />
  <add key="POIButton" value="0" />
  <add key="GalaxyButton" value="0" />
  <add key="EngineersButton" value="0" />
  <add key="PowersButton" value="0" />
  <add key="MiningButton" value="0" />
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

The 'Engineer' tab is integrated with the material shopping list of the [EDEngineer](https://github.com/msarilar/EDEngineer) application.

The local api must be active in EDEngineer and listening on port 44405

**This is optional, EDEngineer doesn't have to be installed or running.**

Any data from [HWInfo](https://www.hwinfo.com) can be displayed. **This also works when Elite Dangerous is not running.**

When HWInfo64 is detected, all the available sensors will be written at startup to the data\hwinfo.json file.

The HWINFO.inc file must be modified, to configure what will be displayed on the screen.
The HWINFO.inc file has the same format as used by various [rainmeter](https://www.deviantart.com/pul53dr1v3r/art/Rainformer-2-9-3-HWiNFO-Edition-Rainmeter-789616481) skins.

Note that you don't need to install rainmeter or any rainmeter plugin.

A configuration tool, to link sensor ids to variables in the HWINFO.inc file, can be downloaded from the hwinfo website [here](https://www.hwinfo.com/beta/HWiNFOSharedMemoryViewer.exe.7z) :

![hwinfo tool](https://i.imgur.com/Px6jvw4.png)

The HWINFO sensor data can optionally be sent to an MQTT server, by creating a file called mqtt.config (this file doesn't exist by default)

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <mqtt>
    <add key="mqttURI" value="192.168.2.34" />
    <add key="mqttUser" value="mqttusername" />
    <add key="mqttPassword" value="secretpassword" />
    <add key="mqttPort" value="1883" />
    <add key="mqttSecure" value="False" />
  </mqtt>
</configuration>
```

![MQTT](https://i.imgur.com/X8IkHPg.png)

You can automatically open the 'Target' tab on ONE Flight Instrument Panel, when a ship is targeted. (ShipTargeted event).

You can automatically open the 'Navigation' tab on ONE Flight Instrument Panel, when a ship enters a system, approaches a station or a planet.
(ApproachBody, ApproachSettlement, DockingRequested, DockingGranted, Docked, CarrierJump, FSDJump, SupercruiseExit events)

Configure the serial number of the Flight Instrument Panel, that needs these features enabled, via panelsettings.config. 

(The special value "window" will also work.)

```
<?xml version="1.0" encoding="utf-8" ?>
<panelSettings>
  <add key="AutoActivateTarget" value="MZE09FE2BC" />
  <add key="AutoActivateNavigation" value="MZAAFAA883" />
</panelSettings>
```

A sound is played when menu options are selected.
This sound can be changed or disabled by editing the 'clickSound' key in in appsettings.config

![Screenshot 1](https://i.imgur.com/KA0iCcj.png)
![Screenshot 2](https://i.imgur.com/JTxiIBL.png)
![Screenshot 3](https://i.imgur.com/uXpUC6m.png)
![Screenshot 4](https://i.imgur.com/Fk62MmG.png)
![Screenshot 5](https://i.imgur.com/4HHcLjJ.png)
![Screenshot 6](https://i.imgur.com/5mlPp2I.png)
![Screenshot 7](https://i.imgur.com/dydIf16.png)
![Screenshot 8](https://i.imgur.com/16pc2zo.png)
![Screenshot 9](https://i.imgur.com/Cgqdic6.png)
![Screenshot 10](https://i.imgur.com/WJHBVIX.png)
![Screenshot 11](https://i.imgur.com/SaMQ2H2.png)
![Screenshot 12](https://i.imgur.com/X5kL0fn.png)
![Screenshot 13](https://i.imgur.com/WepiQbs.png)
![Screenshot 14](https://i.imgur.com/ZOcUfyI.png)
![Screenshot 15](https://i.imgur.com/LyfdMTh.png)
![Screenshot 16](https://i.imgur.com/iqZVk2Y.png)
![Screenshot 17](https://i.imgur.com/ofaRPKm.png)
![Screenshot 18](https://i.imgur.com/zm3Xrm9.png)
![Screenshot 19](https://i.imgur.com/p8gW2Fr.png)
![Screenshot 20](https://i.imgur.com/QoBYgCT.png)
![Screenshot 21](https://i.imgur.com/zGm6qOR.png)
![Screenshot 22](https://i.imgur.com/ncHyT8X.png)
![Screenshot 23](https://i.imgur.com/1ngN8cF.png)
![Screenshot 24](https://i.imgur.com/4gUny6G.png)
![Screenshot 25](https://i.imgur.com/W67Nci1.png)
![Screenshot 26](https://i.imgur.com/z4ACs0q.png)
![Screenshot 27](https://i.imgur.com/oXVakhB.png)
![Screenshot 28](https://i.imgur.com/zR9ye3a.png)
![Screenshot 29](https://i.imgur.com/U8aI2LT.png)
![Screenshot 30](https://i.imgur.com/FuOCfiI.png)
![Screenshot 31](https://i.imgur.com/fUiZ5nZ.png)
![Screenshot 32](https://i.imgur.com/TanDFUm.png)
![Screenshot 33](https://i.imgur.com/QXoqTyk.png)

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

https://www.hwinfo.com/

DaftMav for POI list [see here](https://www.reddit.com/r/EliteDangerous/comments/9mfiug/edison_a_tool_which_helps_getting_to_planet/)

https://eddb.io/ and https://www.edsm.net/ for station, system and body data

https://inara.cz/ for pricing data

https://www.edsm.net/ for the galaxy image

https://edassets.org/ CMDR Qohen Leth and CMDR Nuse for the ship images

http://edtools.ddns.net/
