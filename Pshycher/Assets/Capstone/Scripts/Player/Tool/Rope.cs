using UnityEngine;

public class Rope : MonoBehaviour
{
    [Header("기본 설정")]
    public float maxLength = 5f;
    public float baseCooldown = 0.6f;
    public float swingDamageRadius = 1f;
    public LayerMask targetLayer;
    public LayerMask ropeAnchorLayer; // 줄을 걸 수 있는 오브젝트 레이어

    [Header("강화 설정")]
    public float enhancedLengthMultiplier = 1.4f;
    public float enhancedDamageMultiplier = 1.5f;

    [Header("비주얼")]
    public LineRenderer lineRenderer;

    private float cooldownTimer = 0f;
    private bool isRopeOut = false;
    private Vector2 ropeEndPoint;
    private GameObject anchoredObject;

    private PlayerStat stat;
    private ToolManager toolManager;
    private PlayerController controller;

    void Awake()
    {
        stat = GetComponentInParent<PlayerStat>();
        toolManager = GetComponentInParent<ToolManager>();
        controller = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (isRopeOut && lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, ropeEndPoint);
        }
    }

    public void Attack(bool enhanced)
    {
        float cooldown = baseCooldown * toolManager.GetCooldownMultiplier();
        if (cooldownTimer > 0f) return;

        cooldownTimer = cooldown;

        float length = maxLength * (enhanced ? enhancedLengthMultiplier : 1f);
        float damage = stat.attack * (enhanced ? enhancedDamageMultiplier : 1f);

        Vector2 direction = stat.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)stat.transform.position;

        // 레이캐스트로 전방 오브젝트 감지
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, length, targetLayer | ropeAnchorLayer);

        if (hit.collider != null)
        {
            ropeEndPoint = hit.point;
            isRopeOut = true;

            // 앵커 레이어 오브젝트라면 줄걸기 이벤트
            if (((1 << hit.collider.gameObject.layer) & ropeAnchorLayer) != 0)
            {
                AnchorRope(hit.collider.gameObject);
            }
            else
            {
                // 일반 적 타격
                LivingEntity entity = hit.collider.GetComponent<LivingEntity>();
                entity?.OnDamage(damage);
                Debug.Log($"로프 타격 - 대상: {hit.collider.name} / 데미지: {damage} / 강화: {enhanced}");
            }

            // 휘두르기 범위 추가 피해
            Collider2D[] swingHits = Physics2D.OverlapCircleAll(ropeEndPoint, swingDamageRadius, targetLayer);
            foreach (var swingHit in swingHits)
            {
                if (swingHit.GetComponent<Collider>() == hit.collider) continue;
                LivingEntity entity = swingHit.GetComponent<LivingEntity>();
                entity?.OnDamage(damage * 0.7f);
            }

            Invoke(nameof(RetractRope), 0.4f);
        }
        else
        {
            // 허공에 휘두르기
            ropeEndPoint = origin + direction * length;
            isRopeOut = true;
            Invoke(nameof(RetractRope), 0.3f);
        }

        if (lineRenderer != null)
            lineRenderer.enabled = true;
    }

    private void AnchorRope(GameObject target)
    {
        anchoredObject = target;
        IRopeAnchor anchor = target.GetComponent<IRopeAnchor>();
        anchor?.OnRopeAttached(stat.gameObject);
        Debug.Log($"로프 앵커 연결: {target.name}");
    }

    private void RetractRope()
    {
        isRopeOut = false;
        anchoredObject = null;
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector2.right * maxLength);
    }
}

// 줄을 걸 수 있는 오브젝트가 구현할 인터페이스
public interface IRopeAnchor
{
    void OnRopeAttached(GameObject player);
}