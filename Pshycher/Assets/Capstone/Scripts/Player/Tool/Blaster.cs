using UnityEngine;

public class Blaster : MonoBehaviour
{
    [Header("기본 설정")]
    public float baseRadius = 3f;
    public float baseCooldown = 1.2f;
    public LayerMask targetLayer;

    [Header("강화 설정")]
    public float enhancedRadiusMultiplier = 1.5f;
    public float enhancedDamageMultiplier = 1.6f;
    public bool enhancedKnockback = true;
    public float knockbackForce = 5f;

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
        float castTime = toolManager.GetCastTimeMultiplier();
        float cooldown = baseCooldown * toolManager.GetCooldownMultiplier();

        if (cooldownTimer > 0f) return;

        cooldownTimer = cooldown;

        if (castTime > 1f)
            StartCoroutine(BlastWithDelay(enhanced, (castTime - 1f) * 0.5f));
        else
            Blast(enhanced);
    }

    private System.Collections.IEnumerator BlastWithDelay(bool enhanced, float delay)
    {
        Debug.Log($"블래스터 시전 중... ({delay:F2}초)");
        yield return new WaitForSeconds(delay);
        Blast(enhanced);
    }

    private void Blast(bool enhanced)
    {
        float radius = baseRadius * (enhanced ? enhancedRadiusMultiplier : 1f);
        float damage = stat.attack * (enhanced ? enhancedDamageMultiplier : 1f);

        Vector2 origin = (Vector2)stat.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, targetLayer);

        foreach (var hit in hits)
        {
            LivingEntity entity = hit.GetComponent<LivingEntity>();
            entity?.OnDamage(damage);

            // 강화 시 넉백 적용
            if (enhanced && enhancedKnockback)
                ApplyKnockback(hit, origin);

            Debug.Log($"블래스터 적중 - 대상: {hit.name} / 데미지: {damage} / 강화: {enhanced}");
        }
    }

    private void ApplyKnockback(Collider2D target, Vector2 origin)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) return;

        Vector2 dir = ((Vector2)target.transform.position - origin).normalized;
        targetRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, baseRadius);
    }
}