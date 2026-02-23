using RoR2;
using RoR2.Skills;

using EntityStates;
using UnityEngine;
using UnityEngine.AddressableAssets;
// using EntityStates.Engi.EngiWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;

namespace EngineerRedux.EntityStates.Turret;
public class BeamPrimaryState : BaseSkillState
{
    // Lots of code was taken from SS2's Laser Focus ability, ty!
    public static float fireFrequency = 5f; // Default
    public static float damageCoefficient = .4f; // Default
    public static float procCoefficient = 1f; // Default is 0.6f, this is a bit unfair for things like ATG.
    public static float force = 0f;
    public static float maxRange = 300f; // Default is 25, but for useability its buffed to 300.

    // VFX references
    public static GameObject hitEffectPrefab;
    public static GameObject tracerEffectPrefab;

    public static string laserStartSoundString = "Play_engi_r_walkingTurret_laser_start";
    public static string laserLoopSoundString = "Play_engi_r_walkingTurret_laser_loop";
    public static string laserEndSoundString = "Play_engi_r_walkingTurret_laser_end";

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
    private Vector3 GetAimEndPoint(Ray aimRay)
    {
        // Calculate aim direction, taken from SS2's Engi Laser Ability
        Vector3 aimEndPoint = aimRay.GetPoint(maxRange);
        RaycastHit raycastHit;
        if (Util.CharacterRaycast(base.gameObject, aimRay, out raycastHit, maxRange, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
        {
            aimEndPoint = raycastHit.point;
        }
        return aimEndPoint;
    }

    private void FireBullet(Ray aimRay, Vector3 muzzlePosition, string targetMuzzle)
    {
        // Mostly taken from Commando's M1
        if (base.isAuthority)
        {
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
            // bulletAttack.muzzleName = targetMuzzle;
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

        // PlayAnimation("Gesture, Additive", ChargeStateHash);
        Util.PlaySound(laserStartSoundString, base.gameObject);

        // Get reference to Muzzles and laser prefabs
        if ((bool)modelTransform)
        {
            ChildLocator component = modelTransform.GetComponent<ChildLocator>();
            if ((bool)component)
            {
                muzzleInstance = component.FindChild("Muzzle");
                if ((bool)muzzleInstance && (bool)tracerEffectPrefab)
                {
                    // Instantiate laser visual indicators
                    laserInstance = UnityEngine.Object.Instantiate(tracerEffectPrefab, muzzleInstance.position, muzzleInstance.rotation);
                    laserInstance.transform.parent = transform;
                    laserInstanceEndpoint = laserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        Ray aimRay = base.GetAimRay();
        StartAimMode(aimRay, 2f, false);

        Util.PlaySound(laserLoopSoundString, base.gameObject);

        Vector3 aimEndPoint = GetAimEndPoint(aimRay);
        // Visually update laser indicators
        if ((bool)laserInstance && (bool)laserInstanceEndpoint && (bool)muzzleInstance)
        {
            laserInstance.transform.position = muzzleInstance.position;
            laserInstance.transform.rotation = Quaternion.LookRotation(aimRay.direction);
            laserInstanceEndpoint.position = aimEndPoint;
        }

        // Fire Bullets at a fixed rate.
        timeSinceLastFired += Time.fixedDeltaTime;
        float maxTimeSinceLastFired = 1f / (fireFrequency * base.characterBody.attackSpeed);
        if (timeSinceLastFired >= maxTimeSinceLastFired)
        {
            FireBullet(aimRay, muzzleInstance.position, "Muzzle");
            timeSinceLastFired = 0f;
        }

        if (base.isAuthority && !inputBank.skill1.down)
        {
            outer.SetNextStateToMain();
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        // PlayAnimation("Gesture, Additive", EmptyStateHash);
        Util.PlaySound(laserEndSoundString, base.gameObject);

        if ((bool)laserInstance)
        {
            UnityEngine.Object.Destroy(laserInstance);
        }
    }

    public override InterruptPriority GetMinimumInterruptPriority()
    {
        return InterruptPriority.PrioritySkill;
    }
}
