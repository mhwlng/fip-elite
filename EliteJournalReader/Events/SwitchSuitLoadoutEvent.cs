using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //The "SwitchSuitLoadout" event contains:

    /*
     {"timestamp":"2021-03-31T10:31:46Z","event":"SwitchSuitLoadout",
    "LoadoutID":4293000003,
    "LoadoutName":"Rocket Launcher with Kinetic Side Arm"}
     */
    /*
     { "timestamp":"2021-07-03T08:24:12Z", "event":"SwitchSuitLoadout", "SuitID":1700273897771741, 
    "SuitName":"tacticalsuit_class1", 
    "SuitName_Localised":"Dominator Suit",
    "SuitMods":[  ],
    "LoadoutID":4293000001, 
    "LoadoutName":"dominator", 
    "Modules":[ { "SlotName":"PrimaryWeapon1", "SuitModuleID":1703077474370671, "ModuleName":"wpn_m_sniper_plasma_charged", "ModuleName_Localised":"Manticore Executioner", 
    "Class":2, "WeaponMods":[  ] }, 
    { "SlotName":"SecondaryWeapon", "SuitModuleID":1703092103309717, "ModuleName":"wpn_s_pistol_plasma_charged", "ModuleName_Localised":"Manticore Tormentor", "Class":2, "WeaponMods":[  ] } ] }

{ "timestamp":"2021-07-03T08:24:23Z", "event":"SwitchSuitLoadout", "SuitID":1700273889389298, "SuitName":"utilitysuit_class2", "SuitName_Localised":"$UtilitySuit_Class1_Name;", "SuitMods":[  ],
    "LoadoutID":4293000003, "LoadoutName":"ENGINEER", "Modules":[ { "SlotName":"PrimaryWeapon1", "SuitModuleID":1703077474370671, "ModuleName":"wpn_m_sniper_plasma_charged", "ModuleName_Localised":"Manticore Executioner", "Class":2, "WeaponMods":[  ] }, { "SlotName":"SecondaryWeapon", "SuitModuleID":1703092103309717, "ModuleName":"wpn_s_pistol_plasma_charged",
    "ModuleName_Localised":"Manticore Tormentor", "Class":2, "WeaponMods":[  ] } ] }

     */


    public class SwitchSuitLoadoutEvent : JournalEvent<SwitchSuitLoadoutEvent.SwitchSuitLoadoutEventArgs>
    {
        public SwitchSuitLoadoutEvent() : base("SwitchSuitLoadout") { }

        public class SwitchSuitLoadoutEventArgs : JournalEventArgs
        {
            public class Module
            {
                public string SlotName { get; set; }
                public string SuitModuleID { get; set; }
                public string ModuleName { get; set; }
                public string ModuleName_Localised { get; set; }

                public int Class { get; set; }
            }

            public string SuitID { get; set; }
            public string SuitName { get; set; }
            public string LoadoutID { get; set; }
            public string LoadoutName { get; set; }

            public Module[] Modules { get; set; }
        }
    }
}
