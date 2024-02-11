using Sampo.AI;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    //TODO OPTIMIZATION : ѕолностью перейти на PhysicalNMAgent, этот - дорогой, дл€ частых юнитов не подходит из-за большого количества Ray- и boxcast'ов
    //TODO DESIGN : ѕридумать разные типы движени€ на основе одного базого. Ћетающие, наследующие от наземных, тер€€ огромный пласт функционала - это мусор.
    #region variables
    [Header("constraints")]
    [Header("On ground")]
    public float walkSpeed = 5;
    public float runSpeed = 10;
    public float sprintSpeed = 20;
    [Tooltip("ћаксимальна€ возможна€ скорость, которую может развить этот Rigidbody, и которую будет уменьшать до спринта")]
    public float maximumReachableSpeed = 25;
    [Tooltip("—корость, после которой персонаж спотыкаетс€ и падает в Ragdoll")]
    public float ragdollSpeed = 35;
    public float velociyAngleChangeSpeedEuler = 90;
    public float distToGround = 0.3f;
    public float activeDrag = 20;
    public float passiveDragOnGround = 1;
    public float snapHeight = 3;
    public float snapSpeed = 21;
    public float toGroundForce = 5;
    [Header("Air")]
    public float dragOffGround = 0.2f;
    public float airMultiplier = 0.5f;
    public bool externalGravityControl = false;
    [Header("Jump")]
    public float jumpForce = 10;
    public float jumpCooldown = 2;
    [SerializeField]
    protected bool _inJump = false;
    [Header("Slope")]
    public float slopeAngle = 65;
    [SerializeField]
    private RaycastHit _slopeHit;
    [Header("setup")]
    public LayerMask ground;
    public Collider vital;

    [Header("lookonly")]
    [SerializeField]
    protected Vector3 _movement;
    [SerializeField]
    protected float _currentMoveSpeed;
    [SerializeField]
    protected bool _isGrounded = true;
    [SerializeField]
    private bool _jumpReady = true;
    [SerializeField]
    protected Vector2 _inputMovement;
    [SerializeField]
    protected int _framesSinceLastGrounded = 0;
    [SerializeField]
    protected Vector3 _contactNormal= Vector3.zero;
    protected Rigidbody _rb;

    private const float NO_INPUT_TIME = 0.3f;
    private float currentNoInputTime = 0;
    #endregion

    public enum SpeedType
    {
        walk,
        run,
        sprint
    }

    public Vector2 InputMovement { get => _inputMovement; set => _inputMovement = value; }
    public bool JumpReady { get => _jumpReady; }
    public bool IsGrounded { get => _isGrounded; }
    public bool InJump { get => _inJump; set => _inJump = value; }

    #region unity
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if(TryGetComponent(out AliveBeing being)) 
        {
            vital = being.vital;
        }
    }

    protected virtual void Update()
    {
        if (_inputMovement == Vector2.zero)
            _rb.drag = _isGrounded ? passiveDragOnGround : dragOffGround;
        else
            _rb.drag = 0;

        if(!externalGravityControl)
            _rb.useGravity = !OnSlope();

        if (currentNoInputTime < NO_INPUT_TIME)
            currentNoInputTime += Time.deltaTime;
        else
            _inputMovement = Vector2.zero;
    }

    protected virtual void FixedUpdate()
    {
        _isGrounded = Physics.BoxCast(vital.bounds.center, new Vector3(vital.bounds.size.x / 2, 0.1f, vital.bounds.size.z / 2),
            transform.up * -1, out RaycastHit hit, transform.rotation, vital.bounds.size.y / 2 + distToGround, ground);
        _contactNormal = hit.normal;

        if (_inJump && _isGrounded && _rb.velocity.y < 0)
            _inJump = false;

        ApplyMovement();

        if (IsGrounded)
            FixMovement();

        _framesSinceLastGrounded++;
        if (_isGrounded || SnapToGround())
        {
            _framesSinceLastGrounded = 0;
            _contactNormal.Normalize();
        }
        else
            _contactNormal = Vector3.up;

    }
    #endregion

    public virtual void PassInputDirect(Vector2 inputMovement, SpeedType type, bool jump)
    {
        currentNoInputTime = 0;
        _inputMovement = inputMovement;

        if (type == SpeedType.sprint)
            _currentMoveSpeed = sprintSpeed;
        else if (type == SpeedType.run)
            _currentMoveSpeed = runSpeed;
        else
            _currentMoveSpeed = walkSpeed;

        if (jump)
            Jump();
    }

    #region movement control

    protected virtual void ApplyMovement()
    {
        _inputMovement.x = Mathf.Clamp(_inputMovement.x, -1, 1);
        _inputMovement.y = Mathf.Clamp(_inputMovement.y, -1, 1);

        _movement = transform.forward * _inputMovement.x + transform.right * _inputMovement.y;

        if (OnSlope())
        {
            _rb.AddForce(SlopeMoveDir() * _currentMoveSpeed, ForceMode.Acceleration);

            //if (_rb.velocity.y > 0)
            //    _rb.AddForce(Vector3.down * toGroundForce, ForceMode.Acceleration);

            ControlVelocityAngle(SlopeMoveDir());

            return;
        }

        if (_isGrounded)
        {
            _rb.AddForce(_movement.normalized * _currentMoveSpeed, ForceMode.Acceleration);

            ControlVelocityAngle(_movement.normalized);
        }
        else
            _rb.AddForce(_movement.normalized * _currentMoveSpeed * airMultiplier, ForceMode.Acceleration);        

    }

    protected void ControlVelocityAngle(Vector3 desireDir) 
    {
        if (_inputMovement != Vector2.zero)
        {
            Vector3 rotation = Vector3.RotateTowards(_rb.velocity.normalized, desireDir,
                velociyAngleChangeSpeedEuler * Mathf.Deg2Rad * Time.deltaTime, _currentMoveSpeed);
            _rb.velocity = rotation.normalized * _rb.velocity.magnitude;
        }
    }

    protected virtual void FixMovement()
    {
        if (OnSlope() && _rb.velocity.magnitude > _currentMoveSpeed || _inputMovement == Vector2.zero)
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

    #region ground-specific
    protected virtual bool SnapToGround()
    {
        if (_inJump)
            return false;
        if (_rb.velocity.magnitude > snapSpeed)
            return false;
        if (_framesSinceLastGrounded > 1)
            return false;
        if (!Physics.Raycast(vital.bounds.center, Vector3.down, out RaycastHit hit, snapHeight, ground))
            return false;
        //if(hit.normal.y < minGroundDotProduct)
        //    return false;

        _contactNormal = hit.normal;
        float dot = Vector3.Dot(_rb.velocity, hit.normal);
        if(dot > 0)
            _rb.velocity = (_rb.velocity - hit.normal * dot).normalized * _rb.velocity.magnitude;
        return true;
    }

    protected virtual void Jump()
    {
        _jumpReady = false;
        _inJump = true;

        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

        Invoke(nameof(ResetJump), jumpCooldown);
    }

    protected virtual bool OnSlope()
    {
        if (Physics.Raycast(vital.bounds.center, transform.up * -1, out var hit, vital.bounds.size.y / 2 + distToGround, ground))
        {
            _contactNormal = hit.normal;
            _slopeHit = hit;
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            return angle < slopeAngle && angle != 0;
        }

        return false;
    }

    protected void ResetJump()
    {
        _jumpReady = true;
    }

    protected Vector3 SlopeMoveDir()
    {
        return Vector3.ProjectOnPlane(_movement, _slopeHit.normal).normalized;
    }
    #endregion
    #endregion
}
