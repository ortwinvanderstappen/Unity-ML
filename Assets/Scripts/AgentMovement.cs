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

    private Rigidbody _rigidBody;
    private BoxCollider _boxCollider;
    private GoalSeeker _goalSeeker;

    private float jumpCooldown = .3f;
    private float jumpTimer = .3f;

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
        if (jumpTimer < jumpCooldown)
        {
            return;
        }

        jumpTimer = 0f;
        _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, 0f, _rigidBody.velocity.z);
        _rigidBody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        onJumpDelegate();
    }
    private void Update()
    {
        jumpTimer += Time.deltaTime;
    }

    public bool IsGrounded()
    {
        int layermask = 1 << 3;

        //Vector3 start1 = new Vector3(transform.localPosition.x + _boxCollider.size.x * 0.5f, transform.localPosition.y, transform.localPosition.z);
        //Vector3 start2 = new Vector3(transform.localPosition.x - _boxCollider.size.x * 0.5f, transform.localPosition.y, transform.localPosition.z);
        //Vector3 start3 = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + _boxCollider.size.z * 0.5f);
        //Vector3 start4 = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - _boxCollider.size.z * 0.5f);

        //Ray ray1 = new Ray(start1, Vector3.down);
        //Ray ray2 = new Ray(start2, Vector3.down);
        //Ray ray3 = new Ray(start3, Vector3.down);
        //Ray ray4 = new Ray(start4, Vector3.down);

        //bool check1 = Physics.Raycast(ray1, _boxCollider.bounds.extents.y + .01f, layermask);
        //bool check2 = Physics.Raycast(ray2, _boxCollider.bounds.extents.y + .01f, layermask);
        //bool check3 = Physics.Raycast(ray3, _boxCollider.bounds.extents.y + .01f, layermask);
        //bool check4 = Physics.Raycast(ray4, _boxCollider.bounds.extents.y + .01f, layermask);

        //return check1 || check2 || check3 || check4;

        Ray ray = new Ray(transform.position, Vector3.down);
        return Physics.Raycast(ray, _boxCollider.bounds.extents.y + .01f, layermask);
    }
}
