using UnityEngine;

public class BusterProjectile : MonoBehaviour
{
    [Header("기본 설정")]
    public float baseSpeed = 12f;
    public float maxLifetime = 5f;

    private Vector2 direction;
    private float damage;
    private int pierceCount;
    private float speedMultiplier;
    private LayerMask targetLayer;

    private Rigidbody2D rb;
    private int pierceRemaining;
    private float lifetimeTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir, float dmg, int pierce, float speedMult, LayerMask? targetLayer)
    {
        direction = dir;
        damage = dmg;
        pierceCount = pierce;
        pierceRemaining = pierce;
        speedMultiplier = speedMult;

        if (targetLayer.HasValue)
            this.targetLayer = targetLayer.Value;

        // 발사 방향으로 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (rb != null)
            rb.linearVelocity = direction * baseSpeed * speedMultiplier;
    }

    void Update()
    {
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= maxLifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 지형/벽에 충돌 시 즉시 제거
        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        LivingEntity entity = other.GetComponent<LivingEntity>();
        if (entity != null)
        {
            entity.OnDamage(damage);
            Debug.Log($"버스터 탄환 적중 - 대상: {other.name} / 데미지: {damage}");

            if (pierceRemaining <= 0)
                Destroy(gameObject);
            else
                pierceRemaining--;
        }
    }
}