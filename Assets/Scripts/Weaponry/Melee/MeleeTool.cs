using Sampo.Melee;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeTool : Tool
{
    public float cooldownBetweenAttacks;
    public float damageMultiplier = 1;

    public Transform rightHandHandle;

    public override float GetRange()
    {
        return base.GetRange() + host.GetComponent<MeleeFighter>().baseReachDistance;
    }
}
