using RoR2;
using RoR2.Skills;

using EntityStates;
using UnityEngine;
using UnityEngine.AddressableAssets;
// using EntityStates.Engi.EngiWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;

namespace EngineerRedux.EntityStates.Engi
{
	public class BeamPrimaryState : BaseSkillState
	{
		// Lots of code was taken from SS2's Laser Focus ability, ty!
		public static float fireFrequency = 5f; // Default
		public static float damageCoefficient = .4f; // Default
		public static float procCoefficient = 1f; // Default is 0.6f, this is a bit unfair for things like ATG.
		public static float force = 0f;
		public static float maxRange = 300f; // Default is 25, but for useability its buffed to 300.

		// VFX references
		// FireBeam doesn't use static variables for some reason, so I have to assign these at runtime.
		public static GameObject hitEffectPrefab;
		public static GameObject tracerEffectPrefab;

		public static string laserStartSoundString = "Play_engi_r_walkingTurret_laser_start";
		public static string laserLoopSoundString = "Play_engi_r_walkingTurret_laser_loop";
		public static string laserEndSoundString = "Play_engi_r_walkingTurret_laser_end";

		// private Transform modelTransform; // reference to engi transform

		private Transform leftMuzzleInstance;
		private Transform rightMuzzleInstance;

		private GameObject leftLaserInstance;
		private Transform leftLaserInstanceEndpoint;
		private GameObject rightLaserInstance;
		private Transform rightLaserInstanceEndpoint;

		private float timeSinceLastFired;

		private static int ChargeStateHash = Animator.StringToHash("ChargeGrenades");
		private static int EmptyStateHash = Animator.StringToHash("Empty");

		public static void Init()
		{
			// Assign VFX references
			hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Hitspark1.prefab").WaitForCompletion();
			tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/LaserEngiTurret.prefab").WaitForCompletion();
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

		private void FireBullet(Ray aimRay, Vector3 muzzlePosition, string targetMuzzle){
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
				bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
				bulletAttack.muzzleName = targetMuzzle;
				bulletAttack.hitEffectPrefab = hitEffectPrefab;
				bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
				bulletAttack.radius = 0.1f;
				bulletAttack.smartCollision = true;
				bulletAttack.procCoefficient = procCoefficient;
				bulletAttack.damageType = DamageType.SlowOnHit;
				bulletAttack.damageType.damageSource = DamageSource.Primary;
				bulletAttack.maxDistance = maxRange;
				bulletAttack.Fire();
			}
		}

		public override void OnEnter()
		{
			base.OnEnter();
			Transform modelTransform = base.GetModelTransform();
			Ray aimRay = base.GetAimRay();
			StartAimMode(aimRay, 3f);

			PlayAnimation("Gesture, Additive", ChargeStateHash);
			Util.PlaySound(laserStartSoundString, base.gameObject);

			// Get reference to Muzzles and laser prefabs
			if((bool)modelTransform){
				ChildLocator component = modelTransform.GetComponent<ChildLocator>();
				if((bool)component){
					leftMuzzleInstance = component.FindChild("MuzzleLeft");
					rightMuzzleInstance = component.FindChild("MuzzleRight");
					if((bool)leftMuzzleInstance && (bool)rightMuzzleInstance && (bool)tracerEffectPrefab){
						// Instantiate laser visual indicators
						leftLaserInstance = UnityEngine.Object.Instantiate(tracerEffectPrefab, leftMuzzleInstance.position, leftMuzzleInstance.rotation);
						leftLaserInstance.transform.parent = transform;
						leftLaserInstanceEndpoint = leftLaserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");

						rightLaserInstance = UnityEngine.Object.Instantiate(tracerEffectPrefab, rightMuzzleInstance.position, rightMuzzleInstance.rotation);
						rightLaserInstance.transform.parent = transform;
						rightLaserInstanceEndpoint = rightLaserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
					}
				}
			}
		}

		public override void FixedUpdate(){
			base.FixedUpdate();

			Ray aimRay = base.GetAimRay();
			StartAimMode(aimRay, 2f, false);

			Util.PlaySound(laserLoopSoundString, base.gameObject);

			Vector3 aimEndPoint = GetAimEndPoint(aimRay);
			// Visually update laser indicators
			if((bool)leftLaserInstance && (bool) leftLaserInstanceEndpoint && (bool) leftMuzzleInstance){
				leftLaserInstance.transform.position = leftMuzzleInstance.position;
				leftLaserInstance.transform.rotation = Quaternion.LookRotation(aimRay.direction);
				leftLaserInstanceEndpoint.position = aimEndPoint;
			}
			if((bool)rightLaserInstance && (bool) rightLaserInstanceEndpoint && (bool) rightMuzzleInstance){
				rightLaserInstance.transform.position = rightMuzzleInstance.position;
				rightLaserInstance.transform.rotation = Quaternion.LookRotation(aimRay.direction);
				rightLaserInstanceEndpoint.position = aimEndPoint;
			}

			// Fire Bullets at a fixed rate.
			timeSinceLastFired += Time.fixedDeltaTime;
			float maxTimeSinceLastFired = 1f / (fireFrequency * base.characterBody.attackSpeed);
			if(timeSinceLastFired >= maxTimeSinceLastFired){
				FireBullet(aimRay, leftMuzzleInstance.position, "MuzzleLeft");
				FireBullet(aimRay, rightMuzzleInstance.position, "MuzzleRight");
				timeSinceLastFired = 0f;
			}

			if(base.isAuthority && !inputBank.skill1.down){
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit(){
			base.OnExit();

			PlayAnimation("Gesture, Additive", EmptyStateHash);
			Util.PlaySound(laserEndSoundString, base.gameObject);

			if((bool)leftLaserInstance){
				UnityEngine.Object.Destroy(leftLaserInstance);
			}
			if((bool)rightLaserInstance){
				UnityEngine.Object.Destroy(rightLaserInstance);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
