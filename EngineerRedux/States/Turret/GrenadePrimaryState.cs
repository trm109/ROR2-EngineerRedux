// <copyright file="GrenadePrimaryState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EngineerRedux.States.Turret
{
    using EntityStates;
    using RoR2;
    using RoR2.Projectile;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class GrenadePrimaryState : BaseState
    {
        private static GameObject effectPrefab;
        private static GameObject projectilePrefab;

        private static float damageCoefficient = 1f;
        private static float baseDuration = .05f;
        private static float arcAngle = -3f;
        private static float spreadBloomValue = 0.2f;

        private static string attackSoundString = "Play_engi_M1_shot";

        // Reusing gauss anim
        private static int fireGrenadeStateHash = Animator.StringToHash("FireGauss");
        private static int fireGrenadeParamHash = Animator.StringToHash("FireGauss.playbackRate");

        // private static float recoilAmplitude = 0.5f;
        private float duration;
        private Transform modelTransform;
        private Ray projectileRay;

        public static void Init()
        {
            effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab").WaitForCompletion();
            projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiGrenadeProjectile.prefab").WaitForCompletion();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            this.modelTransform = this.GetModelTransform();
            this.StartAimMode();
            this.FireGrenade();
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
            return InterruptPriority.Skill;
        }

        private void FireGrenade()
        {
            Util.PlaySound(attackSoundString, this.gameObject);
            this.projectileRay = this.GetAimRay();
            this.PlayAnimation("Gesture", fireGrenadeStateHash, fireGrenadeParamHash, this.duration);
            string muzzleName = "Muzzle";
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, this.gameObject, muzzleName, transmit: false);
            }

            if (this.isAuthority)
            {
                // I have no idea what any of this is.
                float x = Random.Range(0f, this.characterBody.spreadBloomAngle);
                float z = Random.Range(0f, 360f);
                Vector3 up = Vector3.up;
                Vector3 axis = Vector3.Cross(up, this.projectileRay.direction);
                Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
                float y = vector.y;
                vector.y = 0f;
                float angle = (Mathf.Atan2(vector.z, vector.x) * 57.29578f) - 90f;
                float angle2 = (Mathf.Atan2(y, vector.magnitude) * 57.29578f) + arcAngle;
                Vector3 forward = Quaternion.AngleAxis(angle, up) * (Quaternion.AngleAxis(angle2, axis) * this.projectileRay.direction);

                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = projectilePrefab,
                    position = this.projectileRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(forward),
                    owner = this.gameObject,
                    damage = this.damageStat * damageCoefficient,
                    force = 0f,
                    crit = Util.CheckRoll(this.critStat, this.characterBody.master),
                    damageTypeOverride = DamageTypeCombo.GenericPrimary,
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            this.characterBody.AddSpreadBloom(spreadBloomValue);
        }
    }
}
