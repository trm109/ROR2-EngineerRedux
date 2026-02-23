// <copyright file="GaussPrimaryState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

// Referencing Base Games Engi turret
// using EntityStates.EngiTurret.EngiTurretWeapon;
// using EntityStates.Engi.EngiWeapon;
namespace EngineerRedux.States.Engi
{
    using EntityStates;
    using RoR2;
    using RoR2.Skills;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class GaussPrimaryState : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        private static float baseDuration = 0.35f / 2f; // Turret is default 35, engi basically has two turrets on his back.

        private static float damageCoefficient = 0.7f;
        private static float force = 200f;
        private static float maxRange = 300f;

        // stealing these from Commando's M1
        private static float recoilAmplitude = 1f;

        // private static float spreadBloomValue = 0.3f;
        private static float trajectoryAimAssistMultiplier = 0.75f;

        // VFX references
        private static GameObject muzzleEffectPrefab;
        private static GameObject hitEffectPrefab;
        private static GameObject tracerEffectPrefab;

        // Reusing Gauss Turret SFX
        private static string attackSoundString = "Play_engi_R_turret_shot";

        private float duration;

        private Transform modelTransform; // reference to engi transform

        private int step; // Which muzzle does the next shot come from?

        public static void Init()
        {
            // Assign VFX references
            muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/MuzzleflashEngiTurret.prefab").WaitForCompletion();
            hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/ImpactEngiTurret.prefab").WaitForCompletion();
            tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/TracerEngiTurret.prefab").WaitForCompletion();
        }

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            this.step = i;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            this.modelTransform = this.GetModelTransform();
            Ray aimRay = this.GetAimRay();
            this.StartAimMode(aimRay, 3f);
            if (this.step % 2 == 0)
            {
                this.FireBullet(aimRay, "MuzzleLeft");
                this.PlayCrossfade("Gesture Left Cannon, Additive", "FireGrenadeLeft", 0.1f);
            }
            else
            {
                this.FireBullet(aimRay, "MuzzleRight");
                this.PlayCrossfade("Gesture Right Cannon, Additive", "FireGrenadeRight", 0.1f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.isAuthority && this.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
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

        private void FireBullet(Ray aimRay, string targetMuzzle)
        {
            Util.PlaySound(attackSoundString, this.gameObject);
            Vector3 muzzlePosition = Vector3.zero;

            // if reference to parent object exists
            if ((bool)this.modelTransform)
            {
                // try and get target muzzle object
                ChildLocator component = this.modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    // try and get target muzzle position
                    Transform targetTransform = component.FindChild(targetMuzzle);
                    if ((bool)targetTransform)
                    {
                        // set target muzzle position to target muzzle position
                        muzzlePosition = targetTransform.position;
                    }
                }
            }

            this.AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
            if ((bool)muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(
                        muzzleEffectPrefab,
                        this.gameObject,
                        targetMuzzle,
                        transmit: false);
            }

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
                bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
                bulletAttack.muzzleName = targetMuzzle;
                bulletAttack.hitEffectPrefab = hitEffectPrefab;
                bulletAttack.isCrit = Util.CheckRoll(this.critStat, this.characterBody.master);
                bulletAttack.radius = 0.15f;
                bulletAttack.smartCollision = true;
                bulletAttack.trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier;
                bulletAttack.damageType = DamageTypeCombo.GenericPrimary;
                bulletAttack.Fire();
            }
        }
    }
}
