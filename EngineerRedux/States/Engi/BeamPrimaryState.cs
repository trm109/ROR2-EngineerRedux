// <copyright file="BeamPrimaryState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EngineerRedux.States.Engi
{
    using EntityStates;

    // using EntityStates.Engi.EngiWeapon;
    using EntityStates.EngiTurret.EngiTurretWeapon;
    using RoR2;
    using RoR2.Skills;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class BeamPrimaryState : BaseSkillState
    {
        // Lots of code was taken from SS2's Laser Focus ability, ty!
        private static float fireFrequency = 5f; // Default
        private static float damageCoefficient = .4f; // Default
        private static float procCoefficient = 1f; // Default is 0.6f, this is a bit unfair for things like ATG.
        private static float force = 0f;
        private static float maxRange = 300f; // Default is 25, but for useability its buffed to 300.

        // VFX references
        // FireBeam doesn't use static variables for some reason, so I have to assign these at runtime.
        private static GameObject hitEffectPrefab;
        private static GameObject tracerEffectPrefab;

        private static string laserStartSoundString = "Play_engi_r_walkingTurret_laser_start";
        private static string laserLoopSoundString = "Play_engi_r_walkingTurret_laser_loop";
        private static string laserEndSoundString = "Play_engi_r_walkingTurret_laser_end";

        private static int chargeStateHash = Animator.StringToHash("ChargeGrenades");
        private static int emptyStateHash = Animator.StringToHash("Empty");

        // private Transform modelTransform; // reference to engi transform
        private Transform leftMuzzleInstance;
        private Transform rightMuzzleInstance;

        private GameObject leftLaserInstance;
        private Transform leftLaserInstanceEndpoint;
        private GameObject rightLaserInstance;
        private Transform rightLaserInstanceEndpoint;

        private float timeSinceLastFired;

        public static void Init()
        {
            // Assign VFX references
            hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Hitspark1.prefab").WaitForCompletion();
            tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/LaserEngiTurret.prefab").WaitForCompletion();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Transform modelTransform = this.GetModelTransform();
            Ray aimRay = this.GetAimRay();
            this.StartAimMode(aimRay, 3f);

            this.PlayAnimation("Gesture, Additive", chargeStateHash);
            Util.PlaySound(laserStartSoundString, this.gameObject);

            // Get reference to Muzzles and laser prefabs
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    this.leftMuzzleInstance = component.FindChild("MuzzleLeft");
                    this.rightMuzzleInstance = component.FindChild("MuzzleRight");
                    if ((bool)this.leftMuzzleInstance && (bool)this.rightMuzzleInstance && (bool)tracerEffectPrefab)
                    {
                        // Instantiate laser visual indicators
                        this.leftLaserInstance = UnityEngine.Object.Instantiate(tracerEffectPrefab, this.leftMuzzleInstance.position, this.leftMuzzleInstance.rotation);
                        this.leftLaserInstance.transform.parent = this.transform;
                        this.leftLaserInstanceEndpoint = this.leftLaserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");

                        this.rightLaserInstance = UnityEngine.Object.Instantiate(tracerEffectPrefab, this.rightMuzzleInstance.position, this.rightMuzzleInstance.rotation);
                        this.rightLaserInstance.transform.parent = this.transform;
                        this.rightLaserInstanceEndpoint = this.rightLaserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            Ray aimRay = this.GetAimRay();
            this.StartAimMode(aimRay, 2f, false);

            Util.PlaySound(laserLoopSoundString, this.gameObject);

            Vector3 aimEndPoint = this.GetAimEndPoint(aimRay);

            // Visually update laser indicators
            if ((bool)this.leftLaserInstance && (bool)this.leftLaserInstanceEndpoint && (bool)this.leftMuzzleInstance)
            {
                this.leftLaserInstance.transform.position = this.leftMuzzleInstance.position;
                this.leftLaserInstance.transform.rotation = Quaternion.LookRotation(aimRay.direction);
                this.leftLaserInstanceEndpoint.position = aimEndPoint;
            }

            if ((bool)this.rightLaserInstance && (bool)this.rightLaserInstanceEndpoint && (bool)this.rightMuzzleInstance)
            {
                this.rightLaserInstance.transform.position = this.rightMuzzleInstance.position;
                this.rightLaserInstance.transform.rotation = Quaternion.LookRotation(aimRay.direction);
                this.rightLaserInstanceEndpoint.position = aimEndPoint;
            }

            // Fire Bullets at a fixed rate.
            this.timeSinceLastFired += Time.fixedDeltaTime;
            float maxTimeSinceLastFired = 1f / (fireFrequency * this.characterBody.attackSpeed);
            if (this.timeSinceLastFired >= maxTimeSinceLastFired)
            {
                this.FireBullet(aimRay, this.leftMuzzleInstance.position, "MuzzleLeft");
                this.FireBullet(aimRay, this.rightMuzzleInstance.position, "MuzzleRight");
                this.timeSinceLastFired = 0f;
            }

            if (this.isAuthority && !this.inputBank.skill1.down)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            this.PlayAnimation("Gesture, Additive", emptyStateHash);
            Util.PlaySound(laserEndSoundString, this.gameObject);

            if ((bool)this.leftLaserInstance)
            {
                UnityEngine.Object.Destroy(this.leftLaserInstance);
            }

            if ((bool)this.rightLaserInstance)
            {
                UnityEngine.Object.Destroy(this.rightLaserInstance);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        private Vector3 GetAimEndPoint(Ray aimRay)
        {
            // Calculate aim direction, taken from SS2's Engi Laser Ability
            Vector3 aimEndPoint = aimRay.GetPoint(maxRange);
            RaycastHit raycastHit;
            if (Util.CharacterRaycast(this.gameObject, aimRay, out raycastHit, maxRange, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
            {
                aimEndPoint = raycastHit.point;
            }

            return aimEndPoint;
        }

        private void FireBullet(Ray aimRay, Vector3 muzzlePosition, string targetMuzzle)
        {
            // Mostly taken from Commando's M1
            if (this.isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();

                bulletAttack.owner = this.gameObject;
                bulletAttack.weapon = this.gameObject;
                bulletAttack.origin = muzzlePosition;
                Vector3 aimEndPoint = this.GetAimEndPoint(aimRay);
                bulletAttack.aimVector = (aimEndPoint - muzzlePosition).normalized;

                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = this.characterBody.spreadBloomAngle;
                bulletAttack.damage = damageCoefficient * this.damageStat;
                bulletAttack.force = force;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.muzzleName = targetMuzzle;
                bulletAttack.hitEffectPrefab = hitEffectPrefab;
                bulletAttack.isCrit = Util.CheckRoll(this.critStat, this.characterBody.master);
                bulletAttack.radius = 0.1f;
                bulletAttack.smartCollision = true;
                bulletAttack.procCoefficient = procCoefficient;
                bulletAttack.damageType = DamageType.SlowOnHit;
                bulletAttack.damageType.damageSource = DamageSource.Primary;
                bulletAttack.maxDistance = maxRange;
                bulletAttack.Fire();
            }
        }
    }
}
