using System.Collections;
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

    private PlayerStat stat;
    private ToolManager toolManager;

    void Awake()
    {
        stat = GetComponentInParent<PlayerStat>();
        toolManager = GetComponentInParent<ToolManager>();
    }

    public void Attack(bool enhanced)
    {
        if (toolManager.IsOnCooldown(ToolType.Blaster)) return;

        toolManager.StartCooldown(ToolType.Blaster, baseCooldown);

        float castTime = toolManager.GetCastTimeMultiplier();
        if (castTime > 1f)
            StartCoroutine(BlastWithDelay(enhanced, (castTime - 1f) * 0.5f));
        else
            Blast(enhanced);
    }

    private IEnumerator BlastWithDelay(bool enhanced, float delay)
    {
        yield return new WaitForSeconds(delay);
        Blast(enhanced);
    }

    private void Blast(bool enhanced)
    {
        float radius = baseRadius * (enhanced ? enhancedRadiusMultiplier : 1f);
        float damage = stat.attack * (enhanced ? enhancedDamageMultiplier : 1f);

        Vector2 origin = stat.CenterPosition;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, targetLayer);

        foreach (var hit in hits)
        {
            LivingEntity entity = hit.GetComponent<LivingEntity>();
            entity?.OnDamage(damage);

            if (enhanced && enhancedKnockback)
                ApplyKnockback(hit, origin);
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