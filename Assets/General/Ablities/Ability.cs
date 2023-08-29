using System;
using UnityEngine;

[Serializable]
public class Ability
{
    public Transform user;
    [SerializeField]
    protected bool _enabled;
    [SerializeField]
    protected bool _activated;

    public Ability(Transform user)
    {
        this.user = user;
    }

    public virtual void Enable() { _enabled = true; }
    public virtual void Disable() {  _enabled = false; }

    public virtual void Activate() {_activated = true; }

    public virtual void Deactivate() {  _activated = false; }

    public virtual bool AbleToUse() 
    {
        return _activated && _enabled;
    }

    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
