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

		public static GameObject stationaryTurretPrefab;
		public static GameObject mobileTurretPrefab;
		public static GameObject droneTurretPrefab;
		// public static GameObject engiBodyPrefab;

		// Called from EngineerRedux.EngineerReduxPlugin.Awake()
		public static void Init() {
			// Assign references to prefabs.
			stationaryTurretPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiTurretBody.prefab").WaitForCompletion();
			mobileTurretPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiWalkerTurretBody.prefab").WaitForCompletion();
			droneTurretPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/BackupDroneBody.prefab").WaitForCompletion();
			// Add components to the prefabs.

			// Hook: RoR2.MasterSummon
			//public static event Action<MasterSummonReport> onServerMasterSummonGlobal;
			RoR2.MasterSummon.onServerMasterSummonGlobal += OnServerMasterSummonGlobal;
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

		public static void ChangeEngiBodyType(TurretWeaponSelectorSkillDef selectorSkillDef){
			// turretBodyPrefab = selectorSkillDef.turretBodyPrefab;
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
