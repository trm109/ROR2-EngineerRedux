// <copyright file="SkillManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

// TODO make all 'add skill' methods return skillDef, and make skillName and skillDesc parameters optional, in case the caller wants to handle the language api stuff themselves.
namespace EngineerRedux.Utils
{
    using System;
    using BepInEx;
    using EntityStates;
    using On;
    using R2API;
    using RoR2;
    using RoR2.Skills;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    /// <summary>
    /// A utility class for managing Engi's skills and skill families. Provides methods for adding skills to Engi's loadout, creating new skill families, and handling the logic for applying turret body and weapon types on summon. This class is the core mechanism of the EngineerRedux mod.
    /// </summary>
    public static class SkillManager
    {
        private static GameObject engiBodyPrefab;

        private static SkillLocator engiSkillLocator;

        private static SkillFamily engiPrimarySkillFamily;

        private static SkillFamily engiSpecialSkillFamily;

        private static SkillFamily engiTurretWeaponSkillFamily;

        // public static SkillFamily engiTurretBodySkillFamily;
        private static GameObject turretBodyPrefab;

        private static SkillLocator turretSkillLocator;

        private static SkillFamily turretPrimarySkillFamily;

        /// <summary>
        /// Initialization method. Loads prefabs, clears out existing special skills, and adds hooks. Should be idempotent.
        /// </summary>
        public static void Init()
        {
            Log.Info("Initializing SkillManager");

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
            SkillFamily.Variant[] specialVariants = Array.Empty<SkillFamily.Variant>();
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

        /// <summary>
        /// Helper method to create a skill family and a placeholder skill to represent it in the loadout menu. Returns the created skill family.
        /// </summary>
        /// <param name="bodyPrefab">The body prefab to add the placeholder skill to. This should be the character that will be using this skill family.</param>
        /// <param name="skillFamilyName">The name of the skill family. This will be used to generate the name of the placeholder skill and the skill family.</param>
        /// <param name="skillSlotToken">The language token for the skill slot that this skill family will occupy. This will be used to set the loadout title token override for the placeholder skill, which determines the name of the skill slot that shows up in the loadout menu. </param>
        /// <returns>The created <see cref="SkillFamily"/>.</returns>
        public static SkillFamily CreateSkillFamily(GameObject bodyPrefab, string skillFamilyName, string skillSlotToken)
        {
            Log.Debug($"Creating skill family {skillFamilyName}.");
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (skillFamily as ScriptableObject).name = bodyPrefab.name + skillFamilyName;
            skillFamily.variants = Array.Empty<SkillFamily.Variant>();

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

                // TODO refactor this to use AddLanguageTokens
                LanguageAPI.Add(skillSlotToken, skillSlotSpaced);
            }

            return skillFamily;
        }

        /// <summary>
        /// Helper method to add a skill to a skill family, with an optional unlockableDef. Not recommended for direct use as other methods will call this automatically.
        /// </summary>
        /// <param name="skillDef">The skillDef to add to the skill family.</param>
        /// <param name="skillFamily">The skill family to add the skillDef to.</param>
        /// <param name="unlockableDef">OPTIONAL. The unlockableDef that gates the skillDef. If null, the skill will be added without an unlockableDef, and the game will register one automatically.</param>
        /// <param name="isDefaultVariant">OPTIONAL. Whether this skill should be the default variant for the skill family. If true, this skill will be the one that is selected by default in the loadout menu.</param>
        /// <returns>The modified <see cref="SkillFamily"/> with the new skill added.</returns>
#pragma warning disable CS0618 // Variant.unlockableName is deprecated, but there isn't a better way to handle null unlockableDefs AFAIK
        public static SkillFamily AddSkillToFamily(SkillDef skillDef, SkillFamily skillFamily, UnlockableDef unlockableDef = null, bool isDefaultVariant = false)
        {
            Log.Debug($"Adding skill {skillDef.skillName} to skill family index {skillFamily.catalogIndex}.");

            // Create a new variant for the skill family
            SkillFamily.Variant newVariant = new SkillFamily.Variant
            {
                skillDef = skillDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null),
            };

            // Handle null unlockableDef
            if (unlockableDef == null)
            {
                newVariant.unlockableName = string.Empty; // will cause game to register unlockableDef via SkillFamily.UpgradeUnlockableNameToUnlockableDef
            }
            else
            {
                newVariant.unlockableDef = unlockableDef;
            }

            // Resize the variants array to fit the new variant
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);

            if (skillFamily.variants.Length < 1)
            {
                // Something has gone terribly wrong.
                throw new InvalidOperationException("Skill family variants array has invalid length after resizing. This should never happen.");
            }

            // Add the new variant to the skill family
            uint newVariantIndex = (uint)skillFamily.variants.Length - 1u;
            skillFamily.variants[newVariantIndex] = newVariant;

            // if specified, set the new variant as the default variant for the skill family
            if (isDefaultVariant)
            {
                skillFamily.defaultVariantIndex = newVariantIndex;
            }

            return skillFamily;
        }

        /// <summary>
        /// Helper method to add language tokens for a skill's name and description. Generates tokens in the format of ENGI_REDUX_[SKILLNAME]_NAME and ENGI_REDUX_[SKILLNAME]_DESC, where [SKILLNAME] is the provided skillName converted from camelCase to SCREAMING_SNAKE_CASE. Also adds the human readable name and description to the LanguageAPI.
        /// </summary>
        /// <param name="skillDef">The skillDef to add the language tokens to.</param>
        /// <param name="skillName">The name of the skill, in camelCase. This will be used to generate the language tokens.</param>
        /// <param name="skillDescription">The description of the skill. This will be added to the LanguageAPI with the generated description token.</param>
        public static void AddLanguageTokens(SkillDef skillDef, string skillName, string skillDescription)
        {
            Log.Debug($"Adding language tokens for skill {skillName}.");

            // TODO add more err handling; illegal characters, incorrect formatting, etc.
            if (string.IsNullOrEmpty(skillName) || string.IsNullOrEmpty(skillDescription))
            {
                // TODO err log
                return;
            }

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

        /// <summary>
        /// Helper method to add a primary skill to Engi's loadout.
        /// </summary>
        /// <param name="skillDef">The SkillDef of the skill to add.</param>
        /// <param name="skillName">The name of the skill, in camelCase. This will be used to generate the language tokens.</param>
        /// <param name="skillDescription">The description of the skill. This will be added to the LanguageAPI with the generated description token.</param>
        /// <param name="unlockableDef">OPTIONAL. The UnlockableDef that gates the skill. If null, the skill will be added without an unlockableDef, and the game will register one automatically.</param>
        /// <param name="isDefaultVariant">OPTIONAL. Whether this skill should be the default variant for the primary skill family. If true, this skill will be the one that is selected by default in the loadout menu.</param>
        /// <returns>The added <see cref="SkillDef"/>.</returns>
        public static SkillDef AddEngiPrimary(SkillDef skillDef, string skillName, string skillDescription, UnlockableDef unlockableDef = null, bool isDefaultVariant = false)
        {
            Log.Debug($"Adding primary skill {skillName} to Engi's loadout.");
            if (!(bool)engiBodyPrefab || !(bool)engiSkillLocator || !(bool)engiPrimarySkillFamily)
            {
                Init();
            }

            // if activationStateMachineName is null or empty, set it to "Weapon".
            if (string.IsNullOrWhiteSpace(skillDef.activationStateMachineName))
            {
                skillDef.activationStateMachineName = "Weapon";
            }

            // Add language tokens, if provided
            if (!string.IsNullOrEmpty(skillName) && !string.IsNullOrEmpty(skillDescription))
            {
                AddLanguageTokens(skillDef, skillName, skillDescription);
            }

            // Add the new skill to the primary skill family
            AddSkillToFamily(skillDef, engiPrimarySkillFamily, unlockableDef, isDefaultVariant);

            // Add the new skill to the content pack
            ContentAddition.AddSkillDef(skillDef);

            return skillDef;
        }

        /// <summary>
        /// Helper method for adding a selectable turret weapon type to Engi's loadout. non default skill slot.
        /// </summary>
        /// <param name="turretSkillDef">The SkillDef of the turret's primary skill. This should be a SkillDef that is designed to be used as the primary skill of a turret.</param>
        /// <param name="skillName">The name of the skill, in camelCase. This will be used to generate the language tokens.</param>
        /// <param name="skillDescription">The description of the skill. This will be added to the LanguageAPI with the generated description token.</param>
        /// <param name="unlockableDef">OPTIONAL. The UnlockableDef that gates the skill. If null, the skill will be added without an unlockableDef, and the game will register one automatically.</param>
        /// <param name="isDefaultVariant">OPTIONAL. Whether this skill should be the default variant for the turret weapon skill family. If true, this skill will be the one that is selected by default in the loadout menu.</param>
        /// <returns>The selectable <see cref="SkillDef"/> that represents the turret weapon type in Engi's loadout. This is the skill that the player will see and select in the loadout menu, NOT the skill that the turrets will use.</returns>
        public static SkillDef AddEngiTurretWeapon(SkillDef turretSkillDef, string skillName, string skillDescription, UnlockableDef unlockableDef = null, bool isDefaultVariant = false)
        {
            Log.Debug($"Adding turret weapon skill {skillName} to Engi's loadout.");
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

            engiSkillDef.SetSelectedPrimarySkillDef(turretSkillDef);
            engiSkillDef.activationState = new SerializableEntityStateType(typeof(Idle));
            engiSkillDef.activationStateMachineName = "Weapon";

            // Add language tokens, if provided
            if (!string.IsNullOrEmpty(skillName) && !string.IsNullOrEmpty(skillDescription))
            {
                AddLanguageTokens(engiSkillDef, skillName, skillDescription);
            }

            // dont bother adding language tokens for the turret skill since it will never be seen by the player

            // Add the new skill to the turret weapon skill family
            AddSkillToFamily(engiSkillDef, engiTurretWeaponSkillFamily, unlockableDef);

            // Add the new skill to the turret primary skill family
            AddSkillToFamily(turretSkillDef, turretPrimarySkillFamily);

            // Add the engi skill to the content pack
            ContentAddition.AddSkillDef(engiSkillDef);

            // Add the turret skill to the content pack
            ContentAddition.AddSkillDef(turretSkillDef);

            return engiSkillDef;
        }

        // Adds a special skill to Engi's loadout that represents a turret weapon type and also adds the turret body stats to the skillDef for use in OnServerMasterSummonGlobal.

        /// <summary>
        /// Adds a selectable turret body type to Engi's loadout as a special skill. Uses the default "Special" skill slot. SkillDef is expected to have an activation state that is derived from EntityStates.Engi.EngiWeapon.PlaceTurret.
        /// </summary>
        /// <param name="skillDef">The SkillDef of the skill to add. This should be a SkillDef that is designed to be used as the special skill for selecting a turret body type. It is expected to have an activation state that is derived from EntityStates.Engi.EngiWeapon.PlaceTurret, but this is not strictly required.</param>
        /// <param name="skillName">The name of the skill, in camelCase. This will be used to generate the language tokens.</param>
        /// <param name="skillDescription">The description of the skill. This will be added to the LanguageAPI with the generated description token.</param>
        /// <param name="turretBodyStats">The stats to apply to the turret when it is summoned. This will be stored in the skillDef and applied to the turret in the OnServerMasterSummonGlobal hook.</param>
        /// <param name="unlockableDef">OPTIONAL. The UnlockableDef that gates the skill. If null, the skill will be added without an unlockableDef, and the game will register one automatically.</param>
        /// <param name="isDefaultVariant">OPTIONAL. Whether this skill should be the default variant for the special skill family. If true, this skill will be the one that is selected by default in the loadout menu.</param>
        /// <returns>The selectable <see cref="SkillDef"/> that represents the turret body type in Engi's loadout.</returns>
        public static SkillDef AddEngiTurretBody(SkillDef skillDef, string skillName, string skillDescription, TurretBodyStats turretBodyStats, UnlockableDef unlockableDef = null, bool isDefaultVariant = false)
        {
            Log.Debug($"Adding turret body skill {skillName} to Engi's loadout.");
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
            engiSkillDef.SetTurretBodyStats(turretBodyStats);

            // Add language tokens, if provided
            if (!string.IsNullOrEmpty(skillName) && !string.IsNullOrEmpty(skillDescription))
            {
                AddLanguageTokens(engiSkillDef, skillName, skillDescription);
            }

            // Add the new skill to the turret body skill family
            AddSkillToFamily(engiSkillDef, engiSpecialSkillFamily, unlockableDef);

            // Add the new skill to the content pack
            ContentAddition.AddSkillDef(engiSkillDef);

            return engiSkillDef;
        }

        // On summon of a turret.
        private static void OnServerMasterSummonGlobal(RoR2.MasterSummon.MasterSummonReport report)
        {
            CharacterBody summonBody = report.summonBodyInstance;

            // Check if its one of our turrets. This value should be the same for both walker and stationary. For custom turrets, be sure to set this correctly!
            if (summonBody.baseNameToken != "ENGITURRET_BODY_NAME")
            {
                return;
            }

            Log.Debug("Turret summoned, applying stats and skills.");

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
                summonBody.baseMaxHealth = bodySkillDef.TurretBodyStats.MaxHealth;
                summonBody.levelMaxHealth = bodySkillDef.TurretBodyStats.MaxHealthInc;

                summonBody.baseRegen = bodySkillDef.TurretBodyStats.HealthRegen;
                summonBody.levelRegen = bodySkillDef.TurretBodyStats.HealthRegenInc;

                summonBody.baseArmor = bodySkillDef.TurretBodyStats.Armor;

                summonBody.baseMoveSpeed = bodySkillDef.TurretBodyStats.Movespeed;
            }
            else
            {
                // TODO err log
                return;
            }

            // Turret Weapon Handling
            if (weaponSkillDef != null)
            {
                summonBody.skillLocator.primary.SetBaseSkill(weaponSkillDef.SelectedPrimarySkillDef);
            }
            else
            {
                // TODO err log
                return;
            }
        }
    }
}
