// <copyright file="TurretWeaponSkillDef.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EngineerRedux.Utils
{
    using System;
    using RoR2;
    using RoR2.Skills;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    /// <summary>
    /// A <see cref="SkillDef"/> that is used to replace the turrets' primary skillDef with the one selected by the player in the Loadout menu. This is necessary because the turrets' primary skillDef is not set until the player selects a primary skill for the turrets in the Loadout menu, and we need a way to store the selected skillDef until it is set on the turrets.
    /// </summary>
    public class TurretWeaponSkillDef : SkillDef
    {
        /// <summary>
        /// Gets the skillDef that will replace the turrets' primary skillDef. This is set in the Loadout menu when the player selects a primary skill for the turrets.
        /// </summary>
        public SkillDef SelectedPrimarySkillDef { get; private set; }

        /// <summary>
        /// Sets the <see cref="SelectedPrimarySkillDef"/> to the given <see cref="SkillDef"/>.
        /// </summary>
        /// <param name="skillDef">The <see cref="SkillDef"/> to set as the <see cref="SelectedPrimarySkillDef"/>.</param>
        public void SetSelectedPrimarySkillDef(SkillDef skillDef)
        {
            this.SelectedPrimarySkillDef = skillDef;
        }
    }
}
