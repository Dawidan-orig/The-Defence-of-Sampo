using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositioner : MonoBehaviour
{
    public Transform player;
    public Vector2 sensitivity;

    private Vector2 _rotation;
    private Vector3 _offset;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity.x;
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity.y;

        _rotation.x -= mouseY;
        _rotation.y += mouseX;

        _rotation.x = Mathf.Clamp(_rotation.x, -30, 45);
        transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
        player.rotation = Quaternion.Euler(0, _rotation.y, 0);
    }
}
