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
			SteppedSkillDef gaussPrimary = CreateGaussPrimary();
			ContentAddition.AddSkillDef(gaussPrimary);

			Array.Resize(ref primarySkillFamily.variants, primarySkillFamily.variants.Length + 1);
			primarySkillFamily.variants[primarySkillFamily.variants.Length - 1] = new SkillFamily.Variant {
				skillDef = gaussPrimary,
				unlockableName = "",
				viewableNode = new ViewablesCatalog.Node(gaussPrimary.skillNameToken, false, null)
			};
		}
		public static SteppedSkillDef CreateGaussPrimary() {
			SteppedSkillDef skillDef = ScriptableObject.CreateInstance<SteppedSkillDef>();

			skillDef.activationState = new SerializableEntityStateType(typeof(EngineerRedux.EntityStates.EngiGaussPrimaryState));
			skillDef.activationStateMachineName = "Weapon";
			skillDef.baseMaxStock = 1;
			skillDef.baseRechargeInterval = 0f;
			skillDef.beginSkillCooldownOnSkillEnd = true;
			skillDef.canceledFromSprinting = false;
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

			return skillDef;
		}
	}
}
