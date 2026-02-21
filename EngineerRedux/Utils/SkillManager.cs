using RoR2;
using RoR2.Skills;
using R2API;
using BepInEx;
using EntityStates;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using On;

namespace EngineerRedux.Utils;
public static class SkillManager {
	public static GameObject engiBodyPrefab;
	public static SkillLocator engiSkillLocator;
	public static SkillFamily engiPrimarySkillFamily;
	public static SkillFamily engiTurretWeaponSkillFamily;
	public static SkillFamily engiTurretBodySkillFamily;

	public static GameObject turretBodyPrefab;
	public static SkillLocator turretSkillLocator;
	public static SkillFamily turretPrimarySkillFamily;

	public static void Init() {
		engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();
		engiSkillLocator = engiBodyPrefab.GetComponent<SkillLocator>();
		engiPrimarySkillFamily = engiSkillLocator.primary.skillFamily;

		engiTurretWeaponSkillFamily = CreateSkillFamily(engiBodyPrefab, "TurretWeapon", "ENGI_REDUX_TURRET_WEAPON_SLOT");
		ContentAddition.AddSkillFamily(engiTurretWeaponSkillFamily);

		engiTurretBodySkillFamily = engiSkillLocator.special.skillFamily; // TODO maybe seperate this and replace special?

		turretBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiTurretBody.prefab").WaitForCompletion();
		turretSkillLocator = turretBodyPrefab.GetComponent<SkillLocator>();
		turretPrimarySkillFamily = turretSkillLocator.primary.skillFamily;

		// Add hook on Turret Summon to swap to desired turret weapon and body types
		RoR2.MasterSummon.onServerMasterSummonGlobal += OnServerMasterSummonGlobal;
	}

	//TODO refactor this to use AddLanguageTokens
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
			viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
		};
	}

	public static void AddLanguageTokens(SkillDef skillDef, string skillName, string skillDescription) {
		// Generate a name token
		// SkillNameExample -> SKILL_NAME_EXAMPLE
		string nameToken = "ENGI_REDUX_" + System.Text.RegularExpressions.Regex.Replace(skillName, "(\\B[A-Z])", "_$1").ToUpper() + "_NAME";
		string descriptionToken = "ENGI_REDUX_" + System.Text.RegularExpressions.Regex.Replace(skillName, "(\\B[A-Z])", "_$1").ToUpper() + "_DESC";

		skillDef.skillName = nameToken;
		skillDef.skillNameToken = nameToken;
		skillDef.skillDescriptionToken = descriptionToken;

		LanguageAPI.Add(nameToken, skillName);
		LanguageAPI.Add(descriptionToken, skillDescription);

		return;
	}

	public static void AddEngiPrimary(SkillDef skillDef, string skillName, string skillDescription) {
		if (!(bool)engiBodyPrefab || !(bool)engiSkillLocator || !(bool)engiPrimarySkillFamily)
		{
			Init();
		}

		// if activationStateMachineName is null or empty, set it to "Weapon".
		if(string.IsNullOrWhiteSpace(skillDef.activationStateMachineName)) {
			skillDef.activationStateMachineName = "Weapon";
		}

		// Add language tokens
		AddLanguageTokens(skillDef, skillName, skillDescription);
		// Add the new skill to the primary skill family
		AddSkillToFamily(skillDef, engiPrimarySkillFamily);
		// Add the new skill to the content pack
		ContentAddition.AddSkillDef(skillDef);

		return;
	}

	public static void AddEngiTurretWeapon(SkillDef turretSkillDef, string skillName, string skillDescription) {
		if (!(bool)engiBodyPrefab || !(bool)engiSkillLocator || !(bool)engiTurretWeaponSkillFamily || !(bool)turretPrimarySkillFamily)
		{
			Init();
		}
		// if activationStateMachineName is null or empty, set it to "Weapon".
		if(string.IsNullOrEmpty(turretSkillDef.activationStateMachineName)) {
			turretSkillDef.activationStateMachineName = "Weapon";
		}
		TurretWeaponSelectorSkillDef engiSkillDef = ScriptableObject.CreateInstance<TurretWeaponSelectorSkillDef>();
		engiSkillDef.icon = turretSkillDef.icon;
		engiSkillDef.skillName = turretSkillDef.skillName;
		engiSkillDef.skillNameToken = turretSkillDef.skillNameToken;
		engiSkillDef.skillDescriptionToken = turretSkillDef.skillDescriptionToken;
		engiSkillDef.keywordTokens = turretSkillDef.keywordTokens;

		engiSkillDef.selectedPrimarySkillDef = turretSkillDef; // NRE
		engiSkillDef.activationState = new SerializableEntityStateType(typeof(Idle));
		engiSkillDef.activationStateMachineName = "Weapon";

		// Add language tokens
		AddLanguageTokens(engiSkillDef, skillName, skillDescription);
		// dont bother adding language tokens for the turret skill since it will never be seen by the player

		// Add the new skill to the turret weapon skill family
		AddSkillToFamily(engiSkillDef, engiTurretWeaponSkillFamily);
		// Add the new skill to the turret primary skill family
		AddSkillToFamily(turretSkillDef, turretPrimarySkillFamily);

		// Add the engi skill to the content pack
		ContentAddition.AddSkillDef(engiSkillDef);
		// Add the turret skill to the content pack
		ContentAddition.AddSkillDef(turretSkillDef);

		return;
	}

	public static void AddEngiTurretBody(SkillDef skillDef, string skillName, string skillDescription) {
		if (!(bool)engiBodyPrefab || !(bool)engiSkillLocator || !(bool)engiTurretBodySkillFamily)
		{
			Init();
		}

		// Add language tokens
		AddLanguageTokens(skillDef, skillName, skillDescription);
		// Add the new skill to the turret body skill family
		AddSkillToFamily(skillDef, engiTurretBodySkillFamily);
		// Add the new skill to the content pack
		ContentAddition.AddSkillDef(skillDef);

		return;
	}

	private static void OnServerMasterSummonGlobal(RoR2.MasterSummon.MasterSummonReport report) {
		//Check if its one of our turrets
		CharacterBody summonBody = report.summonBodyInstance;
		// check if the summonBody is one of our turret bodies by checking its baseNameToken. This is the same for walkers and stationary.
		if(summonBody.baseNameToken == "ENGITURRET_BODY_NAME") {
			// if so, look for TurretWeaponSelectorSkillDef
			CharacterBody ownerBody = report.leaderBodyInstance;
			SkillLocator ownerSkillLocator = ownerBody.skillLocator;
			foreach(GenericSkill skill in ownerSkillLocator.AllSkills) {
				// Find Turret Body Type to change base stats
				// TODO replace the nameToken comparison with a field accessor on the 'body skill def', similar to how turret weapon is handled.
				if(skill.skillDef.skillName == "PlaceStationaryTurret") {
					summonBody.baseMaxHealth = 195f;
					summonBody.levelMaxHealth = 58.5f;

					summonBody.baseArmor = 30f;
				}
				if(skill.skillDef.skillName == "PlaceMobileTurret") {
					summonBody.baseMaxHealth = 130f;
					summonBody.levelMaxHealth = 39f;

					summonBody.baseArmor = 10f;
				}
				if(skill.skillDef.skillName == "PlaceDroneTurret") {
					summonBody.baseMaxHealth = 100f;
					summonBody.levelMaxHealth = 30f;
				}
				// Find Turret Weapon Type
				if(skill.skillDef is TurretWeaponSelectorSkillDef) {
					TurretWeaponSelectorSkillDef selectorSkillDef = skill.skillDef as TurretWeaponSelectorSkillDef;
					// if we find it, set the turret's primary skill to the selected primary skill def in the TurretWeaponSelectorSkillDef.
					// TODO determine if there is a better way to do this.
					summonBody.skillLocator.primary.SetBaseSkill(selectorSkillDef.selectedPrimarySkillDef);
				}
			}
		}
	}
}
