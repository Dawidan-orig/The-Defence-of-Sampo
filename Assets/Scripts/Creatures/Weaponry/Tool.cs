using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Tool : MonoBehaviour
{
    public Transform host;
    public float additionalMeleeReach;
    public LayerMask alive;
    public LayerMask structures;

    public void SetHost(Transform newHost)
    {
        host = newHost;
    }

    public virtual float GetRange() { return additionalMeleeReach; }
}
