using System.Collections.Generic;
using System.Linq;

namespace DronesNames
{
    public struct NamesByBodyNames
    {
        public string bodyName { get; }
        public string[] additionalIndexes { get; }

        public Dictionary<string, string> names;

        public NamesByBodyNames(string bodyName, string[] additionalIndexes)
        {
            this.bodyName = bodyName;
            this.additionalIndexes = additionalIndexes;
            
            names = new Dictionary<string, string>();
        }
    }

    public class NamesList
    {
        public static Xoroshiro128Plus rng = new Xoroshiro128Plus(0);

        public static List<NamesByBodyNames> namesByBodyNames = new List<NamesByBodyNames>()
        {
            { new NamesByBodyNames("EmergencyDroneBody",          new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("Turret1Body",                 new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("Drone1Body",                  new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("Drone2Body",                  new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("FlameDroneBody",              new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("MissileDroneBody",            new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("EquipmentDroneBody",          new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("BackupDroneBody",             new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("EngiTurretBody",              new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("EngiWalkerTurretBody",        new string[1] { "DefaultDrones" }     ) },
            { new NamesByBodyNames("MegaDroneBody",               new string[0] {}                      ) },
            { new NamesByBodyNames("DroneCommanderBody",          new string[0] {}                      ) },
            { new NamesByBodyNames("BeetleGuardAllyBody",         new string[0] {}                      ) },
            { new NamesByBodyNames("SquidTurretBody",             new string[0] {}                      ) },
            { new NamesByBodyNames("RoboBallRedBuddyBody",        new string[0] {}                      ) },
            { new NamesByBodyNames("RoboBallGreenBuddyBody",      new string[0] {}                      ) },
            { new NamesByBodyNames("MinorConstructOnKillBody",    new string[0] {}                      ) },
        };

        public static void BuildNamesByBodyName()
        {
            foreach (var namesByBodyName in namesByBodyNames)
            {
                foreach (var possibleIndexes in names.Keys)
                {
                    var addNames = false;
                    
                    foreach (var possibleIndex in possibleIndexes)
                    {                        
                        if (namesByBodyName.bodyName == possibleIndex || namesByBodyName.additionalIndexes.Contains(possibleIndex))
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
                            namesByBodyName.names.Add(name.Key, name.Value);
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
                if (DronesNames.LogDebug) Log.LogDebug("Found token " + DronesNames.savedTokens[netIdValue] + " at ID " + netIdValue);
                
                return DronesNames.savedTokens[netIdValue];
            }

            var bodyIndex = characterBody.bodyIndex;
            var bodyName = RoR2.BodyCatalog.GetBodyName(bodyIndex);
            var forcedIndex = -1;

            // SPECIAL CASE for Empathy Cores, where both Empathy Cores have linked names
            var storeEmpathyCoreIndex = false;
            if (bodyName == "RoboBallRedBuddyBody" || bodyName == "RoboBallGreenBuddyBody") 
            {
                var empathyCoresSyncedIndex = CheckForEmpathyCoresSync(characterMaster, bodyName);
                if (empathyCoresSyncedIndex != -1)
                {
                    forcedIndex = empathyCoresSyncedIndex;
                }
                else
                {
                    storeEmpathyCoreIndex = true;
                }
            }

            // If it's not in the list of names, skip
            var namesDictionary = new Dictionary<string, string>();
            var foundDictionary = false;
            foreach (var namesByBodyName in namesByBodyNames)
            {
                if (namesByBodyName.bodyName == bodyName)
                {
                    namesDictionary = namesByBodyName.names;
                    foundDictionary = true;
                    break;
                }
            }

            if (!foundDictionary) return "";

            // Get the names dictionary <token,name>. If it's empty, skip
            if (namesDictionary.Count == 0) return "";

            // Set seed of rng and draw a token
            rng.ResetSeed(netIdValue);
            var randomIndex = forcedIndex != -1 ? forcedIndex : rng.RangeInt(0, namesDictionary.Count);
            string randomToken = namesDictionary.ElementAt(randomIndex).Key;

            if (DronesNames.LogDebug)
            {
                if (forcedIndex == -1) 
                {
                    Log.LogDebug("Spawning " + bodyName + ", RNG seed is " + netIdValue + ", random index is " + randomIndex + ", returning token " + randomToken);
                }
                else
                {
                    Log.LogDebug("Spawning " + bodyName + ", forced index is " + forcedIndex + ", returning token " + randomToken);
                }
            }

            // Add to the saved tokens
            DronesNames.savedTokens.Add(netIdValue, randomToken);

            // Store for empathy cores sync
            if (storeEmpathyCoreIndex)
            {
                #pragma warning disable Publicizer001
                DronesNames.empathyCoresNameIndexes.Add(characterMaster.minionOwnership.ownerMasterId, randomIndex);
                #pragma warning restore Publicizer001
            }

            return randomToken;
        }

        private static int CheckForEmpathyCoresSync(RoR2.CharacterMaster characterMaster, string bodyName)
        {

            var empathyCoresSyncedToken = -1;

            #pragma warning disable Publicizer001
            var ownerId = characterMaster.minionOwnership.ownerMasterId;
            #pragma warning restore Publicizer001

            if (DronesNames.empathyCoresNameIndexes.ContainsKey(ownerId))
            {
                empathyCoresSyncedToken = DronesNames.empathyCoresNameIndexes[ownerId];
            }
            
            return empathyCoresSyncedToken;
        }


        public static Dictionary<string[],Dictionary<string,string>> names = new Dictionary<string[], Dictionary<string, string>>
        {
            { new string[1] { "DefaultDrones" }, new Dictionary<string,string>
                {
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
                    
                }
            },

            { new string[1] { "Turret1Body" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_USELESS", "Useless" },
                    { "DRONESNAMES_FLIGHTLESSDRONE", "Flightless Drone" },
                }
            },

            { new string[1] { "Drone1Body" }, new Dictionary<string,string>
                {

                }
            },

            { new string[1] { "Drone2Body" }, new Dictionary<string,string>
                {

                }
            },

            { new string[1] { "FlameDroneBody" }, new Dictionary<string,string>
                {

                }
            },

            { new string[1] { "MissileDroneBody" }, new Dictionary<string,string>
                {

                }
            },

            { new string[1] { "EquipmentDroneBody" }, new Dictionary<string,string>
                {

                }
            },

            { new string[1] { "MegaDroneBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_DESTRUCTION", "DESTRUCTION" },
                    { "DRONESNAMES_APOCALYPSE", "APOCALYPSE" },
                    { "DRONESNAMES_ARMAGEDDON", "ARMAGEDDON" },
                    { "DRONESNAMES_BADINVESTMENT", "BAD INVESTMENT" },
                }
            },

            { new string[1] { "DroneCommanderBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_COLONELBEEPBOOP", "Colonel Beep Boop" },
                }
            },

            { new string[1] { "BackupDroneBody" }, new Dictionary<string,string>
                {

                }
            },

            { new string[1] { "BeetleGuardAllyBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_SIRDIESALOT", "Sir Dies-A-Lot" },
                }
            },

            { new string[1] { "SquidTurretBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_PEARL", "Pearl" },
                    { "DRONESNAMES_MARINA", "Marina" },
                    { "DRONESNAMES_FRYE", "Frye" },
                    { "DRONESNAMES_SHIVER", "Shiver" },
                    { "DRONESNAMES_AGENT3", "Agent 3" },
                    { "DRONESNAMES_AGENT4", "Agent 4" },
                    { "DRONESNAMES_CALLIE", "Callie" },
                    { "DRONESNAMES_MARIE", "Marie" },
                    { "DRONESNAMES_FREESCRAP", "Free Scrap" },
                }
            },

            { new string[1] { "RoboBallRedBuddyBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_DOYOULOVETHEMTOO", "DO YOU LOVE THEM TOO" },
                    { "DRONESNAMES_REDANDLONELY", "RED AND LONELY" },
                    { "DRONESNAMES_COLD", "COLD" },
                }
            },

            { new string[1] { "RoboBallGreenBuddyBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_YESILOVEYOUTOO", "YES I LOVE YOU TOO" },
                    { "DRONESNAMES_GREENANDLONELY", "GREEN AND LONELY" },
                    { "DRONESNAMES_BOLD", "BOLD" },
                }
            },

            { new string[1] { "MinorConstructOnKillBody" }, new Dictionary<string,string>
                {
                    { "DRONESNAMES_TRIANGLE", "Triangle" },
                }
            },
        };

    }
}
