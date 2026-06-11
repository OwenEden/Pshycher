using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float moveSpeed = 5f;
    private float jumpForce = 7f;

    public bool isFacingRight { get; private set; } = true;
    public bool isSprinting { get; private set; }
    public bool isJumping { get; private set; }
    public bool isGrounded { get; private set; }
    public bool isClimbing { get; private set; }
    public bool isOnLadder { get; private set; } = false; // Ladder.cs에서 참조

    [Header("점프 설정")]
    public int maxJumpCount = 2;
    private int jumpCountRemaining = 0;

    [Header("사다리 설정")]
    public float climbSpeed = 3f;

    private Ladder currentLadder = null;

    private Rigidbody2D rb;
    private AbilityManager abilityManager;

    [Header("지면 감지")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilityManager = GetComponent<AbilityManager>();
    }

    void FixedUpdate()
    {
        CheckGround();

        if (isOnLadder)
            ClimbMove();
        else
            Move();

        if (PlayerUIManager.Instance.GetJump())
        {
            if (isOnLadder)
                ExitLadderByJump();
            else
                TryJump();
        }
    }

    // ── 일반 이동 ──────────────────────────────
    void Move()
    {
        float move = PlayerUIManager.Instance.GetMove();
        isSprinting = move != 0;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = move * moveSpeed;
        rb.linearVelocity = velocity;

        HandleFlip(move);
    }

    // ── 사다리 이동 ────────────────────────────
    void ClimbMove()
    {
        float climbInput = PlayerUIManager.Instance.GetClimb();

        Vector2 velocity = Vector2.zero;
        velocity.y = climbInput * climbSpeed;
        rb.linearVelocity = velocity;

        isClimbing = climbInput != 0;
    }

    // ── 점프 ───────────────────────────────────
    void TryJump()
    {
        if (jumpCountRemaining <= 0) return;

        jumpCountRemaining--;
        isJumping = true;

        if (!isGrounded)
        {
            Vector2 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;
        }

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        Debug.Log($"점프 - 남은 횟수: {jumpCountRemaining}");
    }

    // ── 사다리 진입/이탈 ───────────────────────
    public void OnEnterLadder(Ladder ladder)
    {
        isOnLadder = true;
        currentLadder = ladder;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        isClimbing = false;

        // Player-Ground 충돌 비활성화
        ladder.SetCollisionWithGround(false);
        Debug.Log("사다리 등반 시작");
    }

    public void OnExitLadder()
    {
        isOnLadder = false;
        rb.gravityScale = 3.7f;
        isClimbing = false;

        // Player-Ground 충돌 복구
        currentLadder?.SetCollisionWithGround(true);
        currentLadder = null;
        Debug.Log("사다리 이탈");
    }
    private void ExitLadderByJump()
    {
        Ladder ladder = currentLadder;
        OnExitLadder();
        ladder?.SetButtonsActive(false);
        ResetJumpCount();
        TryJump();
        Debug.Log("사다리에서 점프 이탈");
    }

    // ── 공통 ───────────────────────────────────
    void CheckGround()
    {
        bool wasGrounded = isGrounded;

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            isJumping = false;
            ResetJumpCount();

            // 착지 시 사다리 이탈
            if (isOnLadder)
                OnExitLadder();
        }
    }

    void HandleFlip(float x)
    {
        if (x == 0) return;
        isFacingRight = x > 0;
        Vector3 scale = transform.localScale;
        scale.x = isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void OnStatUpdated(float newMoveSpeed, float newJumpForce)
    {
        moveSpeed = newMoveSpeed;
        jumpForce = newJumpForce;
    }

    public void SetMaxJumpCount(int count)
    {
        maxJumpCount = count;
        jumpCountRemaining = Mathf.Min(jumpCountRemaining, maxJumpCount);
        Debug.Log($"최대 점프 횟수 변경: {maxJumpCount}");
    }

    public void ResetJumpCount()
    {
        jumpCountRemaining = maxJumpCount;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isJumping = false;
            ResetJumpCount();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}