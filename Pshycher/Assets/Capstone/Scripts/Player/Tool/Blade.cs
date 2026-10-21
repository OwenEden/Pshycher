using UnityEngine;

public class Blade : MonoBehaviour
{
    [Header("콤보 설정")]
    public int maxCombo = 3;
    public float comboResetTime = 1.2f;     // 이 시간 지나면 1단부터 다시
    public float comboInputDelay = 0.25f;   // 다음 입력을 받기까지 최소 간격

    [Header("단계별 공격 범위")]
    public float[] attackRanges = { 1.5f, 1.6f, 2.2f };

    [Header("단계별 데미지 배율")]
    public float[] damageMultipliers = { 1f, 1.1f, 1.6f };

    [Header("강화 설정")]
    public float enhancedRangeMultiplier = 1.3f;
    public float enhancedDamageMultiplier = 1.5f;

    [Header("공통")]
    public LayerMask targetLayer;

    public int currentCombo { get; private set; } = 0;   // 0 = 대기, 1~3 = 진행 중

    private float comboTimer;      // 콤보 유지 시간
    private float inputLockTimer;  // 연타 방지
    private int airAttackLanding = -1;   // 공중 공격을 쓴 체공의 번호

    private PlayerStat stat;
    private PlayerController controller;
    private ToolManager toolManager;
    private PlayerAnimator playerAnimator;

    // 이번 타격에 적용할 값 (Animation Event 시점에 사용)
    private int pendingStep;
    private bool pendingEnhanced;

    void Awake()
    {
        stat = GetComponentInParent<PlayerStat>();
        controller = GetComponentInParent<PlayerController>();
        toolManager = GetComponentInParent<ToolManager>();
        playerAnimator = GetComponentInParent<PlayerAnimator>();
    }

    void Update()
    {
        if (inputLockTimer > 0f)
            inputLockTimer -= Time.deltaTime;

        // 콤보 유지 시간 경과 시 초기화
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                ResetCombo();
        }
    }

    private void OnDisable()
    {
        ResetCombo();   // 도구 교체 시 콤보 초기화
    }

    public void Attack(bool enhanced)
    {
        if (inputLockTimer > 0f) return;

        bool airborne = controller != null
            && !controller.isGrounded
            && !controller.isOnLadder
            && !controller.isSwimming;

        // 이번 체공에 이미 공중 공격을 썼으면 무시
        if (airborne && controller.landingCount == airAttackLanding) return;

        int step;

        if (enhanced || airborne)
        {
            step = maxCombo;
        }
        else
        {
            step = currentCombo + 1;
            if (step > maxCombo) step = 1;
        }

        currentCombo = step;
        comboTimer = comboResetTime;
        inputLockTimer = comboInputDelay * toolManager.GetCooldownMultiplier();

        pendingStep = step;
        pendingEnhanced = enhanced;

        if (airborne)
        {
            airAttackLanding = controller.landingCount;
            playerAnimator?.PlayBladeAirAttack();
        }
        else
        {
            playerAnimator?.PlayBladeCombo(step);
        }
    }

    // Animation Event에서 호출 (PlayerAnimator 경유)
    public void OnBladeHitFrame()
    {
        int index = Mathf.Clamp(pendingStep - 1, 0, attackRanges.Length - 1);

        float range = attackRanges[index];
        float damage = stat.attack * damageMultipliers[index];

        if (pendingEnhanced)
        {
            range *= enhancedRangeMultiplier;
            damage *= enhancedDamageMultiplier;
        }

        Vector2 direction = stat.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = stat.CenterPosition;
        Vector2 center = origin + direction * (range * 0.5f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, range * 0.5f, targetLayer);

        foreach (var hit in hits)
        {
            LivingEntity entity = hit.GetComponent<LivingEntity>();
            entity?.OnDamage(damage);
            Debug.Log($"블레이드 {pendingStep}단 적중 - {hit.name} / 데미지 {damage:F1}");
        }
    }

    // 마지막 단계 모션이 끝났을 때 호출 (선택)
    public void OnComboEnd()
    {
        if (pendingStep >= maxCombo)
            ResetCombo();
    }

    private void ResetCombo()
    {
        currentCombo = 0;
        comboTimer = 0f;
        playerAnimator?.ResetBladeCombo();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackRanges == null || attackRanges.Length == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < attackRanges.Length; i++)
        {
            float r = attackRanges[i];
            Gizmos.DrawWireSphere((Vector2)transform.position + Vector2.right * (r * 0.5f), r * 0.5f);
        }
    }
}