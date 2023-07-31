using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("constraints")]
    public float moveSpeed;
    public float distToGround;
    public float dragOnGround;
    public float jumpForce;
    public float jumpCooldown;
    [Header("setup")]
    public LayerMask ground;
    public Collider vital;

    [Header("lookonly")]
    [SerializeField]
    bool _isGrounded = true;
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
        _isGrounded = Physics.Raycast(vital.bounds.center, transform.up*-1, vital.bounds.size.y/2 + distToGround);

        ApplyMovement();

        _rb.drag = _isGrounded ? dragOnGround : 0;

    }

    private void UpdateInput() 
    {
        _inputMovement.x = Input.GetAxisRaw("Vertical");
        _inputMovement.y = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded && _jumpReady)
        {
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ApplyMovement() 
    {
        Vector3 movementDirecion = transform.forward * _inputMovement.x + transform.right * _inputMovement.y;
        _rb.AddForce(movementDirecion.normalized * moveSpeed, ForceMode.Acceleration);
    }

    private void FixMovement() 
    {
        Vector3 flatVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

        if(flatVelocity.magnitude > moveSpeed) 
        {
            _rb.velocity = flatVelocity.normalized * moveSpeed + _rb.velocity.y * Vector3.up;
        }
    }

    private void Jump() 
    {
        _jumpReady = false;

        _rb.velocity = new Vector3(_rb.velocity.x,0,_rb.velocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private void ResetJump() 
    {
        _jumpReady = true;
    }
}
