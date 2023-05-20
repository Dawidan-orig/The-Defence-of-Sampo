using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wibblyHandle : MonoBehaviour
{
    public Rigidbody target;

    private Vector3 resultPoint;

    private void FixedUpdate()
    {
        if (target == null)
            return;

        resultPoint = transform.position;

        target.velocity = (resultPoint - target.transform.position) / Time.fixedDeltaTime;
    
    }
}
