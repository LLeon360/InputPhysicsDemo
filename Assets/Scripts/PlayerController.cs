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
    private SphereCollider sc;
    [SerializeField]
    private PlayerInput playerInput;

    // add variables for speed, jumpHeight, and respawnHeight
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float jumpHeight = 5f;
    [SerializeField]
    private float respawnHeight = -1;

    private Vector2 moveValue = Vector2.zero;

    // add variable to check if we're on the ground
    private bool isGrounded;
    private bool hasDoubleJump;

    // keep references to Actions, in order to remove them from the InputSystem when the PlayerController is destroyed
    private Action<InputAction.CallbackContext> moveCallback;
    private Action<InputAction.CallbackContext> jumpCallback;
    private Action<InputAction.CallbackContext> flattenCallback;
    private Action<InputAction.CallbackContext> unflattenCallback;


    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        sc = gameObject.GetComponent<SphereCollider>();

        moveCallback = ctx => OnMove(ctx.ReadValue<Vector2>());
        playerInput.actions.FindAction("Move").started += moveCallback;
        playerInput.actions.FindAction("Move").performed += moveCallback;
        playerInput.actions.FindAction("Move").canceled += moveCallback;

        jumpCallback = ctx => OnJump();
        playerInput.actions.FindAction("Jump").performed += jumpCallback;

        flattenCallback = ctx => OnFlatten();
        playerInput.actions.FindAction("Flatten").performed += flattenCallback;

        unflattenCallback = ctx => OnUnflatten();
        playerInput.actions.FindAction("Flatten").canceled += unflattenCallback;
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

    public void OnFlatten() {
        // flatten the player
        Flatten();
    }

    void Flatten() {
        // flatten the player

        //adjust position to prevent the ball from suddenly floating
        if(transform.localScale.y == 1)
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        //scale the player and collider
        transform.localScale = new Vector3(1, 0.5f, 1);
        //(this does make the horizontal bounding box inaccurate since the collider is a sphere and not an ellipsoid)
        sc.radius = 0.25f;
    }

    public void OnUnflatten() {
        // unflatten the player
        Unflatten();
    }

    void Unflatten() {
        // unflatten the player

        //prevent clipping through the ground
        if(transform.localScale.y == 0.5f)
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z);
        //rescale the player and collider
        transform.localScale = new Vector3(1, 1, 1);
        sc.radius = 0.5f;
    }
    
    public void OnJump()
    {
        // check if player is on the ground, and call Jump()
        if(isGrounded || hasDoubleJump) {
            hasDoubleJump = isGrounded;
            Jump();
        }
    }

    private void Jump()
    {
        // Set the y velocity to some positive value while keeping the x and z whatever they were originally
        rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
    }

    void OnMove(Vector2 moveVal)
    {
        //store input as a 2D vector and call Move()
        // moveValue = moveVal.Get<Vector2>();
        moveValue = moveVal;
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
        // Check if we are in contact with the ground. If we are, note that we are grounded
        if(collision.gameObject.tag == "Ground")        
        {
            isGrounded = true;
            hasDoubleJump = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // When we leave the ground, we are no longer grounded
        if(collision.gameObject.tag == "Ground")
        {
            isGrounded = false;
        }
    }

    private void Respawn()
    {
        // reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        // remove callbacks from InputSystem
        playerInput.actions.FindAction("Move").started -= moveCallback;
        playerInput.actions.FindAction("Move").performed -= moveCallback;
        playerInput.actions.FindAction("Move").canceled -= moveCallback;
        playerInput.actions.FindAction("Jump").performed -= jumpCallback;
        playerInput.actions.FindAction("Flatten").performed -= flattenCallback;
        playerInput.actions.FindAction("Flatten").canceled -= unflattenCallback;
    }
}
