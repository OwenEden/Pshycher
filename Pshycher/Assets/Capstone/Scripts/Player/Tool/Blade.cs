using UnityEngine;

public class Blade : MonoBehaviour
{
    [Header("기본 설정")]
    public float attackRange = 1.5f;
    public float baseCooldown = 0.5f;
    public LayerMask targetLayer;

    [Header("강화 설정")]
    public float enhancedRangeMultiplier = 1.3f;
    public float enhancedDamageMultiplier = 1.5f;

    private float cooldownTimer = 0f;
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
    }

    public void Attack(bool enhanced)
    {
        float cooldown = baseCooldown * toolManager.GetCooldownMultiplier();
        if (cooldownTimer > 0f) return;

        cooldownTimer = cooldown;

        float range = attackRange * (enhanced ? enhancedRangeMultiplier : 1f);
        float damage = stat.attack * (enhanced ? enhancedDamageMultiplier : 1f);

        // 플레이어 전방 방향
        Vector2 direction = stat.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)stat.transform.position;

        // 전방 부채꼴 범위 내 타겟 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin + direction * (range / 2f), range / 2f, targetLayer);

        foreach (var hit in hits)
        {
            LivingEntity entity = hit.GetComponent<LivingEntity>();
            entity?.OnDamage(damage);
            Debug.Log($"블레이드 공격 - 대상: {hit.name} / 데미지: {damage} / 강화: {enhanced}");
        }

        // 강화 상태 추가 효과
        if (enhanced)
            SpawnEnhancedEffect(origin, direction, range);
    }

    private void SpawnEnhancedEffect(Vector2 origin, Vector2 direction, float range)
    {
        // 강화 이펙트 (파티클 등) 추가 가능
        Debug.Log("블레이드 강화 이펙트 재생");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 dir = Vector2.right;
        Gizmos.DrawWireSphere((Vector2)transform.position + dir * (attackRange / 2f), attackRange / 2f);
    }
}