// <copyright file="TurretBodyStats.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EngineerRedux.Utils
{
    using System;

    /// <summary>
    /// A struct that holds the stats for a turret body. This is used to easily modify the stats of the turret body without having to modify the code in multiple places.
    /// </summary>
    public struct TurretBodyStats
    {
        /// <summary>
        /// The default stats for the turret body. Based on the vanilla stats of engi's turrets.
        /// </summary>
        public static readonly TurretBodyStats Default = new TurretBodyStats
        {
            MaxHealth = 130f,
            MaxHealthInc = 39f,
            HealthRegen = 0.6f,
            HealthRegenInc = 0.12f,
            Armor = 0f,
            Movespeed = 7f,
        };

        // TODO add the rest of the stats.

        /// <summary>The maximum health of the turret.</summary>
        public float MaxHealth;

        /// <summary>The amount of health the turret gains per level.</summary>
        public float MaxHealthInc;

        /// <summary>The amount of health the turret regenerates per second.</summary>
        public float HealthRegen;

        /// <summary>The amount of health regen the turret gains per level.</summary>
        public float HealthRegenInc;

        /// <summary>The amount of armor the turret has.</summary>
        public float Armor;

        /// <summary>The movement speed of the turret.</summary>
        public float Movespeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TurretBodyStats"/> struct with the specified values.
        /// </summary>
        /// <param name="maxHealth">The maximum health of the turret. Default is 130.</param>
        /// <param name="maxHealthInc">The amount of health the turret gains per level. Default is 39.</param>
        /// <param name="healthRegen">The amount of health the turret regenerates per second. Default is 0.6.</param>
        /// <param name="healthRegenInc">The amount of health regen the turret gains per level. Default is 0.12.</param>
        /// <param name="armor">The amount of armor the turret has. Default is 0.</param>
        /// <param name="movespeed">The movement speed of the turret. Default is 7.</param>
        public TurretBodyStats(float maxHealth = 130f, float maxHealthInc = 39f, float healthRegen = 0.6f, float healthRegenInc = 0.12f, float armor = 0f, float movespeed = 7f)
        {
            this.MaxHealth = maxHealth;
            this.MaxHealthInc = maxHealthInc;
            this.HealthRegen = healthRegen;
            this.HealthRegenInc = healthRegenInc;
            this.Armor = armor;
            this.Movespeed = movespeed;
        }
    }
}
