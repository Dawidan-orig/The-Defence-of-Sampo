using UnityEngine;

public class CameraPositioner : MonoBehaviour
{
    public Transform player;
    public Vector2 sensitivity;
    public Vector2 xAngleLimit = new Vector2(-75, 75);

    private Vector2 _rotation;
    private Vector3 _offset;
    private Transform _lockTarget;
    private Vector3 _lockPosition;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _offset = transform.position - player.position;
    }

    private void LateUpdate()
    {
        transform.position = player.position + player.rotation * _offset;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 300))
            {
                if (hit.transform == player)
                    Physics.Raycast(hit.point, transform.forward, out hit, 300);

                if(player.TryGetComponent<SwordControl>(out var c))
                    if(c.blade.transform == hit.transform)
                        Physics.Raycast(hit.point, transform.forward, out hit, 300);

                _lockPosition = hit.point; // Просто большое число вдали
            }
            else
                _lockPosition = transform.position + transform.forward * 300; // Просто большое число

            if (hit.rigidbody && hit.transform != player)
                _lockTarget = hit.transform;
            else
                _lockTarget = null;
        }
        else if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            if (_lockTarget)
                _lockPosition = _lockTarget.position;

            Debug.DrawLine(player.position, _lockPosition);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            GameObject go = new GameObject();
            Transform probe = go.transform;
            probe.position = transform.position;
            probe.rotation = transform.rotation;

            probe.LookAt(_lockPosition, Vector3.up);

            _rotation = probe.localEulerAngles;

            if (_rotation.x > 180)
                _rotation.x -= 360;

            Destroy(go);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity.x;
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity.y;

            _rotation.x -= mouseY;
            _rotation.y += mouseX;
            
            _rotation.x = Mathf.Clamp(_rotation.x, xAngleLimit.x, xAngleLimit.y);
        }
        
        transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
        player.rotation = Quaternion.Euler(0, _rotation.y, 0);

        Vector3 eulers = transform.rotation.eulerAngles;

        float passingX = eulers.x;

        if (passingX > 180)
            passingX -= 360;

        transform.rotation = Quaternion.Euler(Mathf.Clamp(passingX, xAngleLimit.x, xAngleLimit.y), eulers.y, eulers.z);

        transform.position += Vector3.up * (transform.rotation.eulerAngles.x > 180 ? transform.rotation.eulerAngles.x - 360 : transform.rotation.eulerAngles.x) / 20;
    }
}
