using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgentMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float jumpHeight = 2f;

    public delegate void OnJump();
    public OnJump onJumpDelegate;
    public delegate void OnLand();
    public OnLand onLandDelegate;
    public delegate void OnUnGround();
    public OnUnGround onUnGroundDelegate;

    private Rigidbody _rigidBody;
    private BoxCollider _boxCollider;

    private bool _isGrounded = true;

    private float _jumpCooldown = .3f;
    private float _jumpTimer = .3f;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    public void RotateAgent(float rotation)
    {
        Quaternion rotationVelocity = Quaternion.Euler(0, rotation * rotationSpeed * Time.deltaTime, 0);
        _rigidBody.MoveRotation(_rigidBody.rotation * rotationVelocity);
    }

    public void MoveAgent(float forwardMovement)
    {
        Vector3 forward = transform.forward * forwardMovement * moveSpeed * Time.deltaTime;
        _rigidBody.MovePosition(transform.position + forward);
    }

    public void Jump()
    {
        // Can't jump if mid air
        if (!IsGrounded())
        {
            return;
        }

        // Don't jump when jump is on cooldown
        if (_jumpTimer < _jumpCooldown)
        {
            return;
        }

        _jumpTimer = 0f;
        _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, 0f, _rigidBody.velocity.z);
        _rigidBody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);

        onJumpDelegate();
    }
    private void Update()
    {
        _jumpTimer += Time.deltaTime;

        // Agent was not grounded
        if (!_isGrounded)
        {
            if (IsGrounded())
            {
                _isGrounded = true;
                onLandDelegate();
            }
        }
        // Agent was grounded
        else
        {
            // Check if agent became ungrounded
            if (!IsGrounded())
            {
                _isGrounded = false;
                onUnGroundDelegate();
            }
        }
    }

    public bool IsGrounded()
    {
        int layermask = 1 << 3;

        Ray ray = new Ray(transform.position, Vector3.down);
        return Physics.Raycast(ray, _boxCollider.bounds.extents.y + .01f, layermask);
    }
}
