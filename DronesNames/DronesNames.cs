using System.Collections.Generic;
using System.Linq;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DronesNames
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]

    public class DronesNames : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Orso_SaltyFinalBoss";
        public const string PluginName = "DronesNames";
        public const string PluginVersion = "1.0.0";

        public static Dictionary<uint,string> savedTokens = new Dictionary<uint,string>();

        public void Awake()
        {
            // Local Network for testing
            // On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => {};

            Log.Init(Logger);

            NamesList.BuildNamesByBodyName();

            AddTokens();

            On.RoR2.CharacterBody.Start += CharacterBody_Start;
        }

        public void OnDestroy()
        {
            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
        }

        // HOOKS
        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, RoR2.CharacterBody self)
        {
            orig(self);

            OnCharacterBodySpawned(self);
        }

        // FUNCTIONS
        private void OnCharacterBodySpawned(CharacterBody characterBody)
        {
            var characterMaster = characterBody.master;

            var newName = NamesList.GetRandomNameForCharacterMaster(characterMaster, characterBody);

            if (newName == "")
            {
                return;
            }

            characterBody.baseNameToken = newName;

            #pragma warning disable Publicizer001

            var bodyName = BodyCatalog.GetBodyName(BodyCatalog.FindBodyIndex(characterBody));
            var displayName = characterBody.GetDisplayName();
            var netId = characterMaster.networkIdentity.netId.Value;
            Log.LogDebug("SPAWN " + bodyName + " " + displayName + " " + netId);

            #pragma warning restore Publicizer001
        }

        private void AddTokens()
        {
            foreach (var nameList in NamesList.names.Values)
            {
                foreach (var token in nameList.Keys)
                {
                    var name = nameList[token];
                    LanguageAPI.Add(token, name);
                }
            }
        }
    }
}
