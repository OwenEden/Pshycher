using UnityEngine;

public enum DamageType
{
    Direct,       // 일반 공격
    Continuous    // 지속 피해 (익사, 독 등)
}

public enum DeathFallType
{
    Backward = 0,
    Forward = 1
}

public struct DamageInfo
{
    public float amount;
    public DamageType type;
    public bool ignoreDefense;
    public bool hasSource;
    public Vector2 sourcePosition;   // 방향 판정용
    public GameObject sourceObject;  // 반사 대상 (없을 수 있음)

    // 공격자 오브젝트를 아는 경우 (권장)
    public static DamageInfo Hit(float amount, GameObject source) => new DamageInfo
    {
        amount = amount,
        type = DamageType.Direct,
        hasSource = source != null,
        sourcePosition = source != null ? (Vector2)source.transform.position : Vector2.zero,
        sourceObject = source
    };

    // 위치만 아는 경우
    public static DamageInfo Hit(float amount, Vector2 sourcePosition) => new DamageInfo
    {
        amount = amount,
        type = DamageType.Direct,
        hasSource = true,
        sourcePosition = sourcePosition
    };

    public static DamageInfo Plain(float amount) => new DamageInfo
    {
        amount = amount,
        type = DamageType.Direct
    };

    public static DamageInfo Continuous(float amount, bool ignoreDefense = true) => new DamageInfo
    {
        amount = amount,
        type = DamageType.Continuous,
        ignoreDefense = ignoreDefense
    };
}