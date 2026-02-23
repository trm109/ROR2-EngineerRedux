// <copyright file="TurretBodySkillDef.cs" company="PlaceholderCompany">
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
    /// A <see cref="SkillDef"/> that contains a <see cref="TurretBodyStats"/>. This is used to create the turret body skills.
    /// </summary>
    public class TurretBodySkillDef : SkillDef
    {
        /// <summary>
        /// Gets the <see cref="TurretBodyStats"/> that will be applied to the turret body when it is summoned. This will replace the turrets stats after it is summoned.
        /// </summary>
        public TurretBodyStats TurretBodyStats { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TurretBodySkillDef"/> class. This will replace the turrets stats after it is summoned.
        /// </summary>
        /// <param name="turretBodyStats">The <see cref="TurretBodyStats"/> that will be applied to the turret body when it is summoned.</param>
        public void SetTurretBodyStats(TurretBodyStats turretBodyStats)
        {
            this.TurretBodyStats = turretBodyStats;
        }
    }
}
