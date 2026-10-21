using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float moveSpeed = 5f;
    private float jumpForce = 7f;

    // ── 상태 ──────────────────────────────────
    public bool isFacingRight { get; private set; } = true;
    public bool isSprinting { get; private set; }
    public bool isJumping { get; private set; }
    public bool isGrounded { get; private set; }
    public bool isClimbing { get; private set; }
    public bool isOnLadder { get; private set; } = false;
    public bool isTurning { get; private set; }
    public bool isInWind { get; private set; }
    public bool isInWater { get; private set; }
    public bool isSwimming { get; private set; }
    public bool isSwimMoving { get; private set; }

    // 땅·사다리·물에서 벗어날 때마다 증가 — 공중 1회 제한 판정용
    public int landingCount { get; private set; }

    [Header("점프 설정")]
    public int maxJumpCount = 2;
    private int jumpCountRemaining = 0;

    [Header("사다리 설정")]
    public float climbSpeed = 3f;
    public float ladderBottomRayDistance = 0.3f;

    [Header("턴어라운드 설정")]
    public float turnDuration = 0.15f;
    [Range(0f, 1f)] public float turnMoveSpeedRate = 0.3f;

    // [관성] ─────────────────────────────────
    [Header("관성 (실험)")]
    public bool useInertia = true;
    public float skidDuration = 0.25f;     // 미끄러지는 시간 (전체 동안 방향전환 모션)
    public float skidMinSpeed = 2f;        // 이 속도 이상으로 달리다 꺾을 때만 미끄러짐

    public bool isSkidding { get; private set; }
    private float skidTimer;
    private float skidStartSpeed;
    // ────────────────────────────────────────

    // [대쉬] ─────────────────────────────────
    [Header("대쉬")]
    public AbilityType dashAbility = AbilityType.Plasma;
    public float dashSpeed = 18f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public bool allowAirDash = true;
    public bool invincibleWhileDashing = true;

    public bool isDashing { get; private set; }
    public bool isInvincible => isDashing && invincibleWhileDashing;

    public bool IsDashAbilityActive =>
        abilityManager != null && abilityManager.isAbilityActive
        && abilityManager.currentAbility == dashAbility;

    private float dashTimer;
    private float dashDirection;
    private float nextDashTime;
    private int airDashLanding = -1;
    // ────────────────────────────────────────

    [Header("수영 모션")]
    public float swimStrokeAnimTime = 0.35f;

    [Header("지면 감지")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;          // 일반 지면 (Ground)
    public LayerMask oneWayGroundLayer;    // 위에서만 밟히는 발판 (LadderTop)

    private Rigidbody2D rb;
    private AbilityManager abilityManager;
    private PlayerStat stat;

    private LadderTrigger currentLadder = null;
    private WindZone2D currentWind = null;
    private WaterZone2D currentWater = null;

    private float originalGravityScale;

    private bool pendingFacingRight;
    private float turnTimer;

    private float waterDrainTimer;
    private float drainTickTimer;

    private bool canSwimFreely;
    private bool verticalSwimControl;
    private float swimStrokeTimer;

    private bool IsHeavyInWater =>
        isInWater && currentWater != null && currentWater.IsHeavy(abilityManager);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilityManager = GetComponent<AbilityManager>();
        stat = GetComponent<PlayerStat>();
        originalGravityScale = rb.gravityScale;
    }

    void FixedUpdate()
    {
        CheckGround();
        CheckSwimInterrupt();
        UpdateFreeSwim();
        ApplyGravityScale();

        if (PlayerUIManager.Instance.GetDash())   // [대쉬]
            TryDash();

        if (isDashing) UpdateDash();
        else if (isOnLadder) ClimbMove();
        else if (isSwimming) SwimMove();
        else Move();

        ApplyWind();
        ApplyBuoyancy();
        ApplyWaterDrain();

        // 대쉬 중 누른 점프는 소비하지 않고 대쉬가 끝난 뒤 처리
        if (!isDashing && PlayerUIManager.Instance.GetJump())
            HandleJumpInput();
    }

    // ── 점프 입력 분기 ─────────────────────────
    private void HandleJumpInput()
    {
        if (isOnLadder)
        {
            ExitLadderByJump();
            return;
        }

        if (isSwimming)
        {
            SwimStroke();
            return;
        }

        // 물속에서 철고화가 아니면 수영 시작
        if (isInWater && currentWater != null && !IsHeavyInWater)
        {
            StartSwimming();
            return;
        }

        TryJump();
    }

    // ── 일반 이동 ──────────────────────────────
    void Move()
    {
        float move = PlayerUIManager.Instance.GetMove();
        isSprinting = move != 0;

        HandleFlip(move);

        if (isSkidding)   // [관성]
        {
            UpdateSkid();
        }
        else
        {
            float speedRate = isTurning ? turnMoveSpeedRate : 1f;
            if (isInWater && stat != null)
                speedRate *= stat.GetWaterSpeedMultiplier();

            Vector2 velocity = rb.linearVelocity;
            velocity.x = move * moveSpeed * speedRate;
            rb.linearVelocity = velocity;
        }

        UpdateTurnTimer();
    }

    // ── 수영 ──────────────────────────────────
    void SwimMove()
    {
        float move = PlayerUIManager.Instance.GetMove();
        float vertical = PlayerUIManager.Instance.GetClimb();
        isSprinting = move != 0;

        float speedRate = stat != null ? stat.GetWaterSpeedMultiplier() : 1f;

        Vector2 v = rb.linearVelocity;
        v.x = move * moveSpeed * speedRate;

        // 수액화일 때만 상하 직접 제어 (이때는 부력 미적용)
        verticalSwimControl = canSwimFreely && vertical != 0f;
        if (verticalSwimControl)
            v.y = vertical * currentWater.swimVerticalSpeed * speedRate;

        rb.linearVelocity = v;

        HandleFlip(move);
        UpdateTurnTimer();

        if (swimStrokeTimer > 0f)
            swimStrokeTimer -= Time.fixedDeltaTime;

        isSwimMoving = move != 0f || verticalSwimControl || swimStrokeTimer > 0f;
    }

    private void SwimStroke()
    {
        if (currentWater == null) return;

        // 능력 보정 전 기본 점프력 기준 — 수영 배율만 적용
        float baseJump = stat != null ? stat.baseJumpForce : jumpForce;
        float strokeVelocity = (baseJump / rb.mass) * currentWater.swimJumpMultiplier;

        Vector2 v = rb.linearVelocity;
        v.y = Mathf.Min(Mathf.Max(v.y, 0f) + strokeVelocity, currentWater.maxSwimRiseSpeed);
        rb.linearVelocity = v;

        swimStrokeTimer = swimStrokeAnimTime;
    }

    private void StartSwimming()
    {
        EndDash();
        CancelSkid();

        if (isSwimming) return;

        isSwimming = true;
        isJumping = false;

        SwimStroke();   // 진입과 동시에 한 번 떠오름
    }

    private void StopSwimming()
    {
        if (!isSwimming) return;

        isSwimming = false;
        canSwimFreely = false;
        verticalSwimControl = false;
        isSwimMoving = false;
        PlayerUIManager.Instance?.RequestVerticalButtons(this, false);
    }

    // 수영 중 + 수액화일 때만 상하 버튼 표시
    private void UpdateFreeSwim()
    {
        bool free = isSwimming && currentWater != null
                 && currentWater.CanSwimFreely(abilityManager);

        if (free == canSwimFreely) return;

        canSwimFreely = free;
        PlayerUIManager.Instance?.RequestVerticalButtons(this, free);
    }

    // 수영 중 물을 벗어났거나 철고화를 켰으면 해제
    private void CheckSwimInterrupt()
    {
        if (!isSwimming) return;

        if (currentWater == null || IsHeavyInWater)
            StopSwimming();
    }

    // ── 사다리 이동 ────────────────────────────
    void ClimbMove()
    {
        float climbInput = PlayerUIManager.Instance.GetClimb();

        float centerX = currentLadder.centerX;
        float snapX = Mathf.Lerp(transform.position.x, centerX, Time.fixedDeltaTime * 10f);
        transform.position = new Vector3(snapX, transform.position.y, transform.position.z);

        Vector2 velocity = Vector2.zero;
        velocity.y = climbInput * climbSpeed;
        rb.linearVelocity = velocity;

        isClimbing = climbInput != 0;

        if (climbInput < 0f)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                groundCheck.position, Vector2.down, ladderBottomRayDistance, groundLayer);

            if (hit.collider != null)
            {
                currentLadder?.SetButtonsActive(false);
                OnExitLadder();
            }
        }
    }

    // ── 중력 ──────────────────────────────────
    private void ApplyGravityScale()
    {
        if (isDashing) { rb.gravityScale = 0f; return; }   // [대쉬]
        if (isOnLadder) { rb.gravityScale = 0f; return; }

        if (isInWater && currentWater != null)
        {
            rb.gravityScale = 0f;   // 물속에서는 부력이 세로 움직임을 담당
            return;
        }

        rb.gravityScale = originalGravityScale;
    }

    // ── 구역 효과 ─────────────────────────────
    private void ApplyWind()
    {
        if (isDashing) return;   // [대쉬]
        if (!isInWind || currentWind == null || isOnLadder) return;
        if (currentWind.IsImmune(abilityManager)) return;

        rb.linearVelocity += currentWind.GetWindVelocity() * Time.fixedDeltaTime;
    }

    private void ApplyBuoyancy()
    {
        if (!isInWater || currentWater == null) return;
        if (isOnLadder || isDashing) return;
        if (verticalSwimControl) return;   // 직접 상하로 헤엄치는 중

        float target = -currentWater.GetSinkSpeed(abilityManager);
        Vector2 v = rb.linearVelocity;

        // 목표보다 위로 가는 중이면 부력이 끌어내리고, 더 빨리 떨어지는 중이면 저항이 늦춤
        float rate = v.y > target ? currentWater.riseDeceleration : currentWater.fallDrag;

        v.y = Mathf.MoveTowards(v.y, target, rate * Time.fixedDeltaTime);
        rb.linearVelocity = v;
    }

    private void ApplyWaterDrain()
    {
        if (!isInWater || currentWater == null) return;
        if (stat == null || stat.dead) return;

        if (currentWater.IsDrainImmune(abilityManager))
        {
            waterDrainTimer = 0f;
            drainTickTimer = 0f;
            return;
        }

        waterDrainTimer += Time.fixedDeltaTime;
        if (waterDrainTimer < currentWater.drainStartDelay) return;

        drainTickTimer -= Time.fixedDeltaTime;
        if (drainTickTimer <= 0f)
        {
            stat.ApplyEnvironmentDamage(currentWater.drainAmount);
            drainTickTimer = Mathf.Max(0.01f, currentWater.drainInterval);
        }
    }

    public void EnterWindZone(WindZone2D zone)
    {
        currentWind = zone;
        isInWind = true;
    }

    public void ExitWindZone(WindZone2D zone)
    {
        if (currentWind != zone) return;
        currentWind = null;
        isInWind = false;
    }

    public void EnterWaterZone(WaterZone2D zone)
    {
        currentWater = zone;
        isInWater = true;
        waterDrainTimer = 0f;
        drainTickTimer = 0f;
    }

    public void ExitWaterZone(WaterZone2D zone)
    {
        if (currentWater != zone) return;

        landingCount++;

        bool wasSwimming = isSwimming;
        StopSwimming();

        currentWater = null;
        isInWater = false;
        rb.gravityScale = originalGravityScale;
        waterDrainTimer = 0f;
        drainTickTimer = 0f;

        // 헤엄쳐서 물 밖으로 나오면 공중 점프를 남겨둠
        if (wasSwimming && !isGrounded)
            jumpCountRemaining = Mathf.Max(0, maxJumpCount - 1);
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
    }

    // ── 사다리 진입/이탈 ───────────────────────
    public void OnEnterLadder(LadderTrigger ladder)
    {
        EndDash();
        CancelSkid();
        landingCount++;
        StopSwimming();

        isOnLadder = true;
        currentLadder = ladder;
        rb.linearVelocity = Vector2.zero;
        isClimbing = false;
    }

    public void OnExitLadder()
    {
        isOnLadder = false;
        isClimbing = false;
        currentLadder?.SetCollisionWithLadderTop(true);
        currentLadder = null;
    }

    private void ExitLadderByJump()
    {
        LadderTrigger ladder = currentLadder;
        OnExitLadder();
        ladder?.SetButtonsActive(false);
        ResetJumpCount();
        TryJump();
    }

    // ── 방향 전환 ─────────────────────────────
    void HandleFlip(float x)
    {
        if (x == 0) return;

        bool wantRight = x > 0;
        if (wantRight == isFacingRight) return;
        if (isTurning && wantRight == pendingFacingRight) return;

        if (!isGrounded || isOnLadder || isSwimming)
        {
            ApplyFlip(wantRight);
            return;
        }

        // [관성] 빠르게 달리다 반대로 꺾으면 미끄러짐
        if (useInertia && Mathf.Abs(rb.linearVelocity.x) >= skidMinSpeed)
        {
            StartSkid(wantRight);
            return;
        }

        StartTurn(wantRight, turnDuration);
    }

    private void StartTurn(bool faceRight, float duration)
    {
        isTurning = true;
        pendingFacingRight = faceRight;
        turnTimer = duration;
    }

    private void UpdateTurnTimer()
    {
        if (!isTurning || isSkidding) return;   // 미끄러지는 중엔 UpdateSkid가 관리

        turnTimer -= Time.fixedDeltaTime;
        if (turnTimer > 0f) return;

        FinishTurn();
    }

    // 방향전환 모션 종료와 동시에 실제로 뒤집기
    private void FinishTurn()
    {
        if (!isTurning) return;
        ApplyFlip(pendingFacingRight);
        isTurning = false;
    }

    private void ApplyFlip(bool faceRight)
    {
        isFacingRight = faceRight;
        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // 외부에서 시작 방향 지정
    public void SetFacing(bool faceRight)
    {
        CancelSkid();
        isTurning = false;
        ApplyFlip(faceRight);
    }

    // [관성] ─────────────────────────────────
    private void StartSkid(bool faceRight)
    {
        isSkidding = true;
        skidTimer = 0f;
        skidStartSpeed = rb.linearVelocity.x;

        StartTurn(faceRight, skidDuration);   // 미끄러지는 동안 내내 방향전환 모션
    }

    private void UpdateSkid()
    {
        skidTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(skidTimer / skidDuration);

        Vector2 v = rb.linearVelocity;
        v.x = Mathf.Lerp(skidStartSpeed, 0f, t);
        rb.linearVelocity = v;

        // 다 미끄러졌거나 점프 등으로 땅을 떠나면 그 자리에서 방향전환 완료
        if (t >= 1f || !isGrounded)
        {
            isSkidding = false;
            FinishTurn();
        }
    }

    private void CancelSkid() => isSkidding = false;
    // ────────────────────────────────────────

    // [대쉬] ─────────────────────────────────
    private void TryDash()
    {
        if (isDashing || !IsDashAbilityActive) return;
        if (isOnLadder || isSwimming) return;
        if (Time.time < nextDashTime) return;

        if (!isGrounded)
        {
            if (!allowAirDash || airDashLanding == landingCount) return;
            airDashLanding = landingCount;
        }

        // 방향전환·미끄러짐 중이면 마무리하고 새로 바라보는 쪽으로
        CancelSkid();
        FinishTurn();

        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = isFacingRight ? 1f : -1f;
        nextDashTime = Time.time + dashDuration + dashCooldown;

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
    }

    private void UpdateDash()
    {
        dashTimer -= Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        if (dashTimer <= 0f)
            EndDash();
    }

    private void EndDash()
    {
        if (!isDashing) return;
        isDashing = false;

        // 대쉬 속도가 이어지지 않게 정리 — 다음 프레임에 Move가 입력대로 다시 설정
        Vector2 v = rb.linearVelocity;
        v.x = 0f;
        rb.linearVelocity = v;
    }
    // ────────────────────────────────────────

    // ── 사망 ──────────────────────────────────
    public void OnDeath()
    {
        EndDash();
        CancelSkid();

        if (isOnLadder)
        {
            currentLadder?.SetButtonsActive(false);
            OnExitLadder();
        }
        StopSwimming();

        isTurning = false;
        isSprinting = false;

        // 사다리 위에서 죽어도 바닥으로 떨어지도록 중력 복구
        ApplyGravityScale();

        Vector2 v = rb.linearVelocity;
        v.x = 0f;
        rb.linearVelocity = v;

        enabled = false;
    }

    // ── 지면 / 스탯 ───────────────────────────
    void CheckGround()
    {
        bool wasGrounded = isGrounded;

        if (groundCheck != null)
        {
            bool onGround = Physics2D.OverlapCircle(
                groundCheck.position, groundCheckRadius, groundLayer);

            // 사다리를 타는 중이 아니고, 올라가는 중이 아닐 때만 발판을 지면으로 인정
            bool onPlatform = !isOnLadder
                && rb.linearVelocity.y <= 0.01f
                && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, oneWayGroundLayer);

            isGrounded = onGround || onPlatform;
        }

        if (!wasGrounded && isGrounded)
        {
            isJumping = false;
            ResetJumpCount();
            landingCount++;

            if (isOnLadder) OnExitLadder();
        }
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
    }

    public void ResetJumpCount()
    {
        jumpCountRemaining = maxJumpCount;
    }
}