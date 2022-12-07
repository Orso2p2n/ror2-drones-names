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

            On.RoR2.CharacterMaster.SpawnBody += CharacterMaster_SpawnBody;
            On.RoR2.CharacterMaster.OnBodyDeath += CharacterMaster_OnBodyDeath;

            On.RoR2.CharacterMaster.OnSerialize += CharacterMaster_OnSerialize;
            On.RoR2.CharacterMaster.OnDeserialize += CharacterMaster_OnDeserialize;
        }

        // HOOKS
        private CharacterBody CharacterMaster_SpawnBody(On.RoR2.CharacterMaster.orig_SpawnBody orig, CharacterMaster self, Vector3 position, Quaternion rotation)
        {
            var body = orig(self, position, rotation);

            if (body != null)
            {
                OnCharacterBodySpawned(self, body);
            }

            return body;
        }

        private void CharacterMaster_OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);
        }

        private bool CharacterMaster_OnSerialize(On.RoR2.CharacterMaster.orig_OnSerialize orig, CharacterMaster self, UnityEngine.Networking.NetworkWriter writer, bool initialState) {
            var b = orig(self, writer, initialState);

            if (self == null) {
                return b;
            }

            var characterBody = self.GetBody();

            if (characterBody == null) {
                return b;
            }

            var bodyName = BodyCatalog.GetBodyName(BodyCatalog.FindBodyIndex(characterBody));
            var displayName = characterBody.GetDisplayName();
            var bodyInstanceId = self.networkIdentity.netId.Value;
            Log.LogDebug("SERIALIZE " + bodyName + " " + displayName + " " + bodyInstanceId);

            return b;
        }

        private void CharacterMaster_OnDeserialize(On.RoR2.CharacterMaster.orig_OnDeserialize orig, CharacterMaster self, UnityEngine.Networking.NetworkReader reader, bool initialState) {
            orig(self, reader, initialState);

            

            if (self == null) {
                return;
            }

            var characterBody = self.GetBody();

            if (characterBody == null) {
                return;
            }

            var bodyName = BodyCatalog.GetBodyName(BodyCatalog.FindBodyIndex(characterBody));
            var displayName = characterBody.GetDisplayName();
            var bodyInstanceId = self.networkIdentity.netId.Value;
            Log.LogDebug("DESERIALIZE " + bodyName + " " + displayName + " " + bodyInstanceId);
        }


        // FUNCTIONS
        private void OnCharacterBodySpawned(CharacterMaster characterMaster, CharacterBody characterBody)
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

            // Log.LogDebug( + " " + );
            #pragma warning disable Publicizer001

            var bodyName = BodyCatalog.GetBodyName(BodyCatalog.FindBodyIndex(characterBody));
            var displayName = characterBody.GetDisplayName();
            var bodyInstanceId = characterMaster.networkIdentity.netId.Value;
            Log.LogDebug("SPAWN " + bodyName + " " + displayName + " " + bodyInstanceId);

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
