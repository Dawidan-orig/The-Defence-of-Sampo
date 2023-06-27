using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLock : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = Vector3.forward * 10;

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}
