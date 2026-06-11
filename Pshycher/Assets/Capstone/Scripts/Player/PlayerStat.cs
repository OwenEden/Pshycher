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
    public float baseJumpForce = 12f;

    // 능력 보정 후 실제 스탯
    public float attack { get; private set; }
    public float defense { get; private set; }
    public float moveSpeed { get; private set; }
    public float jumpForce { get; private set; }

    private AbilityManager abilityManager;

    private void Awake()
    {
        abilityManager = GetComponent<AbilityManager>();
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

    // 능력 변경 시 AbilityManager가 호출
    public void ApplyStatModifiers(AbilityType ability)
    {
        // 기본값으로 초기화
        attack = baseAttack;
        defense = baseDefense;
        moveSpeed = baseMoveSpeed;
        jumpForce = baseJumpForce;

        switch (ability)
        {
            case AbilityType.IronHardening: // 철고화
                defense *= 1.7f;
                moveSpeed *= 0.7f;
                jumpForce *= 0.7f;
                break;

            case AbilityType.FluidBody: // 수액화
                moveSpeed *= 0.75f;
                // 수중 이동속도 증가는 별도 수중 판정 로직에서 처리
                break;

            case AbilityType.WindForm: // 풍기화
                moveSpeed *= 1.25f;
                defense *= 0.75f;
                break;

            case AbilityType.Plasma: // 플라즈마
                moveSpeed *= 1.5f;
                jumpForce *= 1.5f;
                attack *= 1.25f;
                defense *= 1.25f;
                break;

            case AbilityType.AquaStep: // 수륙발판
                // 수면 이동속도 증가는 별도 수면 판정 로직에서 처리
                break;

            case AbilityType.GroundShock: // 지면진동
                // 시전시간/쿨타임은 ToolManager에서 처리
                break;

            case AbilityType.FlameCharge:  // 화염돌진
            case AbilityType.AirSlash:     // 공기참격
            case AbilityType.GravityShift: // 자가 중력변환 (미구현)
            case AbilityType.None:
            default:
                break;
        }

        // 변경된 스탯을 PlayerController에 반영
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.OnStatUpdated(moveSpeed, jumpForce);
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

    public void OnDamage(float damage)
    {
        float reduced = Mathf.Max(0, damage - defense);
        currentHealth -= reduced;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        CheckHp();

        if (currentHealth <= 0 && !dead)
            Die();

        Debug.Log($"{gameObject.name} 이(가) {reduced} 데미지를 받았습니다. (방어력 {defense} 적용)");
    }

    public bool UseMana(float amount)
    {
        if (currentMana < amount) return false;
        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        CheckMana();
        return true;
    }

    private void Die()
    {
        dead = true;
        Debug.Log($"{gameObject.name} 사망.");
        // 사망 처리 추가 가능
    }
}