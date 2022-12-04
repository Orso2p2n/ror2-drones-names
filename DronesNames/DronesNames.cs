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
    // [R2APISubmoduleDependency(/*nameof(LanguageAPI)*/)]


    public class DronesNames : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Orso";
        public const string PluginName = "DronesNames";
        public const string PluginVersion = "1.0.0";

        public void Awake()
        {
            Log.Init(Logger);

            NamesList.BuildNamesByBodyName();

            AddTokens();

            On.RoR2.CharacterBody.Start += CharacterBody_Start;
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, RoR2.CharacterBody self)
        {
            orig(self);

            OnCharacterBodySpawned(self);
        }

        private void OnCharacterBodySpawned(CharacterBody characterBody)
        {
            // if (characterBody.master.teamIndex != RoR2.TeamIndex.Player) {
            //     return;
            // }

            var newName = NamesList.GetRandomNameForCharacterBody(characterBody);

            if (newName == "")
            {
                return;
            }

            characterBody.baseNameToken = newName;

            Log.LogDebug(BodyCatalog.GetBodyName(BodyCatalog.FindBodyIndex(characterBody)) + " " + characterBody.GetDisplayName());
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
