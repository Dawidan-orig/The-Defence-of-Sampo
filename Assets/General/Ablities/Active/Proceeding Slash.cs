using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceedingSlash : Ability
{
    public GameObject slashPrefab;
    public const float SPEED = 25;
    public const float LIFETIME = 10;
    SwordControl userActions;

    public ProceedingSlash(Transform user, SwordControl controlSystem, GameObject slashPrefab) : base(user)
    {
        userActions = controlSystem;
        userActions.OnSlashEnd += PerformAbility;
        this.slashPrefab = slashPrefab;
    }

    public void PerformAbility(object sender, SwordControl.ActionData e)
    {
        if (!Activated)
            return;

        GameObject slash = GameObject.Instantiate(slashPrefab, Vector3.Lerp(e.moveStart.position,e.desire.position,0.5f),
            Quaternion.FromToRotation(Vector3.right,
            Vector3.ProjectOnPlane((e.desire.position-e.moveStart.position).normalized,user.forward)));

        slash.GetComponent<Faction>().type = user.GetComponent<Faction>().type;

        Rigidbody body = slash.GetComponent<Rigidbody>();
        body.AddForce(user.forward * 10, ForceMode.VelocityChange);
        body.drag = 0;

        Physics.IgnoreCollision(slash.GetComponent<Collider>(), e.blade.GetComponent<Collider>());

        Object.Destroy(slash, LIFETIME);
    }
}
