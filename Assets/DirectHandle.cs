using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectHandle : MonoBehaviour
    // Контроль за Rigidbody, чтобы континуально перемещать объекты.
{
    public Transform bladeHandle;
    public Rigidbody bladeTarget;

    private Vector3 desirePoint;
    private Vector3 desireRotation;

    private void FixedUpdate()
    {
        desirePoint = transform.position;
        desireRotation = bladeTarget.transform.rotation.eulerAngles;

        bladeTarget.velocity = (desirePoint - bladeHandle.position) / Time.fixedDeltaTime;

        bladeTarget.maxAngularVelocity = 20;
        Quaternion detailRot = transform.rotation * Quaternion.Inverse(bladeTarget.transform.rotation);
        Vector3 euler = new(Mathf.DeltaAngle(0, detailRot.eulerAngles.x),
            Mathf.DeltaAngle(0, detailRot.eulerAngles.y),
            Mathf.DeltaAngle(0, detailRot.eulerAngles.z));
        euler *= 0.95f;
        euler *= Mathf.Deg2Rad;
        bladeTarget.angularVelocity = euler / Time.fixedDeltaTime;
    }
}
