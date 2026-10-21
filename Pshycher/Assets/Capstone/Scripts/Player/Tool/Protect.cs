using UnityEngine;

public class Protect : MonoBehaviour
{
    [Header("기본 설정")]
    [Range(0f, 1f)] public float baseBlockDamageReduction = 0.8f;
    public float baseDuration = 2f;
    public float baseCooldown = 3f;

    [Header("강화 설정")]
    [Range(0f, 1f)] public float enhancedBlockDamageReduction = 0.95f;
    public float enhancedDuration = 3f;
    public bool enhancedReflect = true;
    [Range(0f, 1f)] public float reflectRatio = 0.3f;

    [Header("방어 대상")]
    [Tooltip("체크하면 익사 같은 지속 피해도 방어")]
    public bool blockContinuousDamage = false;

    public bool isBlocking { get; private set; } = false;

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
        if (!isBlocking) return;

        durationTimer -= Time.deltaTime;
        if (durationTimer <= 0f)
            EndBlock();
    }

    void OnDisable()
    {
        // 방어 중 도구를 바꾸면 즉시 종료하고 쿨타임 시작
        if (isBlocking) EndBlock();
    }

    public void Activate(bool enhanced)
    {
        if (isBlocking) return;
        if (toolManager.IsOnCooldown(ToolType.Protect)) return;

        currentEnhanced = enhanced;
        isBlocking = true;
        durationTimer = enhanced ? enhancedDuration : baseDuration;
    }

    public float CalculateBlockedDamage(DamageInfo info)
    {
        if (!isBlocking) return info.amount;

        if (info.type == DamageType.Continuous && !blockContinuousDamage)
            return info.amount;

        if (!info.hasSource) return info.amount;
        if (!stat.IsFromFront(info.sourcePosition)) return info.amount;

        float reduction = currentEnhanced ? enhancedBlockDamageReduction : baseBlockDamageReduction;
        float blocked = info.amount * reduction;

        if (currentEnhanced && enhancedReflect)
            Reflect(info, blocked * reflectRatio);

        return info.amount - blocked;
    }

    private void Reflect(DamageInfo info, float damage)
    {
        if (info.sourceObject == null || damage <= 0f) return;

        LivingEntity target = info.sourceObject.GetComponent<LivingEntity>();
        if (target == null || target.dead) return;

        target.OnDamage(damage);
    }

    private void EndBlock()
    {
        isBlocking = false;
        toolManager.StartCooldown(ToolType.Protect, baseCooldown);
    }
}