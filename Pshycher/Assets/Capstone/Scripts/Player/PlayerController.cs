using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private bool isFacingRight = true;
    private bool isGrounded;
    public bool isSprinting;
    public bool isjumping;
    public bool isClimbing;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Move();

        if (InputManager.Instance.GetJump())
            Jump();
    }

    void Move()
    {
        float move = InputManager.Instance.GetMove();
        if (move != 0)
            isSprinting = true;
        else
            isSprinting = false;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = move * moveSpeed;

        rb.linearVelocity = velocity;
        HandleFlip(move);
    }

    void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void HandleFlip(float x)
    {
        if (x == 0) return;

        Vector3 scale = transform.localScale;

        if (x > 0)
            scale.x = Mathf.Abs(scale.x);
        else
            scale.x = -Mathf.Abs(scale.x);

        transform.localScale = scale;
    }
}