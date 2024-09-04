using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WingedCore.Weaponry.Ranged;

namespace Sampo.SpecificWeaponry
{
    public class LightningGun : BaseShooting
    {
        public override Vector3 PredictMovement(Rigidbody target)
        {
            return target.position;
        }
    }
}