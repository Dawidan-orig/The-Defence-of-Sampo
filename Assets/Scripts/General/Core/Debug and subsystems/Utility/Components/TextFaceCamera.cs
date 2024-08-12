using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextFaceCamera : MonoBehaviour
{
    private void Start()
    {
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(Vector3.up, 180);
    }

    void Update()
    {
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(Vector3.up, 180);
    }
}
