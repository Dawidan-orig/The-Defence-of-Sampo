using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositioner : MonoBehaviour
{
    public Transform player;
    public Vector2 sensitivity;
    public Vector2 inAttackSensitivity;

    private Vector2 _rotation;
    private Vector3 _offset;
    private Vector3 _lockPoint;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _offset = transform.position - player.position;
    }

    private void Update()
    {
        transform.position = player.position + player.rotation * _offset;

        Vector2 actualInAttackSensitivity = Vector2.one;
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            transform.LookAt(_lockPoint, Vector3.up);
        }
        else
        { 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity.x * actualInAttackSensitivity.x;
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity.y * actualInAttackSensitivity.y;

        _rotation.x -= mouseY;
        _rotation.y += mouseX;

        _rotation.x = Mathf.Clamp(_rotation.x, -30, 45);
        transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
        player.rotation = Quaternion.Euler(0, _rotation.y, 0);
    }
}
