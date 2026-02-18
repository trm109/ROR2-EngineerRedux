using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Skills;
using EntityStates;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EngineerRedux.Skills
{
	public static class EngiSkillManager {
		public static SkillFamily turretWeaponSkillFamily;

		// Called from EngineerRedux.EngineerReduxPlugin.Awake()
		public static void Init() {
			GameObject engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();

			SkillLocator skillLocator = engiBodyPrefab.GetComponent<SkillLocator>();

			// SF::Primaries
			SkillFamily primarySkillFamily = skillLocator.primary.skillFamily;
			// 	Gauss Primary
			ContentAddition.AddSkillDef(AddGaussPrimary(primarySkillFamily));
			// 	Beam Primary
			ContentAddition.AddSkillDef(AddBeamPrimary(primarySkillFamily));

			// SF::Special (Turret Body Type)
			SkillFamily specialSkillFamily = skillLocator.special.skillFamily;
			// Modify Stationary Turret Skill
			SkillDef stationaryTurretSkillDef = specialSkillFamily.variants[0].skillDef;
			stationaryTurretSkillDef.skillName = "PlaceStationaryTurret";
			stationaryTurretSkillDef.skillNameToken = "ENGI_SPECIAL_TURRET_STATIONARY_NAME";
			stationaryTurretSkillDef.skillDescriptionToken = "ENGI_SPECIAL_TURRET_STATIONARY_DESC";
			LanguageAPI.Add(stationaryTurretSkillDef.skillNameToken, "Stationary Turret");
			LanguageAPI.Add(stationaryTurretSkillDef.skillDescriptionToken, "Deploy a stationary turret. Has <style=cIsHealing> 150% health</style>, <style=cIsUtility> 30 armor</style>, and <style=cIsUtility>0% movespeed</style>.");
			// Modify Mobile Turret Skill
			SkillDef mobileTurretSkillDef = specialSkillFamily.variants[1].skillDef;
			mobileTurretSkillDef.skillName = "PlaceMobileTurret";
			mobileTurretSkillDef.skillNameToken = "ENGI_SPECIAL_TURRET_MOBILE_NAME";
			mobileTurretSkillDef.skillDescriptionToken = "ENGI_SPECIAL_TURRET_MOBILE_DESC";
			LanguageAPI.Add(mobileTurretSkillDef.skillNameToken, "Mobile Turret");
			LanguageAPI.Add(mobileTurretSkillDef.skillDescriptionToken, "Deploy a mobile turret. Has <style=cIsHealing> 100% health</style>, <style=cIsUtility> 20 armor</style>, and <style=cIsUtility>100% movespeed</style>.");
			// Create Drone Turret Skill (copy of stationary turret)

			// SF::Turret Weapons
			// 	Create Skill Family
			turretWeaponSkillFamily = CreateSkillFamily(engiBodyPrefab, "TurretWeapon", "ENGI_REDUX_TURRET_WEAPON_SLOT");

			ContentAddition.AddSkillFamily(turretWeaponSkillFamily);
			// Add Grenades
			ContentAddition.AddSkillDef(AddGrenadeTurretWeapon(turretWeaponSkillFamily));
			// Add Gauss
			ContentAddition.AddSkillDef(AddGaussTurretWeapon(turretWeaponSkillFamily));
			// Add Laser
			ContentAddition.AddSkillDef(AddBeamTurretWeapon(turretWeaponSkillFamily));

		}

		public static SkillFamily CreateSkillFamily(GameObject bodyPrefab, string skillFamilyName, string skillSlotToken) {
			SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
			(skillFamily as ScriptableObject).name = bodyPrefab.name + skillFamilyName;
			skillFamily.variants = new SkillFamily.Variant[0];

			// Creating a placeholder skill is required to get the game to show this skill family in the loadout menu.
			GenericSkill placeHolderSkill = bodyPrefab.AddComponent<GenericSkill>();
			placeHolderSkill.skillName = skillFamilyName;
			placeHolderSkill.hideInCharacterSelect = true;
			placeHolderSkill._skillFamily = skillFamily;

			if(!string.IsNullOrEmpty(skillSlotToken)) {
				// placeHolderSkill.SetLoadoutTitleTokenOverride(skillSlotToken);
				placeHolderSkill.loadoutTitleToken = skillSlotToken;
				// Convert camelCase `TurretWeapon` to spaced `Turret Weapon` for the language token
				string skillSlotSpaced = System.Text.RegularExpressions.Regex.Replace(skillFamilyName, "(\\B[A-Z])", " $1");
				LanguageAPI.Add(skillSlotToken, skillSlotSpaced);
			}

			return skillFamily;
		}

		public static void AddSkillToFamily(SkillDef skillDef, SkillFamily skillFamily) {
			Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
			skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant {
				skillDef = skillDef,
				unlockableName = "",
				viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
			};
		}

		public static TurretWeaponSelectorSkillDef AddGrenadeTurretWeapon(SkillFamily skillFamily){
			TurretWeaponSelectorSkillDef skillDef = ScriptableObject.CreateInstance<TurretWeaponSelectorSkillDef>();
			skillDef.activationState = new SerializableEntityStateType(typeof(Idle));
			skillDef.activationStateMachineName = "Weapon";
			skillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiBodyFireGrenade.asset").WaitForCompletion().icon; // placeholder icon
			skillDef.skillDescriptionToken = "ENGI_TURRET_WEAPON_GRENADE_DESC";
			skillDef.skillName = "ENGI_TURRET_WEAPON_GRENADE_NAME";
			skillDef.skillNameToken = "ENGI_TURRET_WEAPON_GRENADE_NAME";
			// Add a reference to the grenade turret weapon skillDef
			// TODO implement a turret grenade skilldef
			skillDef.selectedPrimarySkillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiBodyFireGrenade.asset").WaitForCompletion();

			// Add language tokens
			LanguageAPI.Add(skillDef.skillNameToken, "Grenade Launcher");
			LanguageAPI.Add(skillDef.skillDescriptionToken, "Turrets charge up to <style=cIsDamage>4</style> grenades that deal <style=cIsDamage>100% damage</style> each.");

			// Add to skillFamily
			AddSkillToFamily(skillDef, skillFamily);

			return skillDef;
		}
		public static TurretWeaponSelectorSkillDef AddGaussTurretWeapon(SkillFamily skillFamily){
			TurretWeaponSelectorSkillDef skillDef = ScriptableObject.CreateInstance<TurretWeaponSelectorSkillDef>();
			skillDef.activationState = new SerializableEntityStateType(typeof(Idle));
			skillDef.activationStateMachineName = "Weapon";
			skillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFirePistol.asset").WaitForCompletion().icon; // placeholder icon
			skillDef.skillDescriptionToken = "ENGI_TURRET_WEAPON_GAUSS_DESC";
			skillDef.skillName = "ENGI_TURRET_WEAPON_GAUSS_NAME";
			skillDef.skillNameToken = "ENGI_TURRET_WEAPON_GAUSS_NAME";
			// Add a reference to the gauss turret weapon skillDef
			skillDef.selectedPrimarySkillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiTurretBodyTurret.asset").WaitForCompletion(); // Lmao, this is what they named the Engi Turret's default skillDef?

			// Add language tokens
			LanguageAPI.Add(skillDef.skillNameToken, "Gauss Cannon");
			LanguageAPI.Add(skillDef.skillDescriptionToken, "Turrets fire <style=cIsDamage>70% damage</style> bullets, 3 times per second.");

			// Add to skillFamily
			AddSkillToFamily(skillDef, skillFamily);

			return skillDef;
		}
		public static TurretWeaponSelectorSkillDef AddBeamTurretWeapon(SkillFamily skillFamily){
			TurretWeaponSelectorSkillDef skillDef = ScriptableObject.CreateInstance<TurretWeaponSelectorSkillDef>();
			skillDef.activationState = new SerializableEntityStateType(typeof(Idle));
			skillDef.activationStateMachineName = "Weapon";
			skillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFireFMJ.asset").WaitForCompletion().icon; // placeholder icon
			skillDef.skillDescriptionToken = "ENGI_TURRET_WEAPON_BEAM_DESC";
			skillDef.skillName = "ENGI_TURRET_WEAPON_BEAM_NAME";
			skillDef.skillNameToken = "ENGI_TURRET_WEAPON_BEAM_NAME";
			// Add a reference to the beam turret weapon skillDef
			// TODO Increase distance to 50f;
			skillDef.selectedPrimarySkillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiTurretFireBeam.asset").WaitForCompletion();

			// Add language tokens
			LanguageAPI.Add(skillDef.skillNameToken, "Laser Cannon");
			LanguageAPI.Add(skillDef.skillDescriptionToken, "Turrets fire a continuous laser that deals <style=cIsDamage>40% damage</style>, 5 times per second. <style=cIsUtility>Slows</style> enemies by <style=cIsUtility>50%</style> on hit.");

			// Add to skillFamily
			AddSkillToFamily(skillDef, skillFamily);

			return skillDef;
		}

		public static SteppedSkillDef AddGaussPrimary(SkillFamily skillFamily) {
			SteppedSkillDef skillDef = ScriptableObject.CreateInstance<SteppedSkillDef>();
				skillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.EngiGaussPrimaryState));
				skillDef.activationStateMachineName = "Weapon";
				skillDef.baseRechargeInterval = 0f;
				skillDef.beginSkillCooldownOnSkillEnd = true;
				skillDef.canceledFromSprinting = true;
				skillDef.interruptPriority = InterruptPriority.Any;
				skillDef.rechargeStock = 0;
				skillDef.requiredStock = 0;
				skillDef.stockToConsume = 0;
				skillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFirePistol.asset").WaitForCompletion().icon; // placeholder icon
				skillDef.skillDescriptionToken = "ENGI_PRIMARY_GAUSS_DESC";
				skillDef.skillName = "ENGI_PRIMARY_GAUSS_NAME";
				skillDef.skillNameToken = "ENGI_PRIMARY_GAUSS_NAME";
				skillDef.stepCount = 2; // used to alternate between the two barrels
			// Add language tokens
			LanguageAPI.Add(skillDef.skillNameToken, "Gauss Cannon");
			LanguageAPI.Add(skillDef.skillDescriptionToken, "Fire <style=cIsDamage>2x70% damage</style> bullets, 3 times per second.");

			// Add to skillFamily
			AddSkillToFamily(skillDef, skillFamily);

			return skillDef;
		}

		public static SkillDef AddBeamPrimary(SkillFamily skillFamily) {
			// Requires assets that I have to reference at runtime...
			EngineerRedux.EntityStates.EngiBeamPrimaryState.Init();

			SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
				skillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.EngiBeamPrimaryState));
				skillDef.activationStateMachineName = "Weapon";
				skillDef.baseRechargeInterval = 0f;
				skillDef.beginSkillCooldownOnSkillEnd = true;
				skillDef.canceledFromSprinting = true;
				skillDef.interruptPriority = InterruptPriority.Any;
				skillDef.rechargeStock = 0;
				skillDef.requiredStock = 0;
				skillDef.stockToConsume = 0;
				skillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFireFMJ.asset").WaitForCompletion().icon; // placeholder icon
				skillDef.skillDescriptionToken = "ENGI_PRIMARY_BEAM_DESC";
				skillDef.skillName = "ENGI_PRIMARY_BEAM_NAME";
				skillDef.skillNameToken = "ENGI_PRIMARY_BEAM_NAME";

			// Add to skillFamily
			LanguageAPI.Add(skillDef.skillNameToken, "Laser Cannon");
			LanguageAPI.Add(skillDef.skillDescriptionToken, "Fire two continuous lasers that deal <style=cIsDamage>2x40% damage</style>, 5 times per second. <style=cIsUtility>Slows</style> enemies by <style=cIsUtility>50%</style> on hit.");

			// Add to skillFamily
			AddSkillToFamily(skillDef, skillFamily);

			return skillDef;
		}
	}
}
