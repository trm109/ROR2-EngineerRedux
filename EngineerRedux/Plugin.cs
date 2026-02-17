using System;
using System.IO;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EngineerRedux
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class EngineerReduxPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Saik3617";
        public const string PluginName = "EngineerRedux";
        public const string PluginVersion = "1.0.0";

        // private GameObject engiBody = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();
        // private GameObject engiBody = Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/Engi/EngiTurretBody.prefab").WaitForCompletion();

        public void Awake()
        {
            EngineerRedux.Skills.EngiSkillManager.Init(); // See Skills/EngiSkillManager.cs for more details.
            // Log to confirm addition.
            //Log.LogInfo("Added Gauss and Beam attacks to Engineer's primary skill family.");
        }

    }
}
