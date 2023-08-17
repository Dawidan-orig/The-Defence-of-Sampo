using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextFaceCamera : MonoBehaviour
{
    private void Start()
    {
        transform.LookAt(SceneView.lastActiveSceneView.camera.transform.position);
        transform.Rotate(Vector3.up, 180);
    }

    void Update()
    {
        transform.LookAt(SceneView.lastActiveSceneView.camera.transform.position);
        transform.Rotate(Vector3.up, 180);
    }
}
