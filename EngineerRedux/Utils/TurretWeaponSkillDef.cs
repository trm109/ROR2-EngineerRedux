using System;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EngineerRedux.Utils
{
	// These skills are selected in the Loadout menu. Not actual skills, more like passives.
	public class TurretWeaponSkillDef : SkillDef
	{
		// Holds a reference to the skillDef that will replace the turrets' primary skillDef.
		public SkillDef selectedPrimarySkillDef;
	}
}
