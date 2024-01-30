using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// import UnityEngine.InputSystem and UnityEngine.SceneManagement
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // add component references
    private Rigidbody rb;
    [SerializeField]
    private PlayerInput playerInput;

    // add variables for speed, jumpHeight, and respawnHeight
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float jumpHeight = 5f;
    [SerializeField]
    private float respawnHeight = -1;
    [SerializeField]
    private bool canDoubleJump = false;

    private Vector2 moveValue = Vector2.zero;

    // add variable to check if we're on the ground
    private bool isGrounded;
    private bool hasDoubleJump;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // check if player is under respawnHeight and call Respawn()
        if(transform.position.y < respawnHeight)
        {
            Respawn();
        }
        Move(moveValue.x, moveValue.y);
    }

    public void OnFlatten(InputAction.CallbackContext ctx) {
        if(ctx.phase == InputActionPhase.Started) {
            Flatten();
        } else if(ctx.phase == InputActionPhase.Canceled) {
            Unflatten();
        }
    }

    void Flatten() {
        // flatten the player

        //adjust position to prevent the ball from suddenly floating
        if(transform.localScale.y == 1)
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z);
        //scale the player and collider
        transform.localScale = new Vector3(2, 0.5f, 2);
    }

    void Unflatten() {
        // unflatten the player

        //prevent clipping through the ground
        if(transform.localScale.y == 0.5f)
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z);
        //rescale the player and collider
        transform.localScale = new Vector3(1, 1, 1);
    }
    
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if(ctx.phase == InputActionPhase.Started) {
            // check if player is on the ground, and call Jump()
            if(isGrounded || hasDoubleJump) {
                hasDoubleJump = isGrounded && canDoubleJump;
                Jump();
            }
        }
        
    }

    private void Jump()
    {
        // Set the y velocity to some positive value while keeping the x and z whatever they were originally
        rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        //store input as a 2D vector and call Move()
        // moveValue = moveVal.Get<Vector2>();
        moveValue = ctx.ReadValue<Vector2>();
    }

    private void Move(float x, float z)
    {
        // Set the x & z velocity of the Rigidbody to correspond with our inputs while keeping the y velocity what it originally is.
        rb.velocity = new Vector3(x * speed, rb.velocity.y, z * speed);
    }

    void OnCollisionEnter(Collision collision)
    {
        // This function is commonly useful, but for our current implementation we don't need it

    }

    void OnCollisionStay(Collision collision)
    {
        Vector3 norm = collision.GetContact(0).normal;
        
        if(Vector3.Angle(norm, Vector3.up) < 45) {
            isGrounded = true;
            hasDoubleJump = canDoubleJump;
        }

        
        // Check if we are in contact with the ground. If we are, note that we are grounded
        // if(collision.gameObject.CompareTag("Ground"))
        // {
        //     isGrounded = true;
        //     hasDoubleJump = canDoubleJump;
        // }
    }

    void OnCollisionExit(Collision collision)
    {
        // When we leave the ground, we are no longer grounded
        // if(collision.gameObject.CompareTag("Ground"))
        // {
        //     isGrounded = false;
        // }

        isGrounded = false;
    }

    private void Respawn()
    {
        // reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
