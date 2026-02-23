using System;
using System.Runtime.CompilerServices;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.EngiTurret.EngiTurretWeapon;

namespace EngineerRedux
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.SkillsAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
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
            Utils.SkillManager.Init();

            UnlockableDef mobileTurretUnlockableDef = Addressables.LoadAssetAsync<UnlockableDef>("RoR2/Base/Engi/Skills.Engi.WalkerTurret.asset").WaitForCompletion();
            // Add your new skill Definitions here.
            // Primaries:
            // - Gauss
            SteppedSkillDef gaussSkillDef = ScriptableObject.CreateInstance<SteppedSkillDef>();
            gaussSkillDef.activationStateMachineName = "Weapon";
            gaussSkillDef.beginSkillCooldownOnSkillEnd = true;
            gaussSkillDef.canceledFromSprinting = true;
            gaussSkillDef.cancelSprintingOnActivation = true;
            gaussSkillDef.interruptPriority = InterruptPriority.Any;
            gaussSkillDef.baseRechargeInterval = 0f;
            gaussSkillDef.rechargeStock = 0;
            gaussSkillDef.requiredStock = 0;
            gaussSkillDef.stockToConsume = 0;
            EngineerRedux.EntityStates.Engi.GaussPrimaryState.Init(); // Grabs hit effect, tracer, other vfx prefabs.
            gaussSkillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.Engi.GaussPrimaryState));
            gaussSkillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFirePistol.asset").WaitForCompletion().icon; // placeholder icon
            gaussSkillDef.stepCount = 2; // used to alternate between the two barrels
            gaussSkillDef.stepGraceDuration = 3f; // time allowed between steps before it resets to the first barrel

            // Add the entity state
            ContentAddition.AddEntityState(typeof(EngineerRedux.EntityStates.Engi.GaussPrimaryState), out _);
            // Add to skillFamily
            Utils.SkillManager.AddEngiPrimary(gaussSkillDef, "GaussCannon", "Fire <style=cIsDamage>2x70% damage</style> bullets, 3 times per second.");
            // - Beam
            SkillDef beamSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            beamSkillDef.activationStateMachineName = "Weapon";
            gaussSkillDef.beginSkillCooldownOnSkillEnd = true;
            beamSkillDef.canceledFromSprinting = true;
            beamSkillDef.cancelSprintingOnActivation = true;
            beamSkillDef.interruptPriority = InterruptPriority.Any;
            beamSkillDef.baseRechargeInterval = 0f;
            beamSkillDef.rechargeStock = 0;
            beamSkillDef.requiredStock = 0;
            beamSkillDef.stockToConsume = 0;
            EngineerRedux.EntityStates.Engi.BeamPrimaryState.Init(); // Grabs VFX references
            beamSkillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.Engi.BeamPrimaryState));
            beamSkillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFireFMJ.asset").WaitForCompletion().icon; // placeholder icon

            // Add the entity state
            ContentAddition.AddEntityState(typeof(EngineerRedux.EntityStates.Engi.BeamPrimaryState), out _);
            // Here, I'm reusing the mobile turret unloackable Def
            Utils.SkillManager.AddEngiPrimary(beamSkillDef, "LaserBeam", "Fire two continuous lasers that deal <style=cIsDamage>2x40% damage</style>, 5 times per second. <style=cIsUtility>Slows</style> enemies by <style=cIsUtility>50%</style> on hit.", mobileTurretUnlockableDef);
            // Turret Bodies:
            // - Stationary Turret
            SkillDef stationaryTurretSkillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiBodyPlaceTurret.asset").WaitForCompletion();
            Utils.TurretBodyStats stationaryStats = Utils.TurretBodyStats.Default;
            stationaryStats.movespeed = 0f;
            stationaryStats.maxHealth = 195f;
            stationaryStats.maxHealthInc = 58.5f;
            stationaryStats.healthRegen = 0.9f;
            stationaryStats.healthRegenInc = 0.18f;
            stationaryStats.armor = 30f;
            Utils.SkillManager.AddEngiTurretBody(stationaryTurretSkillDef, "StationaryTurret", "Summon a stationary turret with <style=cIsUtility>High health</style>, but <style=cIsUtility>cannot move</style>.", stationaryStats);

            // - Mobile Turret
            SkillDef mobileTurretSkillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiBodyPlaceWalkerTurret.asset").WaitForCompletion();
            Utils.TurretBodyStats mobileStats = Utils.TurretBodyStats.Default;
            mobileStats.movespeed = 8f;
            Utils.SkillManager.AddEngiTurretBody(mobileTurretSkillDef, "MobileTurret", "Summon a mobile turret with <style=cIsUtility>decent health and movement</style>.", mobileStats, mobileTurretUnlockableDef);

            // Turret Weapons:
            // - Grenade
            SkillDef turretGrenadeSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            turretGrenadeSkillDef.activationStateMachineName = "Weapon";
            turretGrenadeSkillDef.interruptPriority = InterruptPriority.Any;
            turretGrenadeSkillDef.baseRechargeInterval = 2f;
            turretGrenadeSkillDef.attackSpeedBuffsRestockSpeed = true;
            turretGrenadeSkillDef.baseMaxStock = 4;
            turretGrenadeSkillDef.rechargeStock = 4;
            turretGrenadeSkillDef.requiredStock = 1;
            turretGrenadeSkillDef.stockToConsume = 1;
            turretGrenadeSkillDef.dontAllowPastMaxStocks = true;
            // turretGrenadeSkillDef.fullRestockOnAssign = true;
            turretGrenadeSkillDef.beginSkillCooldownOnSkillEnd = true;
            turretGrenadeSkillDef.canceledFromSprinting = false;
            turretGrenadeSkillDef.cancelSprintingOnActivation = false;
            EngineerRedux.EntityStates.Turret.GrenadePrimaryState.Init(); // Grabs VFX references
            turretGrenadeSkillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.Turret.GrenadePrimaryState));
            turretGrenadeSkillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiBodyFireGrenade.asset").WaitForCompletion().icon;
            // Add the entity state
            ContentAddition.AddEntityState(typeof(EngineerRedux.EntityStates.Turret.GrenadePrimaryState), out _);
            Utils.SkillManager.AddEngiTurretWeapon(turretGrenadeSkillDef, "TurretGrenadeLauncher", "Charge up to 4 grenades, dealing <style=cIsDamage>100% damage</style> each.");
            // - Gauss
            SkillDef turretGaussSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            turretGaussSkillDef.activationStateMachineName = "Weapon";
            turretGaussSkillDef.interruptPriority = InterruptPriority.Any;
            turretGaussSkillDef.fullRestockOnAssign = true;
            turretGaussSkillDef.beginSkillCooldownOnSkillEnd = false;
            turretGaussSkillDef.canceledFromSprinting = false;
            turretGaussSkillDef.cancelSprintingOnActivation = false;
            turretGaussSkillDef.baseRechargeInterval = 0f;
            turretGaussSkillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.Turret.GaussPrimaryState));
            turretGaussSkillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFirePistol.asset").WaitForCompletion().icon; // placeholder icon
            // Add the entity state
            ContentAddition.AddEntityState(typeof(EngineerRedux.EntityStates.Turret.GaussPrimaryState), out _);

            Utils.SkillManager.AddEngiTurretWeapon(turretGaussSkillDef, "TurretGaussCannon", "Fire <style=cIsDamage>70% damage</style> bullets, 3 times per second.");
            // - Beam
            SkillDef turretBeamSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            turretBeamSkillDef.activationStateMachineName = "Weapon";
            turretBeamSkillDef.interruptPriority = InterruptPriority.Any;
            turretBeamSkillDef.fullRestockOnAssign = true;
            turretBeamSkillDef.beginSkillCooldownOnSkillEnd = false;
            turretBeamSkillDef.canceledFromSprinting = false;
            turretBeamSkillDef.cancelSprintingOnActivation = true;
            turretBeamSkillDef.baseRechargeInterval = 0f;
            turretBeamSkillDef.baseMaxStock = 1;
            turretBeamSkillDef.rechargeStock = 1;
            turretBeamSkillDef.requiredStock = 1;
            turretBeamSkillDef.stockToConsume = 1;
            EngineerRedux.EntityStates.Turret.BeamPrimaryState.Init(); // Grabs VFX references
            turretBeamSkillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.Turret.BeamPrimaryState));
            turretBeamSkillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFireFMJ.asset").WaitForCompletion().icon; // placeholder icon
            // Add the entity state
            ContentAddition.AddEntityState(typeof(EngineerRedux.EntityStates.Turret.BeamPrimaryState), out _);
            // Here, I'm reusing the mobile turret unloackable Def
            Utils.SkillManager.AddEngiTurretWeapon(turretBeamSkillDef, "TurretLaserBeam", "Fire a continuous laser that deals <style=cIsDamage>40% damage</style>, 5 times per second. <style=cIsUtility>Slows</style> enemies by <style=cIsUtility>50%</style> on hit.", mobileTurretUnlockableDef);

            // Set the default turret weapon to gauss
            Utils.SkillManager.engiTurretWeaponSkillFamily.defaultVariantIndex = 1;

        }

    }
}
