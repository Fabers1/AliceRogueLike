using System;
using System.Collections;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset inputActionAsset;

    InputAction moveAction;
    InputAction jumpAction;

    Vector2 moveInput;

    Rigidbody2D rb;

    [SerializeField] Transform bottomPos;
    [SerializeField] LayerMask floorLayer;
    [SerializeField] float bottomSize = 1.5f;
    public GameObject characterSprite;

    public float originalSpeed;
    public float jumpForce;
    public float dropDuration = 0.3f;

    [HideInInspector]
    public float modifiedSpeed;
    GameObject currentPlatform;
    bool facingRight = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        jumpAction = InputSystem.actions.FindAction("Jump");

        modifiedSpeed = originalSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void MoveLeft(InputAction.CallbackContext context)
    {
        if(context.started)
            moveInput.x = -1;
        else if(context.canceled)
            moveInput.x = 0;
    }

    public void MoveRight(InputAction.CallbackContext context)
    {
        if (context.started)
            moveInput.x = 1;
        else if (context.canceled)
            moveInput.x = 0;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started && OnGround())
        {
            Jump();
        }
    }

    public void Dropdown(InputAction.CallbackContext context)
    {
        if (context.started && OnGround())
        {
            DropDownPlatform();
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = moveInput.x * modifiedSpeed;

        if ((moveInput.x > 0 && !facingRight) || (moveInput.x < 0 && facingRight)) 
        {
            Flip();
        }
    }

    private void Flip()
    {
        Vector3 currentScale = characterSprite.transform.localScale;
        currentScale.x *= -1;
        characterSprite.transform.localScale = currentScale;

        facingRight = !facingRight;
    }

    private void Jump()
    {
        rb.linearVelocity = Vector2.up * jumpForce;
    }

    void DropDownPlatform()
    {
        if (currentPlatform == null) return;

        Collider2D platformCollider = currentPlatform.GetComponent<Collider2D>();
        Collider2D playerCollider = GetComponent<Collider2D>();

        if(platformCollider != null)
        {
            StartCoroutine(DropThroughPlatform(platformCollider, playerCollider));
        }
    }

    IEnumerator DropThroughPlatform(Collider2D platform, Collider2D player)
    {
        Physics2D.IgnoreCollision(
            platform,
            player,
            true);

        yield return new WaitForSeconds(dropDuration);

        Physics2D.IgnoreCollision(
            platform,
            player,
            false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Platform"))
        {
            currentPlatform = collision.collider.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Platform"))
        {
            currentPlatform = null;
        }
    }

    public bool OnGround()
    {
        // Verifica se o personagem está no chão usando OverlapCircle.
        return Physics2D.OverlapCircle(bottomPos.position, bottomSize, floorLayer);
    }
}
