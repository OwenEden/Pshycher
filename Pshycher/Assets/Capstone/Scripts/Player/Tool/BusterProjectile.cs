using UnityEngine;

public class BusterProjectile : MonoBehaviour
{
    [Header("기본 설정")]
    public float baseSpeed = 12f;
    public float maxLifetime = 5f;

    [Header("충돌 레이어")]
    [Tooltip("피해를 주는 대상 (적, 파괴 가능 오브젝트)")]
    public LayerMask targetLayer;
    [Tooltip("부딪히면 사라지는 지형. 여기 없는 레이어는 통과")]
    public LayerMask obstacleLayer;

    private Vector2 direction;
    private float damage;
    private int pierceRemaining;
    private float lifetimeTimer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir, float dmg, int pierce, float speedMult, LayerMask? overrideTargetLayer = null)
    {
        direction = dir;
        damage = dmg;
        pierceRemaining = pierce;

        if (overrideTargetLayer.HasValue)
            targetLayer = overrideTargetLayer.Value;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (rb != null)
            rb.linearVelocity = direction * baseSpeed * speedMult;
    }

    void Update()
    {
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= maxLifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        int layerBit = 1 << other.gameObject.layer;

        // 1. 피해 대상 - 관통 처리
        if ((layerBit & targetLayer) != 0)
        {
            LivingEntity entity = other.GetComponent<LivingEntity>();
            if (entity == null || entity.dead) return;

            entity.OnDamage(damage);

            if (pierceRemaining <= 0)
                Destroy(gameObject);
            else
                pierceRemaining--;

            return;
        }

        // 2. 지형 - 마스크에 있는 레이어만 막힘
        if ((layerBit & obstacleLayer) != 0)
            Destroy(gameObject);
    }
}