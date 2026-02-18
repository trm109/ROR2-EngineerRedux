using System;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EngineerRedux.Skills
{
	// These skills are selected in the Loadout menu. Not actual skills, more like passives.
	public class TurretWeaponSelectorSkillDef : SkillDef
	{
		// Holds a reference to the skillDef that will replace the turrets' primary skillDef.
		public SkillDef selectedPrimarySkillDef;
	}
}
