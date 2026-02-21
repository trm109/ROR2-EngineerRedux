using RoR2;
using RoR2.Projectile;
using EntityStates;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EngineerRedux.EntityStates.Turret
{
	public class GrenadePrimaryState : BaseState
	{
		public static GameObject effectPrefab;
		public static GameObject projectilePrefab;

		public static float damageCoefficient = 1f;
		public static float baseDuration = .05f;
		public static float arcAngle = -3f;
		public static float recoilAmplitude = 0.5f;
		public static float spreadBloomValue = 0.2f;

		private float duration;
		private Transform modelTransform;
		private Ray projectileRay;

		public static string attackSoundString = "Play_engi_M1_shot";

		// Reusing gauss anim
		private static int FireGrenadeStateHash = Animator.StringToHash("FireGauss");
		private static int FireGrenadeParamHash = Animator.StringToHash("FireGauss.playbackRate");

		public static void Init(){
			effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab").WaitForCompletion();
			projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiGrenadeProjectile.prefab").WaitForCompletion();
		}

		private void FireGrenade(){
			Util.PlaySound(attackSoundString, base.gameObject);
			projectileRay = GetAimRay();
			PlayAnimation("Gesture", FireGrenadeStateHash, FireGrenadeParamHash, duration);
			string muzzleName = "Muzzle";
			if((bool)effectPrefab){
				EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
			}
			if(base.isAuthority){
				// I have no idea what any of this is.
				float x = Random.Range(0f, base.characterBody.spreadBloomAngle);
				float z = Random.Range(0f, 360f);
				Vector3 up = Vector3.up;
				Vector3 axis = Vector3.Cross(up, projectileRay.direction);
				Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
				float y = vector.y;
				vector.y = 0f;
				float angle = Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f;
				float angle2 = Mathf.Atan2(y, vector.magnitude) * 57.29578f + arcAngle;
				Vector3 forward = Quaternion.AngleAxis(angle, up) * (Quaternion.AngleAxis(angle2, axis) * projectileRay.direction);
				//
				FireProjectileInfo fireProjectileInfo = new FireProjectileInfo {
					projectilePrefab = projectilePrefab,
					position = projectileRay.origin,
					rotation = Util.QuaternionSafeLookRotation(forward),
					owner = base.gameObject,
					damage = damageStat * damageCoefficient,
					force = 0f,
					crit = Util.CheckRoll(critStat, base.characterBody.master),
					damageTypeOverride = DamageTypeCombo.GenericPrimary
				};
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
			base.characterBody.AddSpreadBloom(spreadBloomValue);
		}

		public override void OnEnter(){
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;
			modelTransform = GetModelTransform();
			StartAimMode();
			FireGrenade();
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

		public override InterruptPriority GetMinimumInterruptPriority(){
			return InterruptPriority.Skill;
		}
	}
}
