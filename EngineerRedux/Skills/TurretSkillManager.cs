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
	public static class TurretSkillManager {

		// Called from EngineerRedux.EngineerReduxPlugin.Awake()
		public static void Init() {
			GameObject engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();
			// SkillLocator skillLocator = engiBodyPrefab.GetComponent<SkillLocator>();
			// SkillFamily primarySkillFamily = skillLocator.primary.skillFamily;
		}
		// Adds a skill family to select our Turrets primary, independently from the 'body type' of the turret (i.e. stationary, mobile)
		// On Engineer's Special being used, replace the newly spawned turret with the selected primary.
		// Can hook:
		//   EntityStates.Engi.EngiWeapon.PlaceTurret.turretMasterPrefab
		public static void AddTurretWeaponSkillFamily() {
		}
		// Allows selecting the 'body type' of the turret. Stationary, mobile, flying.
		public static void AddTurretBodySkillFamily() {
		}

		public static void AddGrenadePrimary(SkillFamily skillFamily) {}
		public static void AddGaussPrimary(SkillFamily skillFamily) {}
		public static void AddBeamPrimary(SkillFamily skillFamily) {}
		// public static void AddFlamethrowerPrimary(SkillFamily skillFamily) {}
		// public static void AddRailgunPrimary(SkillFamily skillFamily) {}
		// public static void AddMissilePrimary(SkillFamily skillFamily) {}

		// public static void AddStationaryBody(SkillFamily skillFamily) {}
		// 		HP = 195 + 58.5x
		// 		Armor = 30
		// 		Speed = 0
		// public static void AddMobileBody(SkillFamily skillFamily) {}
		//    HP = 130 + 39x
		//    Armor = 10
		//    Speed = 7
		// public static void AddFlyingBody(SkillFamily skillFamily) {
		//    HP = 100 + 30x
		//    Armor = 0
		//    Speed = 12
	}
}
