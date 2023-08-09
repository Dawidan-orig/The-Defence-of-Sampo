using UnityEngine;

public class PlayerController : MonoBehaviour, IAnimationProvider
{
    //TODO : Перенести в State Machine для более удобного контроля
    [Header("constraints")]
    [Header("On ground")]
    public float walkSpeed = 1;
    public float runSpeed = 3;
    public float sprintSpeed = 5;
    public float distToGround = 0.3f;
    public float dragOnGround = 1;
    public float toGroundForce = 5;
    [Header("Air")]
    public float dragOffGround = 0.2f;
    public float airMultiplier = 0.5f;
    [Header("Jump")]
    public float jumpForce = 10;
    public float jumpCooldown = 2;
    public bool inJump = false;
    [Header("Slope")]
    public float slopeAngle = 65;
    [SerializeField]
    private RaycastHit _slopeHit;
    [Header("setup")]
    public LayerMask ground;
    public Collider vital;
    public Transform handTarget;

    [Header("lookonly")]
    [SerializeField]
    Vector3 _movement;
    [SerializeField]
    float _moveSpeed;
    [SerializeField]
    bool _isGrounded = true;
    [SerializeField]
    bool _jumpReady = true;

    private Vector2 _inputMovement;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        UpdateInput();

        FixMovement();
    }

    private void FixedUpdate()
    {    
        _isGrounded = Physics.BoxCast(vital.bounds.center, new Vector3 (vital.bounds.size.x/2, 0.1f, vital.bounds.size.z/2),
            transform.up * -1, out _,transform.rotation, vital.bounds.size.y / 2 + distToGround, ground);

        if (inJump && _isGrounded && _rb.velocity.y < 0)
            inJump = false;

        ApplyMovement();

        _rb.drag = _isGrounded ? dragOnGround : dragOffGround;
        _rb.useGravity = !OnSlope();
    }

    private void UpdateInput()
    {
        _inputMovement.x = Input.GetAxisRaw("Vertical");
        _inputMovement.y = Input.GetAxisRaw("Horizontal");        

        if (_isGrounded && Input.GetKey(KeyCode.LeftShift))
            _moveSpeed = sprintSpeed;
        else if (_isGrounded)
            _moveSpeed = runSpeed;

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded && _jumpReady)
        {
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ApplyMovement()
    {
        _movement = transform.forward * _inputMovement.x + transform.right * _inputMovement.y;

        if(OnSlope()) 
        {
            _rb.AddForce(SlopeMoveDir() * _moveSpeed, ForceMode.Acceleration);

            if(_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * toGroundForce, ForceMode.Acceleration);

            return;
        }

        if(_isGrounded)
            _rb.AddForce(_movement.normalized * _moveSpeed, ForceMode.Acceleration);
        else
            _rb.AddForce(_movement.normalized * _moveSpeed * airMultiplier, ForceMode.Acceleration);
    }

    private void FixMovement()
    {
        if(OnSlope() && _rb.velocity.magnitude > _moveSpeed) 
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

    private void Jump()
    {
        _jumpReady = false;

        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

        inJump = true;
    }

    private void ResetJump()
    {
        _jumpReady = true;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(vital.bounds.center, transform.up * -1, out var hit, vital.bounds.size.y / 2 + distToGround, ground))
        {
            _slopeHit = hit;
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            return angle < slopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 SlopeMoveDir() 
    {
        return Vector3.ProjectOnPlane(_movement, _slopeHit.normal).normalized;
    }

    public Vector3 GetLookTarget()
    {
        Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, out var res, 100, ground);
        return res.point;
    }

    public bool IsGrounded()
    {
        return _isGrounded;
    }

    public bool IsInJump()
    { 
        return inJump;
    }

    public Vector3 GetRightHandTarget()
    {
        return handTarget.position;
    }
}
