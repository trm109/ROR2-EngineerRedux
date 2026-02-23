
using System;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EngineerRedux.Utils
{
    public struct TurretBodyStats
    {
        public float maxHealth;
        public float maxHealthInc;
        public float healthRegen;
        public float healthRegenInc;
        public float armor;
        public float movespeed;

        public static readonly TurretBodyStats Default = new TurretBodyStats
        {
            maxHealth = 130f,
            maxHealthInc = 39f,
            healthRegen = 0.6f,
            healthRegenInc = 0.12f,
            armor = 0f,
            movespeed = 7f
        };
    }

    public class TurretBodySkillDef : SkillDef
    {
        public TurretBodyStats turretBodyStats;
    }
}
