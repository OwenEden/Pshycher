using System;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public AbilityType currentAbility { get; private set; } = AbilityType.None;
    public bool isAbilityActive { get; private set; } = false;

    public event Action OnAbilityStateChanged;

    [Header("점프 설정")]
    public AbilityType tripleJumpAbility = AbilityType.None;

    // ── 능력별 색 ─────────────────────────────
    [Serializable]
    public class AbilityTint
    {
        public AbilityType ability;
        public Color32 color = new Color32(255, 255, 255, 255);
    }

    [Header("능력별 색 (목록에 없는 능력은 원래 색)")]
    public AbilityTint[] tints =
    {
        new AbilityTint { ability = AbilityType.IronHardening, color = new Color32( 60,  60,  60, 255) },
        new AbilityTint { ability = AbilityType.FluidBody,     color = new Color32( 85, 220, 240, 255) },
        new AbilityTint { ability = AbilityType.WindForm,      color = new Color32(245, 255, 228, 255) },
        new AbilityTint { ability = AbilityType.Plasma,        color = new Color32(158,  66, 229, 255) },
        new AbilityTint { ability = AbilityType.FlameCharge,   color = new Color32(220,  26,  58, 255) },
    };

    [Header("색을 바꿀 대상 (비우면 플레이어 본체 SpriteRenderer)")]
    public SpriteRenderer[] tintTargets;
    public float tintFadeDuration = 0.15f;

    private Color[] tintBaseColors;
    private Color currentTint = Color.white;
    private Color targetTint = Color.white;
    private Color fadeFrom = Color.white;
    private float fadeT = 1f;
    // ──────────────────────────────────────────

    private PlayerStat stat;
    private PlayerAnimator playerAnimator;
    private ToolManager toolManager;
    private PlayerController controller;

    void Awake()
    {
        stat = GetComponent<PlayerStat>();
        playerAnimator = GetComponent<PlayerAnimator>();
        toolManager = GetComponent<ToolManager>();
        controller = GetComponent<PlayerController>();

        InitTint();
    }

    void Update()
    {
        UpdateTintFade();
    }

    // ── 능력 전환 ─────────────────────────────
    public void SwitchAbility(AbilityType newAbility)
    {
        if (currentAbility == newAbility) return;

        if (isAbilityActive)
            DeactivateAbility();

        currentAbility = newAbility;
        NotifyStateChanged();
    }

    // 버튼 짧게 탭
    public void ToggleAbility()
    {
        if (currentAbility == AbilityType.None) return;

        // 끄는 건 항상 허용, 켜는 건 사용 가능할 때만
        if (!isAbilityActive && !PlayerUnlocks.IsAbilityAvailable(currentAbility)) return;

        if (isAbilityActive) DeactivateAbility();
        else ActivateAbility();
    }

    // 휠에서 능력 선택 - 전환과 동시에 활성화
    public void SelectAndActivate(AbilityType newAbility)
    {
        if (newAbility == AbilityType.None) return;
        if (!PlayerUnlocks.IsAbilityAvailable(newAbility)) return;

        if (currentAbility == newAbility)
        {
            if (!isAbilityActive) ActivateAbility();
            return;
        }

        if (isAbilityActive) DeactivateAbility();

        currentAbility = newAbility;
        ActivateAbility();
    }

    private void ActivateAbility()
    {
        isAbilityActive = true;
        stat.ApplyStatModifiers(currentAbility);
        toolManager.OnAbilityChanged(currentAbility);
        playerAnimator.PlayAbilityAnim(currentAbility);

        if (tripleJumpAbility != AbilityType.None && currentAbility == tripleJumpAbility)
            controller.SetMaxJumpCount(3);

        NotifyStateChanged();
    }

    private void DeactivateAbility()
    {
        isAbilityActive = false;
        stat.ApplyStatModifiers(AbilityType.None);
        toolManager.OnAbilityChanged(AbilityType.None);
        controller.SetMaxJumpCount(2);

        NotifyStateChanged();
    }

    // 상태가 바뀔 때마다 이벤트 발송 + 색 갱신
    private void NotifyStateChanged()
    {
        SetTintTarget(GetTintForCurrentState());
        OnAbilityStateChanged?.Invoke();
    }

    // ── 도구 강화 판정 ────────────────────────
    public bool IsToolEnhanced(ToolType tool)
    {
        if (!isAbilityActive) return false;

        switch (currentAbility)
        {
            case AbilityType.IronHardening: return tool == ToolType.Blade || tool == ToolType.Buster;
            case AbilityType.FluidBody: return tool == ToolType.Rope || tool == ToolType.Blaster;
            case AbilityType.WindForm: return tool == ToolType.Buster || tool == ToolType.Blaster;
            case AbilityType.Plasma: return tool == ToolType.Blade || tool == ToolType.Buster;
            case AbilityType.FlameCharge: return tool == ToolType.Blade || tool == ToolType.Protect;
            case AbilityType.AquaStep: return tool == ToolType.Protect;
            case AbilityType.GroundShock: return tool == ToolType.Buster || tool == ToolType.Blaster;
            case AbilityType.AirSlash: return tool == ToolType.Blade || tool == ToolType.Rope;
            default: return false;
        }
    }

    // ── 능력별 색 ─────────────────────────────
    private void InitTint()
    {
        if (tintTargets == null || tintTargets.Length == 0)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
            tintTargets = sr != null ? new[] { sr } : new SpriteRenderer[0];
        }

        // 원래 색 기억 (보통 흰색)
        tintBaseColors = new Color[tintTargets.Length];
        for (int i = 0; i < tintTargets.Length; i++)
            tintBaseColors[i] = tintTargets[i] != null ? tintTargets[i].color : Color.white;

        // 시작할 때는 페이드 없이 바로 적용
        targetTint = GetTintForCurrentState();
        currentTint = targetTint;
        fadeT = 1f;
        ApplyTint();
    }

    private Color GetTintForCurrentState()
    {
        if (!isAbilityActive) return Color.white;

        foreach (var t in tints)
            if (t.ability == currentAbility)
                return t.color;

        return Color.white;
    }

    private void SetTintTarget(Color next)
    {
        if (next == targetTint) return;

        fadeFrom = currentTint;
        targetTint = next;
        fadeT = 0f;
    }

    private void UpdateTintFade()
    {
        if (fadeT >= 1f) return;

        fadeT = tintFadeDuration > 0f
            ? Mathf.Min(1f, fadeT + Time.deltaTime / tintFadeDuration)
            : 1f;

        currentTint = Color.Lerp(fadeFrom, targetTint, fadeT);
        ApplyTint();
    }

    private void ApplyTint()
    {
        for (int i = 0; i < tintTargets.Length; i++)
        {
            if (tintTargets[i] == null) continue;
            tintTargets[i].color = tintBaseColors[i] * currentTint;
        }
    }
}