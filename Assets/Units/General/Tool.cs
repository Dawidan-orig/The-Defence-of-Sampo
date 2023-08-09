using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
    public Transform host;
    public float additionalMeleeReach;

    public void SetHost(Transform newHost)
    {
        host = newHost;
    }
}
