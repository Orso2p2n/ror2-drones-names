using System.Collections.Generic;
using System.Linq;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using BepInEx.Configuration;

namespace AllyNames
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]

    public class AllyNames : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SwagWizards";
        public const string PluginName = "AllyNames";
        public const string PluginVersion = "1.0.2";

        public const bool LogDebug = false;
        public static AllyNames instance;

        public static Dictionary<uint,string> savedTokens = new Dictionary<uint,string>();

        public static Dictionary<NetworkInstanceId,int> empathyCoresNameIndexes = new Dictionary<NetworkInstanceId,int>();

        private static ConfigFile namesConfigFile { get; set; }

        public void Awake()
        {
            // Local Network for testing
            // On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => {};
            
            instance = this;

            Log.Init(Logger);

            InitConfig();

            NamesList.BuildNamesByBodyName();

            AddTokens();

            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            On.RoR2.CharacterMaster.OnBodyDeath += CharacterMaster_OnBodyDeath;
        }

        public void OnDestroy()
        {
            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
            On.RoR2.CharacterMaster.OnBodyDeath -= CharacterMaster_OnBodyDeath;
        }

        // HOOKS
        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, RoR2.CharacterBody self)
        {
            orig(self);

            OnCharacterBodySpawned(self);
        }

        private void CharacterMaster_OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, RoR2.CharacterMaster self, CharacterBody body)
        {
            // Check if body that died is an empathy core
            var bodyIndex = body.bodyIndex;
            var redBodyIndex = RoR2.BodyCatalog.FindBodyIndex("RoboBallRedBuddyBody");
            var greenBodyIndex = RoR2.BodyCatalog.FindBodyIndex("RoboBallGreenBuddyBody");
            if (bodyIndex == redBodyIndex || bodyIndex == greenBodyIndex) {
                OnEmpathyCoreDeath(redBodyIndex, greenBodyIndex, self);
            }

            orig(self, body);
        }

        // GAMEPLAY FUNCTIONS
        private void OnCharacterBodySpawned(CharacterBody characterBody)
        {
            var characterMaster = characterBody.master;

            var newName = NamesList.GetRandomNameForCharacterMaster(characterMaster, characterBody);

            if (newName == "") return;

            characterBody.baseNameToken = newName;
        }

        List<string> addedTokens;
        private void AddTokens()
        {
            addedTokens = new List<string>();
            foreach (var namesByBodyIndex in NamesList.namesByBodyIndexes)
            {
                foreach (var token in namesByBodyIndex.names.Keys)
                {
                    if (addedTokens.Contains(token))
                    {
                        continue;
                    }

                    var name = namesByBodyIndex.names[token];
                    LanguageAPI.Add(token, name);

                    addedTokens.Add(token);

                    if (AllyNames.LogDebug)
                    {
                        Log.LogDebug(token + " " + name);
                    }
                }
            }
        }

        private void OnEmpathyCoreDeath(BodyIndex redBodyIndex, BodyIndex greenBodyIndex, CharacterMaster characterMaster)
        {
            #pragma warning disable Publicizer001

            // Get minion information
            var minionOwnership = characterMaster.minionOwnership;
            if (minionOwnership == null) return;

            var minionGroup = characterMaster.minionOwnership.group;
            var minionOwnerId = characterMaster.minionOwnership.ownerMasterId;
            if (minionGroup == null || minionOwnerId == null) return;

            var foundEmpathyCoreInMinions = false;
            // Look for all the minions in the same group
            foreach (var member in minionGroup._members) 
            {
                // Skip null members
                if (member == null) continue;

                // Skip members without a CharacterMaster (shouldn't happen, just in case)
                var memberCharacterMaster = member.GetComponent<CharacterMaster>();
                if (memberCharacterMaster == null) continue;

                // Skip member if is the same as the one that died
                if (memberCharacterMaster.networkIdentity == characterMaster.networkIdentity) continue;

                // Skip members without a CharacterBody
                var memberCharacterBody = memberCharacterMaster.GetBody();
                if (memberCharacterBody == null) continue;

                // If one of the minions is an Empathy Core, don't remove the token from the dictionary
                if (memberCharacterBody.bodyIndex == redBodyIndex || memberCharacterBody.bodyIndex == greenBodyIndex)
                {
                    foundEmpathyCoreInMinions = true;
                    break;
                }
            }

            if (!foundEmpathyCoreInMinions)
            {
                empathyCoresNameIndexes.Remove(minionOwnerId);
            }

            #pragma warning restore Publicizer001
        }

        // META FUNCTIONS
        private void InitConfig()
        {
            namesConfigFile = new ConfigFile(Paths.ConfigPath + "\\AllyNames.cfg", true);
            
            NamesList.InitConfig();
        }
    }
}
