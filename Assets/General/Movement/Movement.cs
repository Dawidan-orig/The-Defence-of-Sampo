using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("constraints")]
    [Header("On ground")]
    public float walkSpeed = 5;
    public float runSpeed = 10;
    public float sprintSpeed = 20;
    public float distToGround = 0.3f;
    public float dragOnGround = 1;
    public float toGroundForce = 5;
    [Header("Air")]
    public float dragOffGround = 0.2f;
    public float airMultiplier = 0.5f;
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
    protected float _moveSpeed;
    [SerializeField]
    protected bool _isGrounded = true;
    [SerializeField]
    bool _jumpReady = true;
    [SerializeField]
    protected Vector2 _inputMovement;
    protected Rigidbody _rb;

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

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }


    protected virtual void Update()
    {
        if (_inputMovement == Vector2.zero)
            _rb.drag = _isGrounded ? dragOnGround : dragOffGround;
        else
            _rb.drag = 0;

        _rb.useGravity = !OnSlope();        
    }

    protected virtual void FixedUpdate()
    {
        _isGrounded = Physics.BoxCast(vital.bounds.center, new Vector3(vital.bounds.size.x / 2, 0.1f, vital.bounds.size.z / 2),
            transform.up * -1, out _, transform.rotation, vital.bounds.size.y / 2 + distToGround, ground);

        if (_inJump && _isGrounded && _rb.velocity.y < 0)
            _inJump = false;

        ApplyMovement();

        if(IsGrounded)
            FixMovement();        
    }

    public void PassInput(Vector2 inputMovement, SpeedType type, bool jump)
    {
        _inputMovement = inputMovement;

        if (type == SpeedType.sprint)
            _moveSpeed = sprintSpeed;
        else if (type == SpeedType.run)
            _moveSpeed = runSpeed;
        else
            _moveSpeed = walkSpeed;

        if (jump)
            Jump();
    }

    protected void ApplyMovement()
    {
        _inputMovement.x = Mathf.Clamp(_inputMovement.x, -1, 1);
        _inputMovement.y = Mathf.Clamp(_inputMovement.y, -1, 1);

        _movement = transform.forward * _inputMovement.x + transform.right * _inputMovement.y;

        if (OnSlope())
        {
            _rb.AddForce(SlopeMoveDir() * _moveSpeed, ForceMode.Acceleration);

            if (_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * toGroundForce, ForceMode.Acceleration);

            return;
        }

        if (_isGrounded)
            _rb.AddForce(_movement.normalized * _moveSpeed, ForceMode.Acceleration);
        else
            _rb.AddForce(_movement.normalized * _moveSpeed * airMultiplier, ForceMode.Acceleration);        
    }

    protected void FixMovement()
    {
        if (OnSlope() && _rb.velocity.magnitude > _moveSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * _moveSpeed;
            return;
        }

        Vector3 flatVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

        if (flatVelocity.magnitude > _moveSpeed)
        {
            _rb.velocity = flatVelocity.normalized * _moveSpeed + _rb.velocity.y * Vector3.up;
        }
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
}
