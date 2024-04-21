using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Tool : MonoBehaviour
{
    [SerializeField]
    protected Transform _host;
    public float additionalMeleeReach;
    public LayerMask alive;
    public LayerMask structures;

    public Transform Host
    {
        get => _host;
        set 
        {
            _host = value;
            if (_host == null)
            {
                GetComponent<Faction>().ChangeFactionCompletely(Faction.FType.aggressive);
            }
            else
            {
                GetComponent<Faction>().ChangeFactionCompletely(_host.GetComponent<Faction>().FactionType);
                Physics.IgnoreCollision(GetComponent<Collider>(), _host.GetComponent<IDamagable>().Vital);
            }
        } 
    }

    public virtual float GetRange() { return additionalMeleeReach; }
}
