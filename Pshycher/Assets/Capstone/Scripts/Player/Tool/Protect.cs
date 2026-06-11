using UnityEngine;

public class Protect : MonoBehaviour
{
    [Header("기본 설정")]
    public float baseBlockDamageReduction = 0.8f; // 기본 80% 피해 감소
    public float baseDuration = 2f;
    public float baseCooldown = 3f;

    [Header("강화 설정")]
    public float enhancedBlockDamageReduction = 0.95f; // 강화 95% 피해 감소
    public float enhancedDuration = 3f;
    public bool enhancedReflect = true; // 강화 시 피해 반사 여부

    public bool isBlocking { get; private set; } = false;

    private float cooldownTimer = 0f;
    private float durationTimer = 0f;
    private bool currentEnhanced = false;

    private PlayerStat stat;
    private ToolManager toolManager;

    void Awake()
    {
        stat = GetComponentInParent<PlayerStat>();
        toolManager = GetComponentInParent<ToolManager>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (isBlocking)
        {
            durationTimer -= Time.deltaTime;
            if (durationTimer <= 0f)
                EndBlock();
        }
    }

    public void Activate(bool enhanced)
    {
        if (cooldownTimer > 0f || isBlocking) return;

        currentEnhanced = enhanced;
        isBlocking = true;
        durationTimer = enhanced ? enhancedDuration : baseDuration;

        Debug.Log($"프로텍트 발동 - 강화: {enhanced} / 지속: {durationTimer}초");
    }

    // PlayerStat.OnDamage 대신 이 메서드로 피해 처리 (PlayerStat에서 연동 필요)
    public float CalculateBlockedDamage(float incomingDamage, Vector2 attackDirection)
    {
        if (!isBlocking) return incomingDamage;

        // 전방에서 오는 공격만 방어
        Vector2 facing = stat.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float dot = Vector2.Dot(facing, attackDirection.normalized);

        if (dot <= 0) return incomingDamage; // 후방 공격은 방어 불가

        float reduction = currentEnhanced ? enhancedBlockDamageReduction : baseBlockDamageReduction;
        float blocked = incomingDamage * reduction;
        float remaining = incomingDamage - blocked;

        Debug.Log($"프로텍트 방어 - 원본: {incomingDamage} / 감소: {blocked} / 잔여: {remaining}");

        if (currentEnhanced && enhancedReflect)
            ReflectDamage(blocked * 0.3f, attackDirection);

        return remaining;
    }

    private void ReflectDamage(float damage, Vector2 attackDirection)
    {
        // 공격 방향 역방향으로 레이캐스트해 반사 피해
        RaycastHit2D hit = Physics2D.Raycast(
            stat.transform.position,
            -attackDirection,
            3f
        );

        if (hit.collider != null)
        {
            LivingEntity entity = hit.collider.GetComponent<LivingEntity>();
            entity?.OnDamage(damage);
            Debug.Log($"프로텍트 반사 - 대상: {hit.collider.name} / 데미지: {damage}");
        }
    }

    private void EndBlock()
    {
        isBlocking = false;
        cooldownTimer = baseCooldown * toolManager.GetCooldownMultiplier();
        Debug.Log("프로텍트 종료");
    }
}