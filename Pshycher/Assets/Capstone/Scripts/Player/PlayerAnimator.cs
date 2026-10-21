using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("수영 모션 속도")]
    public float swimIdleSpeed = 0.4f;          // 가만히 떠 있을 때 재생 속도
    public float swimSpeedChangeRate = 4f;      // 속도가 바뀌는 빠르기

    private float swimAnimSpeed = 1f;

    [Header("블레이드 공중 공격")]
    public string bladeAirAttackState = "Base Layer.Blade State.Blade_AirAttack";

    [Header("버스터 사격 자세")]
    public float busterShootPoseTime = 0.35f;
    [Tooltip("이동↔이동 사격 전환 시 다리 동작을 이어서 재생 (두 클립 프레임 수가 같아야 함)")]
    public bool syncRunShoot = true;
    public string busterMoveState = "Base Layer.Buster State.Buster_Move";
    public string busterMoveAttackState = "Base Layer.Buster State.Buster_MoveAttack";
    public string busterAttackState = "Base Layer.Buster State.Buster_Attack";
    public string busterJumpAttackState = "Base Layer.Buster State.Buster_JumpAttack";
    [Tooltip("이동 사격 중에도 쏠 때마다 처음부터 재생 (다리 동작이 끊길 수 있음)")]
    public bool restartMoveAttack = false;

    [Header("공중 판정")]
    [Tooltip("이 시간 이상 떠 있어야 공중으로 판정 (타일 이음새에서 깜빡임 방지)")]
    public float airborneGraceTime = 0.05f;

    private bool airAttacking;
    private float airTimer;
    private float shootPoseTimer;
    private bool wasShooting;
    private bool pendingRecoil;   // 발사 후 사격 State에 들어가면 반동 재생

    private Animator animator;
    private PlayerController controller;
    private ToolManager toolManager;
    private Protect protect;
    private Blade blade;
    private PlayerStat stat;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        toolManager = GetComponent<ToolManager>();
        stat = GetComponent<PlayerStat>();
        protect = GetComponentInChildren<Protect>(true);
        blade = GetComponentInChildren<Blade>(true);
    }

    void Update()
    {
        // 사망 후에는 파라미터를 건드리지 않음
        if (stat != null && stat.dead) return;

        animator.SetBool("isMoving", controller.isSprinting);
        animator.SetBool("isJumping", controller.isJumping);
        // 점프로 뜬 건 즉시, 걸어서 떨어진 건 유예 시간 뒤에 공중 처리
        if (controller.isGrounded) airTimer = 0f;
        else airTimer += Time.deltaTime;

        bool grounded = controller.isGrounded
                     || (!controller.isJumping && airTimer < airborneGraceTime);
        animator.SetBool("isGrounded", grounded);
        animator.SetBool("isOnLadder", controller.isOnLadder);
        animator.SetBool("isTurning", controller.isTurning);
        animator.SetBool("isSwimming", controller.isSwimming);

        if (toolManager != null)
            animator.SetInteger("currentTool", (int)toolManager.currentTool);

        if (protect != null)
            animator.SetBool("isBlocking", protect.isBlocking);

        // Climb State의 Speed Multiplier - 사다리에서 멈추면 클립만 정지
        bool climbPaused = controller.isOnLadder && !controller.isClimbing;
        animator.SetFloat("climbSpeed", climbPaused ? 0f : 1f);
        // 착지, 사다리, 수영, 도구 교체 시 공중 공격 자세 해제
        if (airAttacking &&
            (controller.isGrounded || controller.isOnLadder || controller.isSwimming
             || toolManager == null || toolManager.currentTool != ToolType.Blade))
        {
            airAttacking = false;
        }
        animator.SetBool("isAirAttacking", airAttacking);
        UpdateBusterShootPose();

        float targetSwim = controller.isSwimMoving ? 1f : swimIdleSpeed;
        swimAnimSpeed = Mathf.MoveTowards(swimAnimSpeed, targetSwim, swimSpeedChangeRate * Time.deltaTime);
        animator.SetFloat("swimSpeed", swimAnimSpeed);
    }

    // ── 공격 ──────────────────────────────────
    public void PlayAttackAnim(ToolType tool)
    {
        switch (tool)
        {
            case ToolType.Buster: break;
            case ToolType.Rope: animator.SetTrigger("attackRope"); break;
            case ToolType.Blaster: animator.SetTrigger("attackBlaster"); break;
                // Blade는 PlayBladeCombo, Protect는 isBlocking이 담당
        }
    }

    public void PlayBladeCombo(int step)
    {
        animator.SetInteger("bladeCombo", step);
        animator.SetTrigger("attackBlade");
    }

    public void ResetBladeCombo()
    {
        animator.SetInteger("bladeCombo", 0);
    }

    // Animation Event
    public void OnBladeHitFrame() => blade?.OnBladeHitFrame();
    public void OnBladeComboEnd() => blade?.OnComboEnd();

    // ── 능력 / 사망 ───────────────────────────
    public void PlayAbilityAnim(AbilityType ability)
    {
        animator.SetTrigger("abilityActivate");
        animator.SetInteger("currentAbility", (int)ability);
    }

    public void PlayDeath(DeathFallType type)
    {
        animator.SetFloat("climbSpeed", 1f);   // 사다리에서 멈춘 채 죽었을 때 대비
        animator.SetBool("isDead", true);
        animator.SetInteger("deathType", (int)type);
        animator.SetTrigger("die");
    }
    public void PlayBladeAirAttack()
    {
        // 이미 공중 공격 중이면 처음부터 다시 재생
        var info = animator.GetCurrentAnimatorStateInfo(0);
        if (airAttacking && info.IsName(bladeAirAttackState))
            animator.Play(info.fullPathHash, 0, 0f);

        airAttacking = true;
        animator.SetBool("isAirAttacking", true);
    }
    private void UpdateBusterShootPose()
    {
        if (shootPoseTimer > 0f)
            shootPoseTimer -= Time.deltaTime;

        bool holdingBuster = toolManager != null && toolManager.currentTool == ToolType.Buster;
        bool shooting = holdingBuster && shootPoseTimer > 0f;

        // 달리는 중 사격 자세가 바뀌면 다리 프레임을 이어서 재생
        if (syncRunShoot && holdingBuster && shooting != wasShooting && IsRunningOnGround())
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);

            if (info.IsName(busterMoveState) || info.IsName(busterMoveAttackState))
            {
                float t = info.normalizedTime % 1f;
                animator.Play(shooting ? busterMoveAttackState : busterMoveState, 0, t);
            }
        }

        animator.SetBool("isShooting", shooting);
        // 사격 State에 들어가 있으면 반동을 처음부터 재생
        if (pendingRecoil)
        {
            if (!shooting)
            {
                pendingRecoil = false;
            }
            else
            {
                var info = animator.GetCurrentAnimatorStateInfo(0);

                if (info.IsName(busterAttackState) || info.IsName(busterJumpAttackState)
                    || (restartMoveAttack && info.IsName(busterMoveAttackState)))
                {
                    animator.Play(info.fullPathHash, 0, 0f);
                    pendingRecoil = false;
                }
                else if (info.IsName(busterMoveAttackState))
                {
                    pendingRecoil = false;   // 이동 사격은 다리 연결 우선
                }
                // 아직 사격 State 진입 전이면 다음 프레임에 다시 확인
            }
        }
        wasShooting = shooting;
    }
    // 버튼을 누른 순간 - 반동 없이 겨눈 자세로 진입
    public void OnBusterAim(float windup)
    {
        shootPoseTimer = Mathf.Max(shootPoseTimer, windup + busterShootPoseTime);
    }

    // 실제 발사 순간 - 반동 재생 예약
    public void OnBusterFired()
    {
        shootPoseTimer = Mathf.Max(shootPoseTimer, busterShootPoseTime);
        pendingRecoil = true;
    }

    private bool IsRunningOnGround()
    {
        return controller.isSprinting && controller.isGrounded
            && !controller.isOnLadder && !controller.isSwimming;
    }
}