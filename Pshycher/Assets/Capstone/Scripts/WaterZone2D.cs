using UnityEngine;

public class WaterZone2D : MonoBehaviour
{
    [Header("부력 — 가라앉는 속도 (초당 칸)")]
    public float heavySinkSpeed = 4f;      // 철고화
    public float normalSinkSpeed = 1.5f;   // 그 외
    [Tooltip("0 = 제자리, 음수 = 떠오름")]
    public float floatSinkSpeed = 0f;      // 수액화

    [Header("부력 — 목표 속도로 수렴하는 세기")]
    [Tooltip("위로 올라가는 움직임을 멈추게 하는 힘. 물속 점프·헤엄 높이를 정함")]
    public float riseDeceleration = 18f;
    [Tooltip("빠르게 떨어지는 움직임을 늦추는 힘. 입수 시 감속감을 정함")]
    public float fallDrag = 25f;

    [Header("능력 지정")]
    public AbilityType heavyAbility = AbilityType.IronHardening;
    public AbilityType floatAbility = AbilityType.FluidBody;

    [Header("수영")]
    public float swimVerticalSpeed = 3f;
    [Tooltip("수영 중 점프 버튼 추진력 = 기본 점프력 × 이 값")]
    public float swimJumpMultiplier = 0.6f;
    public float maxSwimRiseSpeed = 5f;

    [Header("자유 수영 (상하 이동)")]
    public AbilityType freeSwimAbility = AbilityType.FluidBody;

    [Header("체력 감소")]
    public bool drainHealth = true;
    public float drainAmount = 5f;
    public float drainInterval = 1f;
    public float drainStartDelay = 1f;

    [Tooltip("이 능력들이 켜져 있으면 체력이 감소하지 않음")]
    public AbilityType[] immuneAbilities = { AbilityType.FluidBody };

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerController>()?.EnterWaterZone(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerController>()?.ExitWaterZone(this);
    }

    public bool IsHeavy(AbilityManager ability)
    {
        return ability != null && ability.isAbilityActive
            && ability.currentAbility == heavyAbility;
    }

    // 현재 능력에 맞는 가라앉는 속도 (양수 = 아래로)
    public float GetSinkSpeed(AbilityManager ability)
    {
        if (ability != null && ability.isAbilityActive)
        {
            if (ability.currentAbility == heavyAbility) return heavySinkSpeed;
            if (ability.currentAbility == floatAbility) return floatSinkSpeed;
        }
        return normalSinkSpeed;
    }

    public bool CanSwimFreely(AbilityManager ability)
    {
        return ability != null && ability.isAbilityActive
            && ability.currentAbility == freeSwimAbility;
    }

    public bool IsDrainImmune(AbilityManager ability)
    {
        if (!drainHealth) return true;
        if (ability == null || !ability.isAbilityActive) return false;

        foreach (var a in immuneAbilities)
            if (a == ability.currentAbility) return true;

        return false;
    }
}