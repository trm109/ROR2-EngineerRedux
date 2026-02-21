using RoR2;
using RoR2.Skills;

using EntityStates;
using UnityEngine;
using UnityEngine.AddressableAssets;

// Referencing Base Games Engi turret
// using EntityStates.EngiTurret.EngiTurretWeapon;
//using EntityStates.Engi.EngiWeapon;

namespace EngineerRedux.EntityStates.Engi
{
	public class GaussPrimaryState : BaseSkillState, SteppedSkillDef.IStepSetter
	{

		public static float baseDuration = 0.35f / 2f; // Turret is default 35, engi basically has two turrets on his back.
		private float duration;

		public static float damageCoefficient = 0.7f;
		public static float force = 200f;
		public static float maxRange = 300f;

		// stealing these from Commando's M1
		public static float recoilAmplitude = 1f;
		public static float spreadBloomValue = 0.3f;
		public static float trajectoryAimAssistMultiplier = 0.75f;

		// VFX references
		public static GameObject muzzleEffectPrefab;
		public static GameObject hitEffectPrefab;
		public static GameObject tracerEffectPrefab;
		// Reusing Gauss Turret SFX
		public static string attackSoundString = "Play_engi_R_turret_shot";

		private Transform modelTransform; // reference to engi transform
		//private Ray projectileRay; // swaps between muzzles

		private int step; // Which muzzle does the next shot come from?

		void SteppedSkillDef.IStepSetter.SetStep(int i){
			step = i;
		}

		public static void Init() {
			// Assign VFX references
			muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/MuzzleflashEngiTurret.prefab").WaitForCompletion();
			hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/ImpactEngiTurret.prefab").WaitForCompletion();
			tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/TracerEngiTurret.prefab").WaitForCompletion();
		}

		private Vector3 GetAimEndPoint(Ray aimRay){
			// Calculate aim direction, taken from SS2's Engi Laser Ability
			Vector3 aimEndPoint = aimRay.GetPoint(maxRange);
			RaycastHit raycastHit;
			if (Util.CharacterRaycast(base.gameObject, aimRay, out raycastHit, maxRange, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
			{
				aimEndPoint = raycastHit.point;
			}
			return aimEndPoint;
		}

		private void FireBullet(Ray aimRay, string targetMuzzle){
			Util.PlaySound(attackSoundString, base.gameObject);
			Vector3 muzzlePosition = Vector3.zero;
			// if reference to parent object exists
			if((bool)modelTransform){
				// try and get target muzzle object
				ChildLocator component = modelTransform.GetComponent<ChildLocator>();
				if((bool)component){
					// try and get target muzzle position
					Transform targetTransform = component.FindChild(targetMuzzle);
					if((bool)targetTransform){
						// set target muzzle position to target muzzle position
						muzzlePosition = targetTransform.position;
					}
				}
			}
			AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
			if((bool)muzzleEffectPrefab){
				EffectManager.SimpleMuzzleFlash(
						muzzleEffectPrefab,
						base.gameObject,
						targetMuzzle,
						transmit: false);
			}
			// Mostly taken from Commando's M1
			if(base.isAuthority){
				BulletAttack bulletAttack = new BulletAttack();

				bulletAttack.owner = base.gameObject;
				bulletAttack.weapon = base.gameObject;
				bulletAttack.origin = muzzlePosition;
				Vector3 aimEndPoint = GetAimEndPoint(aimRay);
				bulletAttack.aimVector = (aimEndPoint - muzzlePosition).normalized;
				bulletAttack.minSpread = 0f;
				bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
				bulletAttack.damage = damageCoefficient * damageStat;
				bulletAttack.force = force;
				bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
				bulletAttack.muzzleName = targetMuzzle;
				bulletAttack.hitEffectPrefab = hitEffectPrefab;
				bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
				bulletAttack.radius = 0.15f;
				bulletAttack.smartCollision = true;
				bulletAttack.trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier;
				bulletAttack.damageType = DamageTypeCombo.GenericPrimary;
				bulletAttack.Fire();
			}
		}

		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;
			modelTransform = base.GetModelTransform();
			Ray aimRay = base.GetAimRay();
			StartAimMode(aimRay, 3f);
			if(step % 2 == 0){
				FireBullet(aimRay, "MuzzleLeft");
				PlayCrossfade("Gesture Left Cannon, Additive", "FireGrenadeLeft", 0.1f);
			} else {
				FireBullet(aimRay, "MuzzleRight");
				PlayCrossfade("Gesture Right Cannon, Additive", "FireGrenadeRight", 0.1f);
			}
		}

		public override void FixedUpdate(){
			base.FixedUpdate();
			if(base.isAuthority && base.fixedAge >= duration){
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit(){
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
