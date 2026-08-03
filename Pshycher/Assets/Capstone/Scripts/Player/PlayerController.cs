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
    private LadderTrigger currentLadder = null;
    private float originalGravityScale; // 기존 중력값 저장

    [Header("지면 감지")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("사다리 하단 감지")]
    public float ladderBottomRayDistance = 0.3f; // 착지 예측 거리

    [Header("턴어라운드 설정")]
    public float turnDuration = 0.15f;      // 턴 클립 길이와 맞춰주세요
    public bool useAnimationEvent = true;   // 애니메이션 이벤트로 flip 타이밍 제어
    [Range(0f, 1f)]
    public float turnMoveSpeedRate = 0.3f;  // 턴 도중 이동속도 비율

    public bool isTurning { get; private set; }

    private bool pendingFacingRight;        // 턴 완료 후 바라볼 방향
    private float turnTimer;


    private Rigidbody2D rb;
    private AbilityManager abilityManager;


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
        velocity.x = move * moveSpeed * (isTurning ? turnMoveSpeedRate : 1f);
        rb.linearVelocity = velocity;

        HandleFlip(move);
        UpdateTurnTimer();
    }


    // ── 사다리 이동 ────────────────────────────
    void ClimbMove()
    {
        float climbInput = PlayerUIManager.Instance.GetClimb();

        // 사다리 중앙 X로 고정
        float centerX = currentLadder.centerX;
        float currentX = transform.position.x;
        float snapX = Mathf.Lerp(currentX, centerX, Time.fixedDeltaTime * 10f);
        transform.position = new Vector3(snapX, transform.position.y, transform.position.z);

        Vector2 velocity = Vector2.zero;
        velocity.y = climbInput * climbSpeed;
        rb.linearVelocity = velocity;

        isClimbing = climbInput != 0;

        if (climbInput < 0f)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                groundCheck.position,
                Vector2.down,
                ladderBottomRayDistance,
                groundLayer
            );

            if (hit.collider != null)
            {
                currentLadder?.SetButtonsActive(false);
                OnExitLadder();
                Debug.Log("사다리 하단 감지 - Climb 해제");
            }
        }
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
    public void OnEnterLadder(LadderTrigger ladder)
    {
        isOnLadder = true;
        currentLadder = ladder;
        originalGravityScale = rb.gravityScale; // 진입 시 저장
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        isClimbing = false;
        Debug.Log($"사다리 등반 시작 - 기존 중력값 저장: {originalGravityScale}");
    }

    public void OnExitLadder()
    {
        isOnLadder = false;
        rb.gravityScale = originalGravityScale; // 저장된 값으로 복구
        isClimbing = false;
        currentLadder?.SetCollisionWithLadderTop(true);
        currentLadder = null;
        Debug.Log($"사다리 이탈 - 중력값 복구: {originalGravityScale}");
    }
    private void ExitLadderByJump()
    {
        LadderTrigger ladder = currentLadder;
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

        bool wantRight = x > 0;
        if (wantRight == isFacingRight) return;   // 방향 그대로면 무시
        if (isTurning && wantRight == pendingFacingRight) return; // 이미 같은 방향으로 턴 중

        // 공중이거나 사다리 위에서는 턴 없이 즉시 전환
        if (!isGrounded || isOnLadder)
        {
            ApplyFlip(wantRight);
            return;
        }

        StartTurn(wantRight);
    }

    private void StartTurn(bool faceRight)
    {
        isTurning = true;
        pendingFacingRight = faceRight;
        turnTimer = turnDuration;

        // 애니메이션 이벤트를 쓰지 않으면 타이머 종료 시점에 flip
        Debug.Log($"턴어라운드 시작 → {(faceRight ? "오른쪽" : "왼쪽")}");
    }

    private void UpdateTurnTimer()
    {
        if (!isTurning) return;

        turnTimer -= Time.fixedDeltaTime;
        if (turnTimer > 0f) return;

        // 애니메이션 이벤트를 쓰는 경우 이미 flip 됐을 수 있음
        if (!useAnimationEvent)
            ApplyFlip(pendingFacingRight);

        isTurning = false;
    }

    // 애니메이션 이벤트에서 호출 (PlayerAnimator 경유)
    public void OnTurnFlipFrame()
    {
        if (!isTurning) return;
        ApplyFlip(pendingFacingRight);
    }

    private void ApplyFlip(bool faceRight)
    {
        isFacingRight = faceRight;
        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
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