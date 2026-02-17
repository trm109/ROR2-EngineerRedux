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

		// Called from EngineerRedux.EngineerReduxPlugin.Awake()
		public static void Init() {
			GameObject engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();
			SkillLocator skillLocator = engiBodyPrefab.GetComponent<SkillLocator>();
			SkillFamily primarySkillFamily = skillLocator.primary.skillFamily;

			// Gauss Primary
			AddGaussPrimary(primarySkillFamily);
			// Beam Primary
			AddBeamPrimary(primarySkillFamily);

		}
		public static void AddGaussPrimary(SkillFamily skillFamily) {
			SteppedSkillDef skillDef = ScriptableObject.CreateInstance<SteppedSkillDef>();

			skillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.EngiGaussPrimaryState));
			skillDef.activationStateMachineName = "Weapon";
			skillDef.baseMaxStock = 1;
			skillDef.baseRechargeInterval = 0f;
			skillDef.beginSkillCooldownOnSkillEnd = true;
			skillDef.cancelSprintingOnActivation = true;
			skillDef.canceledFromSprinting = true;
			skillDef.fullRestockOnAssign = true;
			skillDef.interruptPriority = InterruptPriority.Any;
			skillDef.isCombatSkill = true;
			skillDef.mustKeyPress = false;
			skillDef.rechargeStock = 0;
			skillDef.requiredStock = 0;
			skillDef.stockToConsume = 0;
			skillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFirePistol.asset").WaitForCompletion().icon; // placeholder icon
			skillDef.skillDescriptionToken = "ENGI_PRIMARY_GAUSS_DESC";
			skillDef.skillName = "ENGI_PRIMARY_GAUSS_NAME";
			skillDef.skillNameToken = "ENGI_PRIMARY_GAUSS_NAME";
			skillDef.stepCount = 2; // used to alternate between the two barrels

			LanguageAPI.Add("ENGI_PRIMARY_GAUSS_NAME", "Gauss Cannon");
			LanguageAPI.Add("ENGI_PRIMARY_GAUSS_DESC", "Fire <style=cIsDamage>2x70% damage</style> bullets, 3 times per second.");

			ContentAddition.AddSkillDef(skillDef);

			Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
			skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant {
				skillDef = skillDef,
				unlockableName = "",
				viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
			};
		}

		public static void AddBeamPrimary(SkillFamily skillFamily) {
			// Requires assets that I have to reference at runtime...
			EngineerRedux.EntityStates.EngiBeamPrimaryState.Init();

			SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();

			skillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.EngiBeamPrimaryState));
			skillDef.activationStateMachineName = "Weapon";
			skillDef.baseMaxStock = 1;
			skillDef.baseRechargeInterval = 0f;
			skillDef.beginSkillCooldownOnSkillEnd = true;
			skillDef.cancelSprintingOnActivation = true;
			skillDef.canceledFromSprinting = true;
			skillDef.fullRestockOnAssign = true;
			skillDef.interruptPriority = InterruptPriority.Any;
			skillDef.isCombatSkill = true;
			skillDef.mustKeyPress = false;
			skillDef.rechargeStock = 0;
			skillDef.requiredStock = 0;
			skillDef.stockToConsume = 0;
			skillDef.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFireFMJ.asset").WaitForCompletion().icon; // placeholder icon
			skillDef.skillDescriptionToken = "ENGI_PRIMARY_BEAM_DESC";
			skillDef.skillName = "ENGI_PRIMARY_BEAM_NAME";
			skillDef.skillNameToken = "ENGI_PRIMARY_BEAM_NAME";

			LanguageAPI.Add("ENGI_PRIMARY_BEAM_NAME", "Laser Cannon");
			LanguageAPI.Add("ENGI_PRIMARY_BEAM_DESC", "Fire two continuous lasers that deal <style=cIsDamage>2x40% damage</style>, 5 times per second. <style=cIsUtility>Slows</style> enemies by <style=cIsUtility>50%</style> on hit.");

			ContentAddition.AddSkillDef(skillDef);

			Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
			skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant {
				skillDef = skillDef,
				unlockableName = "",
				viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
			};
		}
	}
}
