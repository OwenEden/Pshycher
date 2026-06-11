using UnityEngine;

public class Buster : MonoBehaviour
{
    [Header("기본 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float baseCooldown = 0.4f;

    [Header("강화 설정")]
    public float enhancedDamageMultiplier = 1.5f;
    public float enhancedSpeedMultiplier = 1.4f;
    public int enhancedPierceCount = 2; // 강화 시 관통 횟수

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
        if (projectilePrefab == null || firePoint == null) return;

        cooldownTimer = cooldown;

        // 시전시간이 있는 경우 코루틴으로 처리
        if (castTime > 1f)
            StartCoroutine(FireWithDelay(enhanced, (castTime - 1f) * 0.3f));
        else
            Fire(enhanced);
    }

    private System.Collections.IEnumerator FireWithDelay(bool enhanced, float delay)
    {
        Debug.Log($"버스터 시전 중... ({delay:F2}초)");
        yield return new WaitForSeconds(delay);
        Fire(enhanced);
    }

    private void Fire(bool enhanced)
    {
        Vector2 direction = stat.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        GameObject obj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        BusterProjectile projectile = obj.GetComponent<BusterProjectile>();

        if (projectile != null)
        {
            float damage = stat.attack * (enhanced ? enhancedDamageMultiplier : 1f);
            int pierce = enhanced ? enhancedPierceCount : 0;
            float speedMult = enhanced ? enhancedSpeedMultiplier : 1f;

            projectile.Init(direction, damage, pierce, speedMult, targetLayer: null);
        }

        Debug.Log($"버스터 발사 - 방향: {direction} / 강화: {enhanced}");
    }
}