// <copyright file="BeamPrimaryState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EngineerRedux.States.Turret
{
    using EntityStates;

    // using EntityStates.EngiTurret.EngiTurretWeapon;
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
        private static GameObject hitEffectPrefab;
        private static GameObject tracerEffectPrefab;

        private static string laserStartSoundString = "Play_engi_r_walkingTurret_laser_start";
        private static string laserLoopSoundString = "Play_engi_r_walkingTurret_laser_loop";
        private static string laserEndSoundString = "Play_engi_r_walkingTurret_laser_end";

        private Transform muzzleInstance;
        private GameObject laserInstance;
        private Transform laserInstanceEndpoint;

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

            // PlayAnimation("Gesture, Additive", ChargeStateHash);
            Util.PlaySound(laserStartSoundString, this.gameObject);

            // Get reference to Muzzles and laser prefabs
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    this.muzzleInstance = component.FindChild("Muzzle");
                    if ((bool)this.muzzleInstance && (bool)tracerEffectPrefab)
                    {
                        // Instantiate laser visual indicators
                        this.laserInstance = UnityEngine.Object.Instantiate(tracerEffectPrefab, this.muzzleInstance.position, this.muzzleInstance.rotation);
                        this.laserInstance.transform.parent = this.transform;
                        this.laserInstanceEndpoint = this.laserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
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
            if ((bool)this.laserInstance && (bool)this.laserInstanceEndpoint && (bool)this.muzzleInstance)
            {
                this.laserInstance.transform.position = this.muzzleInstance.position;
                this.laserInstance.transform.rotation = Quaternion.LookRotation(aimRay.direction);
                this.laserInstanceEndpoint.position = aimEndPoint;
            }

            // Fire Bullets at a fixed rate.
            this.timeSinceLastFired += Time.fixedDeltaTime;
            float maxTimeSinceLastFired = 1f / (fireFrequency * this.characterBody.attackSpeed);
            if (this.timeSinceLastFired >= maxTimeSinceLastFired)
            {
                this.FireBullet(aimRay, this.muzzleInstance.position);
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

            // PlayAnimation("Gesture, Additive", EmptyStateHash);
            Util.PlaySound(laserEndSoundString, this.gameObject);

            if ((bool)this.laserInstance)
            {
                UnityEngine.Object.Destroy(this.laserInstance);
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

        private void FireBullet(Ray aimRay, Vector3 muzzlePosition)
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

                // bulletAttack.muzzleName = targetMuzzle;
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
