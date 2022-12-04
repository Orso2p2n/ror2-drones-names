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
    [R2APISubmoduleDependency(/*nameof(LanguageAPI)*/)]

    public class DronesNames : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Orso";
        public const string PluginName = "DronesNames";
        public const string PluginVersion = "1.0.0";

        public void Awake()
        {

        }

        private void Update()
        {

        }
    }
}
