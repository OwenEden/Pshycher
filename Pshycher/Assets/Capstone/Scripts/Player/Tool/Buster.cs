using System.Collections;
using UnityEngine;

public class Buster : MonoBehaviour
{
    [Header("기본 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float baseCooldown = 0.4f;
    [Tooltip("버튼을 누르고 실제로 발사되기까지의 시간 (지면진동 시 배율 적용)")]
    public float startupDelay = 0.15f;

    [Header("강화 설정")]
    public float enhancedDamageMultiplier = 1.5f;
    public float enhancedSpeedMultiplier = 1.4f;
    public int enhancedPierceCount = 2;

    public bool isWindingUp { get; private set; }

    private PlayerStat stat;
    private ToolManager toolManager;
    private PlayerAnimator playerAnimator;

    void Awake()
    {
        stat = GetComponentInParent<PlayerStat>();
        toolManager = GetComponentInParent<ToolManager>();
        playerAnimator = GetComponentInParent<PlayerAnimator>();
    }

    void OnDisable()
    {
        // 선딜 중 도구를 바꾸면 코루틴이 멈추므로 상태를 풀어줌
        isWindingUp = false;
    }

    public void Attack(bool enhanced)
    {
        if (isWindingUp) return;
        if (toolManager.IsOnCooldown(ToolType.Buster)) return;
        if (projectilePrefab == null || firePoint == null) return;

        toolManager.StartCooldown(ToolType.Buster, baseCooldown);

        float delay = startupDelay * toolManager.GetCastTimeMultiplier();

        // 누르는 순간엔 겨눈 자세만 (반동은 발사 순간)
        playerAnimator?.OnBusterAim(delay);

        if (delay > 0f)
            StartCoroutine(FireAfter(enhanced, delay));
        else
            Fire(enhanced);
    }

    private IEnumerator FireAfter(bool enhanced, float delay)
    {
        isWindingUp = true;
        yield return new WaitForSeconds(delay);
        isWindingUp = false;

        if (stat.dead) yield break;
        Fire(enhanced);
    }

    private void Fire(bool enhanced)
    {
        // 발사 순간 바라보는 방향으로 나감
        Vector2 direction = stat.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        GameObject obj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        BusterProjectile projectile = obj.GetComponent<BusterProjectile>();

        if (projectile != null)
        {
            float damage = stat.attack * (enhanced ? enhancedDamageMultiplier : 1f);
            int pierce = enhanced ? enhancedPierceCount : 0;
            float speedMult = enhanced ? enhancedSpeedMultiplier : 1f;

            projectile.Init(direction, damage, pierce, speedMult, null);
        }

        playerAnimator?.OnBusterFired();
    }
}