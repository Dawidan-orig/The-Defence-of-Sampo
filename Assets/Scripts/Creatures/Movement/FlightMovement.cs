using UnityEngine;
using UnityEngine.AI;

public class FlightMovement : Movement
{
    //TODO DESIGN: превратить это в модификацию основного Movement, то-есть чтобы летающий и просто ходить мог.

    [Header("===Flying===")]
    [Tooltip("Высота, которой придерживается юнит относительно текущей точки")]
    public float flightHeight = 10;
    public float flightLift = 5;

    private Vector3 lastGroundPos = Vector3.zero;

    protected override void Update()
    {
        if (flightHeight == 0) // Нахождение на земле
        {
            base.ApplyMovement();
        }

        base.Update();
    }
    protected override void FixedUpdate()
    {
        if (flightHeight == 0) //Нахождение на земле
        {
            base.ApplyMovement();
        }

        ApplyMovement();

        FixMovement();
    }

    protected override void ApplyMovement() //Нахождение на земле
    {
        if (flightHeight == 0)
        {
            base.ApplyMovement();
        }

        _inputMovement.x = Mathf.Clamp(_inputMovement.x, -1, 1);
        _inputMovement.y = Mathf.Clamp(_inputMovement.y, -1, 1);

        _movement = transform.forward * _inputMovement.x + transform.right * _inputMovement.y;

        ControlVelocityAngle(_movement.normalized);

        _rb.AddForce(_movement.normalized * _currentMoveSpeed, ForceMode.Acceleration);
    }

    protected override void FixMovement()
    {
        #region flight height
        float yMovement = _rb.velocity.y;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, ground))
        {
            lastGroundPos = hit.point;
        }

        if (transform.position.y - lastGroundPos.y > flightHeight)
        {
            float modifier = 1 - Mathf.InverseLerp(lastGroundPos.y, lastGroundPos.y + transform.position.y, lastGroundPos.y + flightHeight);
            yMovement = -(flightHeight * modifier);
        }
        else if (transform.position.y - lastGroundPos.y < flightHeight)
        {
            float modifier = 1 - Mathf.InverseLerp(lastGroundPos.y, lastGroundPos.y + flightHeight, lastGroundPos.y + transform.position.y);
            yMovement = (flightHeight * modifier);
        }
        _rb.velocity = new Vector3(_rb.velocity.x, yMovement * flightLift, _rb.velocity.z);
        #endregion

        if (_rb.velocity.magnitude > _currentMoveSpeed || _inputMovement == Vector2.zero)
        {
            _rb.velocity = _rb.velocity.normalized * (_rb.velocity.magnitude - activeDrag * Time.fixedDeltaTime);

            if (_rb.velocity.magnitude > maximumReachableSpeed)
                _rb.velocity = _rb.velocity.normalized * maximumReachableSpeed;
            return;
        }

        Vector3 flatVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        if (flatVelocity.magnitude > _currentMoveSpeed || _inputMovement == Vector2.zero)
        {
            _rb.velocity = flatVelocity.normalized * (flatVelocity.magnitude - activeDrag * Time.fixedDeltaTime) + _rb.velocity.y * Vector3.up;
            if (_rb.velocity.magnitude > maximumReachableSpeed)
                _rb.velocity = flatVelocity.normalized * maximumReachableSpeed + _rb.velocity.y * Vector3.up;
        }
    }

    #region nullification overrides
    protected override void Jump()
    {

    }

    protected override bool OnSlope()
    {
        return false;
    }

    protected override bool SnapToGround()
    {
        return false;
    }
    #endregion
}
