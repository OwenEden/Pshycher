using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStat : MonoBehaviour, LivingEntity
{
    [Header("체력")]
    public Image currentHealthBar;
    public float maxHealth = 100f;
    public float currentHealth { get; protected set; }
    public bool dead { get; protected set; }

    [Header("마나")]
    public Image currentManaBar;
    public float maxMana = 100f;
    public float currentMana { get; protected set; }

    [Header("기본 스탯")]
    public float baseAttack = 10f;
    public float baseDefense = 0f;
    public float baseMoveSpeed = 5f;
    public float baseJumpForce = 7f;

    public float attack { get; private set; }
    public float defense { get; private set; }
    public float moveSpeed { get; private set; }
    public float jumpForce { get; private set; }

    [Header("수중 보정")]
    public float fluidBodyWaterBonus = 1.25f;
    public float aquaStepWaterBonus = 1.25f;

    [Header("사망 연출")]
    [Tooltip("최대 체력 대비 이 비율 이상을 한 번에 잃으면 강타로 판정")]
    [Range(0f, 1f)] public float heavyHitRatio = 0.3f;
    [Tooltip("방향 정보가 없는 일반 공격으로 사망 시")]
    public DeathFallType defaultDeathFall = DeathFallType.Backward;
    [Tooltip("사망 모션 재생 후 결과창까지 대기 시간")]
    public float gameOverDelay = 1.2f;

    private AbilityManager abilityManager;
    private PlayerController controller;
    private PlayerAnimator playerAnimator;
    private Collider2D bodyCollider;
    private Protect protect;

    private void Awake()
    {
        abilityManager = GetComponent<AbilityManager>();
        controller = GetComponent<PlayerController>();
        playerAnimator = GetComponent<PlayerAnimator>();
        bodyCollider = GetComponent<Collider2D>();
        protect = GetComponentInChildren<Protect>(true);
    }

    private void Start()
    {
        InitialSet();
    }

    public void InitialSet()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        dead = false;
        ApplyStatModifiers(AbilityType.None);
    }
    // 피벗과 무관한 몸 중심 (공격 기준점용)
    public Vector2 CenterPosition => bodyCollider != null ? (Vector2)bodyCollider.bounds.center : (Vector2)transform.position;

    // ── 능력 스탯 보정 ────────────────────────
    public void ApplyStatModifiers(AbilityType ability)
    {
        attack = baseAttack;
        defense = baseDefense;
        moveSpeed = baseMoveSpeed;
        jumpForce = baseJumpForce;

        switch (ability)
        {
            case AbilityType.IronHardening:
                defense *= 1.7f;
                moveSpeed *= 0.7f;
                jumpForce *= 0.7f;
                break;

            case AbilityType.FluidBody:
                moveSpeed *= 0.75f;   // 수중 보너스는 GetWaterSpeedMultiplier
                break;

            case AbilityType.WindForm:
                moveSpeed *= 1.25f;
                defense *= 0.75f;
                break;

            case AbilityType.Plasma:
                moveSpeed *= 1.5f;
                jumpForce *= 1.5f;
                attack *= 1.25f;
                defense *= 1.25f;
                break;

            default:
                break;
        }

        if (controller != null)
            controller.OnStatUpdated(moveSpeed, jumpForce);
    }

    public float GetWaterSpeedMultiplier()
    {
        if (abilityManager == null || !abilityManager.isAbilityActive) return 1f;

        if (abilityManager.currentAbility == AbilityType.FluidBody) return fluidBodyWaterBonus;
        if (abilityManager.currentAbility == AbilityType.AquaStep) return aquaStepWaterBonus;

        return 1f;
    }

    // ── 피해 ──────────────────────────────────
    // LivingEntity 구현 - 방향 정보 없는 일반 공격
    public void OnDamage(float damage)
    {
        OnDamage(DamageInfo.Plain(damage));
    }
    // 공격자가 바라보는 쪽에 있는지
    public bool IsFromFront(Vector2 sourcePosition)
    {
        bool facingRight = controller == null || controller.isFacingRight;
        float dx = sourcePosition.x - transform.position.x;
        return facingRight ? dx >= 0f : dx <= 0f;
    }

    public void OnDamage(DamageInfo info)
    {
        if (dead) return;
        if (info.type == DamageType.Direct && controller != null && controller.isInvincible)
            return;

        // 1. 프로텍트 방어
        float incoming = protect != null
            ? protect.CalculateBlockedDamage(info)
            : info.amount;

        // 2. 방어력
        float applied = info.ignoreDefense
            ? incoming
            : Mathf.Max(0f, incoming - defense);

        currentHealth = Mathf.Clamp(currentHealth - applied, 0f, maxHealth);
        CheckHp();

        if (currentHealth <= 0f)
            Die(DetermineDeathFall(info, applied));
    }

    // 익사 등 환경 지속 피해
    public void ApplyEnvironmentDamage(float damage)
    {
        OnDamage(DamageInfo.Continuous(damage));
    }

    private DeathFallType DetermineDeathFall(DamageInfo info, float appliedDamage)
    {
        // 1. 지속 피해 → 앞으로
        if (info.type == DamageType.Continuous)
            return DeathFallType.Forward;

        // 2. 강타 → 뒤로 (방향보다 우선)
        if (appliedDamage >= maxHealth * heavyHitRatio)
            return DeathFallType.Backward;

        // 3·4. 공격 방향
        if (info.hasSource)
        {
            bool facingRight = controller == null || controller.isFacingRight;
            float dx = info.sourcePosition.x - transform.position.x;
            bool fromFront = facingRight ? dx >= 0f : dx <= 0f;

            return IsFromFront(info.sourcePosition) ? DeathFallType.Backward : DeathFallType.Forward;
        }

        // 5. 정보 없음
        return defaultDeathFall;
    }

    // ── 추락 ──────────────────────────────────
    public bool WillDieFromFall(float rate)
    {
        return currentHealth - (maxHealth * rate) <= 0f;
    }

    public void ApplyFallDamage(float rate)
    {
        if (dead) return;

        currentHealth = Mathf.Clamp(currentHealth - maxHealth * rate, 0f, maxHealth);
        CheckHp();

        // 추락 사망은 CheckpointManager가 결과창을 직접 띄움
        if (currentHealth <= 0f)
            dead = true;
    }

    // ── 사망 / 부활 ───────────────────────────
    private void Die(DeathFallType fall)
    {
        if (dead) return;
        dead = true;

        if (controller != null) controller.OnDeath();
        if (playerAnimator != null) playerAnimator.PlayDeath(fall);

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(gameOverDelay);
        ResultManager.Instance?.ShowGameOver();
    }

    public void Revive()
    {
        dead = false;
        currentHealth = maxHealth;
        currentMana = maxMana;
        CheckHp();
        CheckMana();
    }

    // ── 마나 / UI ─────────────────────────────
    public bool UseMana(float amount)
    {
        if (currentMana < amount) return false;
        currentMana = Mathf.Clamp(currentMana - amount, 0f, maxMana);
        CheckMana();
        return true;
    }

    public void CheckHp()
    {
        if (currentHealthBar != null)
            currentHealthBar.fillAmount = currentHealth / maxHealth;
    }

    public void CheckMana()
    {
        if (currentManaBar != null)
            currentManaBar.fillAmount = currentMana / maxMana;
    }
}