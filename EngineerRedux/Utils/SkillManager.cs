using RoR2;
using RoR2.Skills;
using R2API;
using BepInEx;
using EntityStates;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using On;

namespace EngineerRedux.Utils;
public static class SkillManager
{
    public static GameObject engiBodyPrefab;
    public static SkillLocator engiSkillLocator;
    public static SkillFamily engiPrimarySkillFamily;
    public static SkillFamily engiSpecialSkillFamily;
    public static SkillFamily engiTurretWeaponSkillFamily;
    // public static SkillFamily engiTurretBodySkillFamily;

    public static GameObject turretBodyPrefab;
    public static SkillLocator turretSkillLocator;
    public static SkillFamily turretPrimarySkillFamily;

    public static void Init()
    {
        // Load Prefabs
        engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();
        turretBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiTurretBody.prefab").WaitForCompletion();

        // Skill Locators
        engiSkillLocator = engiBodyPrefab.GetComponent<SkillLocator>();
        turretSkillLocator = turretBodyPrefab.GetComponent<SkillLocator>();

        // Default Skill Families
        engiPrimarySkillFamily = engiSkillLocator.primary.skillFamily;
        engiSpecialSkillFamily = engiSkillLocator.special.skillFamily;
        turretPrimarySkillFamily = turretSkillLocator.primary.skillFamily;

        // Custom Skill Families
        // engiTurretBodySkillFamily = CreateSkillFamily(engiBodyPrefab, "TurretBody", "ENGI_REDUX_TURRET_BODY_SLOT");
        engiTurretWeaponSkillFamily = CreateSkillFamily(engiBodyPrefab, "TurretWeapon", "ENGI_REDUX_TURRET_WEAPON_SLOT");
        // ContentAddition.AddSkillFamily(engiTurretBodySkillFamily);
        ContentAddition.AddSkillFamily(engiTurretWeaponSkillFamily);

        // Clear Engi Special variant skillDefs that are not TurretBodySkillDefs for compat
        SkillFamily.Variant[] specialVariants = new SkillFamily.Variant[0];
        foreach (SkillFamily.Variant variant in engiSpecialSkillFamily.variants)
        {
            if (variant.skillDef is TurretBodySkillDef)
            {
                Array.Resize(ref specialVariants, specialVariants.Length + 1);
                specialVariants[specialVariants.Length - 1] = variant;
            }
        }
        engiSpecialSkillFamily.variants = specialVariants;

        // Add hook on Turret Summon to swap to desired turret weapon and body types
        RoR2.MasterSummon.onServerMasterSummonGlobal += OnServerMasterSummonGlobal;
    }

    //TODO refactor this to use AddLanguageTokens
    public static SkillFamily CreateSkillFamily(GameObject bodyPrefab, string skillFamilyName, string skillSlotToken)
    {
        SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
        (skillFamily as ScriptableObject).name = bodyPrefab.name + skillFamilyName;
        skillFamily.variants = new SkillFamily.Variant[0];

        // Creating a placeholder skill is required to get the game to show this skill family in the loadout menu.
        GenericSkill placeHolderSkill = bodyPrefab.AddComponent<GenericSkill>();
        placeHolderSkill.skillName = skillFamilyName;
        placeHolderSkill.hideInCharacterSelect = true;
        placeHolderSkill._skillFamily = skillFamily;

        if (!string.IsNullOrEmpty(skillSlotToken))
        {
            // placeHolderSkill.SetLoadoutTitleTokenOverride(skillSlotToken);
            placeHolderSkill.loadoutTitleToken = skillSlotToken;
            // Convert camelCase `TurretWeapon` to spaced `Turret Weapon` for the language token
            string skillSlotSpaced = System.Text.RegularExpressions.Regex.Replace(skillFamilyName, "(\\B[A-Z])", " $1");
            LanguageAPI.Add(skillSlotToken, skillSlotSpaced);
        }

        return skillFamily;
    }

    [Obsolete]
    public static void AddSkillToFamily(SkillDef skillDef, SkillFamily skillFamily, UnlockableDef unlockableDef = null)
    {
        // Create a new variant for the skill family
        SkillFamily.Variant newVariant = new SkillFamily.Variant
        {
            skillDef = skillDef,
            viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
        };
        // Handle null unlockableDef
        if (unlockableDef == null)
        {
            newVariant.unlockableName = ""; // will cause game to register unlockableDef via SkillFamily.UpgradeUnlockableNameToUnlockableDef
        }
        else
        {
            newVariant.unlockableDef = unlockableDef;
        }
        // Resize the variants array to fit the new variant
        Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
        // Add the new variant to the skill family
        skillFamily.variants[skillFamily.variants.Length - 1] = newVariant;
        return;
    }

    public static void AddLanguageTokens(SkillDef skillDef, string skillName, string skillDescription)
    {
        // Generate a name token
        // SkillNameExample -> SKILL_NAME_EXAMPLE
        string nameToken = "ENGI_REDUX_" + System.Text.RegularExpressions.Regex.Replace(skillName, "(\\B[A-Z])", "_$1").ToUpper() + "_NAME";
        string descriptionToken = "ENGI_REDUX_" + System.Text.RegularExpressions.Regex.Replace(skillName, "(\\B[A-Z])", "_$1").ToUpper() + "_DESC";

        skillDef.skillName = nameToken;
        skillDef.skillNameToken = nameToken;
        skillDef.skillDescriptionToken = descriptionToken;

        string humanReadableName = System.Text.RegularExpressions.Regex.Replace(skillName, "(\\B[A-Z])", " $1");

        LanguageAPI.Add(nameToken, humanReadableName);
        LanguageAPI.Add(descriptionToken, skillDescription);

        return;
    }

    public static void AddEngiPrimary(SkillDef skillDef, string skillName, string skillDescription, UnlockableDef unlockableDef = null)
    {
        if (!(bool)engiBodyPrefab || !(bool)engiSkillLocator || !(bool)engiPrimarySkillFamily)
        {
            Init();
        }

        // if activationStateMachineName is null or empty, set it to "Weapon".
        if (string.IsNullOrWhiteSpace(skillDef.activationStateMachineName))
        {
            skillDef.activationStateMachineName = "Weapon";
        }

        // Add language tokens
        AddLanguageTokens(skillDef, skillName, skillDescription);
        // Add the new skill to the primary skill family
        AddSkillToFamily(skillDef, engiPrimarySkillFamily, unlockableDef);
        // Add the new skill to the content pack
        ContentAddition.AddSkillDef(skillDef);

        return;
    }

    public static void AddEngiTurretWeapon(SkillDef turretSkillDef, string skillName, string skillDescription, UnlockableDef unlockableDef = null)
    {
        if (!(bool)engiBodyPrefab || !(bool)engiSkillLocator || !(bool)engiTurretWeaponSkillFamily || !(bool)turretPrimarySkillFamily)
        {
            Init();
        }
        // if activationStateMachineName is null or empty, set it to "Weapon".
        if (string.IsNullOrEmpty(turretSkillDef.activationStateMachineName))
        {
            turretSkillDef.activationStateMachineName = "Weapon";
        }
        TurretWeaponSkillDef engiSkillDef = ScriptableObject.CreateInstance<TurretWeaponSkillDef>();
        engiSkillDef.icon = turretSkillDef.icon;
        engiSkillDef.skillName = turretSkillDef.skillName;
        engiSkillDef.skillNameToken = turretSkillDef.skillNameToken;
        engiSkillDef.skillDescriptionToken = turretSkillDef.skillDescriptionToken;
        engiSkillDef.keywordTokens = turretSkillDef.keywordTokens;

        engiSkillDef.selectedPrimarySkillDef = turretSkillDef;
        engiSkillDef.activationState = new SerializableEntityStateType(typeof(Idle));
        engiSkillDef.activationStateMachineName = "Weapon";

        // Add language tokens
        AddLanguageTokens(engiSkillDef, skillName, skillDescription);
        // dont bother adding language tokens for the turret skill since it will never be seen by the player

        // Add the new skill to the turret weapon skill family
        AddSkillToFamily(engiSkillDef, engiTurretWeaponSkillFamily, unlockableDef);
        // Add the new skill to the turret primary skill family
        AddSkillToFamily(turretSkillDef, turretPrimarySkillFamily);

        // Add the engi skill to the content pack
        ContentAddition.AddSkillDef(engiSkillDef);
        // Add the turret skill to the content pack
        ContentAddition.AddSkillDef(turretSkillDef);

        return;
    }

    public static void AddEngiTurretBody(SkillDef skillDef, string skillName, string skillDescription, TurretBodyStats turretBodyStats, UnlockableDef unlockableDef = null)
    {
        if (!(bool)engiBodyPrefab || !(bool)engiSkillLocator || !(bool)engiSpecialSkillFamily)
        {
            Init();
        }
        // Create a skillDef to represent our turret body type in the loadout menu.
        TurretBodySkillDef engiSkillDef = ScriptableObject.CreateInstance<TurretBodySkillDef>();
        // copy every field from skillDef onto engiSkillDef
        // why, C# & Unity, is this the 'correct' way to copy values from a base class to a derived class.
        engiSkillDef.skillName = skillDef.skillName;
        engiSkillDef.skillNameToken = skillDef.skillNameToken;
        engiSkillDef.skillDescriptionToken = skillDef.skillDescriptionToken;
        engiSkillDef.keywordTokens = skillDef.keywordTokens;
        engiSkillDef.icon = skillDef.icon;
        engiSkillDef.activationStateMachineName = skillDef.activationStateMachineName;
        engiSkillDef.activationState = skillDef.activationState;
        engiSkillDef.interruptPriority = skillDef.interruptPriority;
        engiSkillDef.baseRechargeInterval = skillDef.baseRechargeInterval;
        engiSkillDef.baseMaxStock = skillDef.baseMaxStock;
        engiSkillDef.rechargeStock = skillDef.rechargeStock;
        engiSkillDef.requiredStock = skillDef.requiredStock;
        engiSkillDef.stockToConsume = skillDef.stockToConsume;
        engiSkillDef.attackSpeedBuffsRestockSpeed = skillDef.attackSpeedBuffsRestockSpeed;
        engiSkillDef.attackSpeedBuffsRestockSpeed_Multiplier = skillDef.attackSpeedBuffsRestockSpeed_Multiplier;
        engiSkillDef.resetCooldownTimerOnUse = skillDef.resetCooldownTimerOnUse;
        engiSkillDef.fullRestockOnAssign = skillDef.fullRestockOnAssign;
        engiSkillDef.dontAllowPastMaxStocks = skillDef.dontAllowPastMaxStocks;
        engiSkillDef.beginSkillCooldownOnSkillEnd = skillDef.beginSkillCooldownOnSkillEnd;
        engiSkillDef.isCooldownBlockedUntilManuallyReset = skillDef.isCooldownBlockedUntilManuallyReset;
        engiSkillDef.cancelSprintingOnActivation = skillDef.cancelSprintingOnActivation;
        engiSkillDef.forceSprintDuringState = skillDef.forceSprintDuringState;
        engiSkillDef.canceledFromSprinting = skillDef.canceledFromSprinting;
        engiSkillDef.isCombatSkill = skillDef.isCombatSkill;
        engiSkillDef.mustKeyPress = skillDef.mustKeyPress;
        engiSkillDef.triggeredByPressRelease = skillDef.triggeredByPressRelease;
        engiSkillDef.autoHandleLuminousShot = skillDef.autoHandleLuminousShot;
        engiSkillDef.suppressSkillActivation = skillDef.suppressSkillActivation;
        engiSkillDef.hideStockCount = skillDef.hideStockCount;
        engiSkillDef.hideCooldown = skillDef.hideCooldown;
        // Add turret body stats
        engiSkillDef.turretBodyStats = turretBodyStats; //NRE

        // Add language tokens
        AddLanguageTokens(engiSkillDef, skillName, skillDescription);
        // Add the new skill to the turret body skill family
        AddSkillToFamily(engiSkillDef, engiSpecialSkillFamily, unlockableDef);
        // Add the new skill to the content pack
        ContentAddition.AddSkillDef(engiSkillDef);

        return;
    }

    private static void OnServerMasterSummonGlobal(RoR2.MasterSummon.MasterSummonReport report)
    {
        CharacterBody summonBody = report.summonBodyInstance;
        //Check if its one of our turrets. This value should be the same for both walker and stationary. For custom turrets, be sure to set this correctly!
        if (summonBody.baseNameToken != "ENGITURRET_BODY_NAME")
        {
            return;
        }
        // Grab references to ownerBody and ownerSkillLocator
        CharacterBody ownerBody = report.leaderBodyInstance;
        SkillLocator ownerSkillLocator = ownerBody.skillLocator;
        // Init relevant skillDefs for later use
        TurretBodySkillDef bodySkillDef = null;
        TurretWeaponSkillDef weaponSkillDef = null;
        // Set skillDefs if we can find them.
        foreach (GenericSkill skill in ownerSkillLocator.AllSkills)
        {
            // Find Turret Body Type to change base stats
            if (skill.skillDef is TurretBodySkillDef)
            {
                bodySkillDef = skill.skillDef as TurretBodySkillDef;
            }
            // Find Turret Weapon Type to change the turret's primary skill
            if (skill.skillDef is TurretWeaponSkillDef)
            {
                weaponSkillDef = skill.skillDef as TurretWeaponSkillDef;
            }
        }
        // Handle logic now that we have references to instantiated turret & owner skills.
        // Turret Body Handling
        if (bodySkillDef != null)
        {
            summonBody.baseMaxHealth = bodySkillDef.turretBodyStats.maxHealth;
            summonBody.levelMaxHealth = bodySkillDef.turretBodyStats.maxHealthInc;

            summonBody.baseRegen = bodySkillDef.turretBodyStats.healthRegen;
            summonBody.levelRegen = bodySkillDef.turretBodyStats.healthRegenInc;

            summonBody.baseArmor = bodySkillDef.turretBodyStats.armor;

            summonBody.baseMoveSpeed = bodySkillDef.turretBodyStats.movespeed;
        }
        else
        {
            // TODO err log
            return;
        }
        // Turret Weapon Handling
        if (weaponSkillDef != null)
        {
            summonBody.skillLocator.primary.SetBaseSkill(weaponSkillDef.selectedPrimarySkillDef);
        }
        else
        {
            // TODO err log
            return;
        }
    }
}
