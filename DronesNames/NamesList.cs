using System.Collections.Generic;
using System.Linq;

namespace DronesNames
{
    public class NamesList
    {
        public static Xoroshiro128Plus rng = new Xoroshiro128Plus(0);

        public static Dictionary<string, Dictionary<string, string>> namesByBodyNames = new Dictionary<string, Dictionary<string, string>>()
        {
            { "EmergencyDroneBody",       new Dictionary<string, string>() },       // (Emergency drone)
            { "Turret1Body",              new Dictionary<string, string>() },       // (Gunner turret)
            { "Drone1Body",               new Dictionary<string, string>() },       // (Gunner drone)
            { "Drone2Body",               new Dictionary<string, string>() },       // (Healing drone)
            { "FlameDroneBody",           new Dictionary<string, string>() },       // (Incinerator drone)
            { "MissileDroneBody",         new Dictionary<string, string>() },       // (Missile drone)
            { "EquipmentDroneBody",       new Dictionary<string, string>() },       // (Equipment drone)
            { "MegaDroneBody",            new Dictionary<string, string>() },       // (TC-280)
            { "DroneCommanderBody",       new Dictionary<string, string>() },       // (Col. Droneman)
            { "BackupDroneBody",          new Dictionary<string, string>() },       // (Strike drone)
            { "BeetleGuardAllyBody",      new Dictionary<string, string>() },       // (Beetle guard)
            { "SquidTurretBody",          new Dictionary<string, string>() },       // (Squid polyp)
            { "RoboBallRedBuddyBody",     new Dictionary<string, string>() },       // (Empathy core 1)
            { "RoboBallGreenBuddyBody",   new Dictionary<string, string>() },       // (Empathy core 2)
            { "MinorConstructOnKillBody", new Dictionary<string, string>() }        // (Defense nucleus)
        };

        public static void BuildNamesByBodyName()
        {
            foreach (var bodyName in namesByBodyNames.Keys)
            {
                foreach (var possibleIndexes in names.Keys)
                {
                    var addNames = false;
                    
                    foreach (var possibleIndex in possibleIndexes)
                    {
                        var possibleBodyIndex = possibleIndex;
                        
                        if (possibleIndex == "Default" || possibleBodyIndex == bodyName)
                        {
                            addNames = true;
                            break;
                        }
                    }

                    // Concat namesByBodyIndex with names
                    if (addNames)
                    {
                        foreach (var name in names[possibleIndexes])
                        {
                            namesByBodyNames[bodyName].Add(name.Key, name.Value);
                        }
                    }
                }
            }

        }

        public static string GetRandomNameForCharacterMaster(RoR2.CharacterMaster characterMaster, RoR2.CharacterBody characterBody)
        {
            // Get network ID of character body
            if (characterMaster == null || characterMaster.networkIdentity == null || characterMaster.networkIdentity.netId == null)
            {
                return "";
            }

            var netIdValue = characterMaster.networkIdentity.netId.Value;
            
            // If already saved in the tokens list, just return that
            if (DronesNames.savedTokens.ContainsKey(netIdValue))
            {
                Log.LogDebug("Found token " + DronesNames.savedTokens[netIdValue] + " at ID " + netIdValue);
                return DronesNames.savedTokens[netIdValue];
            }

            var bodyIndex = characterBody.bodyIndex;
            var bodyName = RoR2.BodyCatalog.GetBodyName(bodyIndex);

            // If it's not in the list of names, skip
            if (!namesByBodyNames.ContainsKey(bodyName))
            {
                return "";
            }

            // Get the names dictionary <token,name>. If it's empty, skip
            var namesDictionary = namesByBodyNames[bodyName];
            if (namesDictionary.Count == 0)
            {
                return "";
            }

            // Set seed of rng and draw a token
            rng.ResetSeed(netIdValue);
            var randomIndex = rng.RangeInt(0, namesDictionary.Count);
            string randomToken = namesDictionary.ElementAt(randomIndex).Key;

            Log.LogDebug("RNG seed is " + netIdValue + ", returning token " + randomToken + " at index " + randomIndex);

            // Add to the saved tokens
            DronesNames.savedTokens.Add(netIdValue, randomToken);

            return randomToken;
        }


        public static Dictionary<string[],Dictionary<string,string>> names = new Dictionary<string[], Dictionary<string, string>>
        {
            { new string[1] { "Default" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_DEFAULT", "Default" },
                    { "DRONESNAMES_CHRISTOPHER", "Christopher" },
                    { "DRONESNAMES_JESSICA", "Jessica" },
                    { "DRONESNAMES_MATTHEW", "Matthew" },
                    { "DRONESNAMES_ASHLEY", "Ashley" },
                    { "DRONESNAMES_JENNIFER", "Jennifer" },
                    { "DRONESNAMES_JOSHUA", "Joshua" },
                    { "DRONESNAMES_AMANDA", "Amanda" },
                    { "DRONESNAMES_DANIEL", "Daniel" },
                    { "DRONESNAMES_DAVID", "David" },
                    { "DRONESNAMES_JAMES", "James" },
                    { "DRONESNAMES_ROBERT", "Robert" },
                    { "DRONESNAMES_JOHN", "John" },
                    { "DRONESNAMES_JOSEPH", "Joseph" },
                    { "DRONESNAMES_ANDREW", "Andrew" },
                    { "DRONESNAMES_RYAN", "Ryan" },
                    { "DRONESNAMES_BRANDON", "Brandon" },
                    { "DRONESNAMES_JASON", "Jason" },
                    { "DRONESNAMES_JUSTIN", "Justin" },
                    { "DRONESNAMES_SARAH", "Sarah" },
                    { "DRONESNAMES_WILLIAM", "William" },
                    { "DRONESNAMES_JONATHAN", "Jonathan" },
                    { "DRONESNAMES_STEPHANIE", "Stephanie" },
                    { "DRONESNAMES_BRIAN", "Brian" },
                    { "DRONESNAMES_NICOLE", "Nicole" },
                    { "DRONESNAMES_NICHOLAS", "Nicholas" },
                    { "DRONESNAMES_ANTHONY", "Anthony" },
                    { "DRONESNAMES_HEATHER", "Heather" },
                    { "DRONESNAMES_ERIC", "Eric" },
                    { "DRONESNAMES_ELIZABETH", "Elizabeth" },
                    { "DRONESNAMES_JPEG", ".jpeg" },
                    { "DRONESNAMES_MICHAEL", "Michael" },
                    { "DRONESNAMES_MICROWAVE", "Microwave" },
                    { "DRONESNAMES_NEWFOLDER1", "New Folder (1)" },
                    { "DRONESNAMES_HOTDOGCOM", "Hotdog.com" },
                    { "DRONESNAMES_BUDDY", "Buddy" },
                    { "DRONESNAMES_ANESTHESIA", "Anesthesia" },
                    { "DRONESNAMES_32BIT", "32-bit" },
                    { "DRONESNAMES_PNG", ".png" },
                    { "DRONESNAMES_CHILD2", "Child 2.0" },
                    { "DRONESNAMES_AMPIE3", "Ampie 3" },
                    { "DRONESNAMES_BEEP", "Beep" },
                    { "DRONESNAMES_BOLT", "Bolt" },
                    { "DRONESNAMES_THEKILLER", "The Killer ^_^" },
                    { "DRONESNAMES_HI", "1001000 1001001" },                  
                }
            },

            { new string[1] { "EmergencyDroneBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_EMERGENCYDRONEBODY", "EmergencyDroneBody" },
                }
            },

            { new string[1] { "Turret1Body" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_TURRET1BODY", "Turret1Body" },
                    { "DRONESNAMES_MYBESTFRIEND", "My Best Friend" },
                }
            },

            { new string[1] { "Drone1Body" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_DRONE1BODY", "Drone1Body" },
                }
            },

            { new string[1] { "Drone2Body" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_DRONE2BODY", "Drone2Body" },
                }
            },

            { new string[1] { "FlameDroneBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_FLAMEDRONEBODY", "FlameDroneBody" },
                }
            },

            { new string[1] { "MissileDroneBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_MISSILEDRONEBODY", "MissileDroneBody" },
                }
            },

            { new string[1] { "EquipmentDroneBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_EQUIPMENTDRONEBODY", "EquipmentDroneBody" },
                }
            },

            { new string[1] { "MegaDroneBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_MEGADRONEBODY", "MegaDroneBody" },
                }
            },

            { new string[1] { "DroneCommanderBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_DRONECOMMANDERBODY", "DroneCommanderBody" },
                }
            },

            { new string[1] { "BackupDroneBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_BACKUPDRONEBODY", "BackupDroneBody" },
                }
            },

            { new string[1] { "BeetleGuardAllyBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_BEETLEGUARDALLYBODY", "BeetleGuardAllyBody" },
                }
            },

            { new string[1] { "SquidTurretBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_SQUIDTURRETBODY", "SquidTurretBody" },
                }
            },

            { new string[1] { "RoboBallRedBuddyBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_YESILOVEYOUTOO", "YES I LOVE YOU TOO" },
                }
            },

            { new string[1] { "RoboBallGreenBuddyBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_DOYOULOVETHEMTOO", "DO YOU LOVE THEM TOO" },
                }
            },

            { new string[1] { "MinorConstructOnKillBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_MINORCONSTRUCTONKILLBODY", "MinorConstructOnKillBody" },
                }
            },
        };

    }
}
