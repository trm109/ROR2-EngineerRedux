using RoR2;
using EntityStates.EngiTurret.EngiTurretWeapon;

namespace EngineerRedux.EntityStates.Turret
{
	public class BeamPrimaryState : FireBeam
	{
		public float procCoefficient = 1f; // Default is 0.6f, this is a bit unfair for things like ATG.
		public float maxDistance = 300f; // Default is 25, this sucks beans.
	}
}
