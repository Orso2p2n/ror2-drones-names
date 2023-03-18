using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using System;
using System.Text;

namespace DronesNames
{
    public struct NamesByBodyIndex
    {
        public string bodyIndex { get; }
        public string[] additionalCategories { get; }

        public Dictionary<string, string> names;

        public NamesByBodyIndex(string bodyIndex, string[] additionalCategories, Dictionary<string, string> names)
        {
            this.bodyIndex = bodyIndex;
            this.additionalCategories = additionalCategories;
            this.names = names;
        }
    }

    public struct ConfigBodyIndex
    {
        public ConfigEntry<string> configEntry { get; set; }

        public string bodyIndex { get; }
        public string realName { get; }
        public string categories { get; }
        public string names { get; }

        public ConfigBodyIndex(string bodyIndex, string realName, string categories, string names)
        {
            this.bodyIndex = bodyIndex;
            this.realName = realName;
            this.categories = categories;
            this.names = names;

            configEntry = DronesNames.instance.Config.Bind<string>(
                "Categories and names by Body Index",
                bodyIndex,

                "[" + categories + "][" + names + "]",

                realName + ". Format: \"[Category1,Category2,...][Name1,Name2,Name3,...]\""
            );
        }
    }

    public struct ConfigCategory
    {
        public ConfigEntry<string> configEntry { get; set; }

        public string category { get; }
        public string names { get; }

        public ConfigCategory(string category, string names)
        {
            this.category = category;
            this.names = names;

            configEntry = DronesNames.instance.Config.Bind<string>(
                "Custom Categories",
                category,

                names,

                "Format: \"Name1,Name2,Name3,...\""
            );
        }
    }

    public class NamesList
    {
        public static Xoroshiro128Plus rng = new Xoroshiro128Plus(0);

        public static List<NamesByBodyIndex> namesByBodyIndexes = new List<NamesByBodyIndex>();
        public static void BuildNamesByBodyName()
        {
            // Build names by category
            Dictionary<string,string[]> namesByCategory = new Dictionary<string,string[]>();
            foreach (var configCategory in configCategories)
            {
                var namesArray = configCategory.configEntry.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                namesByCategory.Add(configCategory.category, namesArray);
            }

            // Build categories and names by BodyIndex
            foreach (var configBodyIndex in configBodyIndexes)
            {
                // Split config value by []
                var splitValue = configBodyIndex.configEntry.Value.Split(new string[] { "[","]" }, StringSplitOptions.RemoveEmptyEntries);

                // Get categories and split them by ,
                var categories = "";
                var categoriesArray = new string[0];
                if (splitValue.Length >= 1)
                {
                    categories = splitValue[0];
                    categoriesArray = categories.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                }

                // Get names and split them by ,
                var names = "";
                var namesArray = new string[0];
                if (splitValue.Length >= 2)
                {
                    names = splitValue[1];
                    namesArray = names.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                }

                // Get BodyIndex
                var bodyIndex = configBodyIndex.bodyIndex + "Body";
                
                // Build names dictionary
                // Add names from BodyIndex
                var namesDictionary = new Dictionary<string,string>();
                foreach (var name in namesArray)
                {
                    var token = TurnNameIntoToken(name);

                    if (!namesDictionary.ContainsKey(token))
                    {
                        namesDictionary.Add(token, name);
                    }
                }

                // Add names from Categories
                foreach (var category in categoriesArray)
                {
                    foreach (var name in namesByCategory[category])
                    {
                        var token = TurnNameIntoToken(name);

                        if (!namesDictionary.ContainsKey(token))
                        {
                            namesDictionary.Add(token, name);
                        }
                    }
                }

                var namesByBodyIndex = new NamesByBodyIndex(bodyIndex, categoriesArray, namesDictionary);
                namesByBodyIndexes.Add(namesByBodyIndex);
            }
        }

        public static string TurnNameIntoToken(string name)
        {
            // Remove special characters
            StringBuilder sb = new StringBuilder();
            foreach (char c in name) {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) {
                    sb.Append(c);
                }
            }
            
            var simplifiedName = sb.ToString();

            var uppercaseName = simplifiedName.ToUpper();

            var token = "DRONESNAMES_" + uppercaseName;

            return token;
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
            foreach (var namesByBodyIndex in namesByBodyIndexes)
            {
                // Log.LogDebug(namesByBodyIndex.bodyIndex);

                if (namesByBodyIndex.bodyIndex == bodyName)
                {
                    // Log.LogDebug("FOUND!!!");

                    namesDictionary = namesByBodyIndex.names;
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

        public static ConfigCategory[] configCategories;
        public static ConfigBodyIndex[] configBodyIndexes;
        public static void InitConfig()
        {
            configCategories = new ConfigCategory[]
            {
                new ConfigCategory( "Default", "Christopher,Jessica,Matthew,Ashley,Jennifer,Joshua,Amanda,Daniel,David,James,Robert,John,Joseph,Andrew,Ryan,Brandon,Jason,Justin,Sarah,William,Jonathan,Stephanie,Brian,Nicole,Nicholas,Anthony,Heather,Eric,Elizabeth,Duncan,Paul,Jeffrey,Ben,Javid,Nick,Steve,Gabriel,Jaime,Sean,Steven,Alejandro,Devon,Hugh,Rick,Reza,Mansoor,Hariz,Orso,Aleks,Jean-luc" ),
                new ConfigCategory( "Drones",  ".jpeg,Michael,Microwave,New Folder (1),Hotdog.com,Buddy,32-bit,.png,Child 2.0,Ampie 3,Beep,Bolt,The Killer ^_^,1001000 1001001,V1,V2,Bee,Beep jr.,Updog,Foo,Bar,Hello World,Null,Access violation,Nova,Bigweld,Robots (2005),Q5U4EX7YY2E9N,Java,she risk on my of til i rain 2,Dronathan,Zip bomb,Killshare,notavirus.exe" ),
                new ConfigCategory( "Void",  "Void Guy" ),
                new ConfigCategory( "CustomCategory1", "" ),
                new ConfigCategory( "CustomCategory2", "" ),
                new ConfigCategory( "CustomCategory3", "" ),
                new ConfigCategory( "CustomCategory4", "" ),
                new ConfigCategory( "CustomCategory5", "" )
            }; 

            configBodyIndexes = new ConfigBodyIndex[]
            {
                new ConfigBodyIndex( "Turret1",               "Gunner Turret",            "Default,Drones" ,        "Useless,Flightless Drone" ),
                new ConfigBodyIndex( "Drone1",                "Gunner Drone",             "Default,Drones" ,        "" ),
                new ConfigBodyIndex( "Drone2",                "Healing Drone",            "Default,Drones" ,        ":3" ),
                new ConfigBodyIndex( "MissileDrone",          "Missiles Drone",           "Default,Drones" ,        "" ),
                new ConfigBodyIndex( "EquipmentDrone",        "Equipment Drone",          "Default,Drones" ,        "Fart" ),
                new ConfigBodyIndex( "FlameDrone",            "Incinerator Drone",        "Default,Drones" ,        "I'M KILLING YOU, I'M KILLING YOU!, MY PROGRAMMING IS JUST 'GET THAT FUCKING GUY RIGHT NOW" ),
                new ConfigBodyIndex( "EmergencyDrone",        "Emergency Drone",          "Default,Drones" ,        "" ),
                new ConfigBodyIndex( "BackupDrone",           "Back-Up Strike Drone",     "Default,Drones" ,        "" ),
                new ConfigBodyIndex( "EngiTurret",            "Engineer Turret",          "Default,Drones" ,        "pew pew,*shoots you*" ),
                new ConfigBodyIndex( "EngiWalkerTurret",      "Engineer Mobile Turret",   "Default,Drones" ,        "woooshhhphewww,*lasers you*" ),
                new ConfigBodyIndex( "MegaDrone",             "TC-280 Prototype",         "Default" ,               "DESTRUCTION,APOCALYPSE,ARMAGEDDON,BAD INVESTMENT,I <3 VOID SEED,*DIES*" ),
                new ConfigBodyIndex( "DroneCommander",        "Col. Droneman",            "Default" ,               "Col. Beep Boop" ),
                new ConfigBodyIndex( "BeetleGuardAlly",       "Beetle Guard",             "Default" ,               "Sir Dies-A-Lot" ),
                new ConfigBodyIndex( "SquidTurret",           "Squid Polyp",              "Default" ,               "Agent 3,Agent 4,Free Scrap,Scrap Me" ),
                new ConfigBodyIndex( "RoboBallRedBuddy",      "Quiet Probe",              "Default" ,               "DO YOU LOVE THEM TOO,RED AND LONELY,COLD" ),
                new ConfigBodyIndex( "RoboBallGreenBuddy",    "Delighted Probe",          "Default" ,               "YES I LOVE YOU TOO,GREEN AND LONELY,BOLD" ),
                new ConfigBodyIndex( "MinorConstructOnKill",  "Alpha Construct",          "Default" ,               "Triangle" ),
                new ConfigBodyIndex( "TitanGold",             "Aurelionite",              "Default" ,               "Big Boy,The Guardian" ),
                new ConfigBodyIndex( "NullifierAlly",         "Void Reaver",              "Default,Void" ,          "the crap" ),
                new ConfigBodyIndex( "VoidJailerAlly",        "Void Jailer",              "Default,Void" ,          "the lober" ),
                new ConfigBodyIndex( "VoidMegaCrabAlly",      "Void Devastator",          "Default,Void" ,          "the cooler crap" ),
            };
        }    
    }
}
