using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    public ToolType currentTool { get; private set; } = ToolType.None;
    public bool isCurrentToolEnhanced { get; private set; } = false;

    [Header("맨손 해제 시 돌아갈 기본 도구")]
    public ToolType defaultTool = ToolType.Blade;

    private ToolType lastEquippedTool;

    // 맨손일 때 탭하면 꺼내질 도구 (없으면 None)
    public ToolType NextToggleTool =>
        PlayerUnlocks.IsToolAvailable(lastEquippedTool)
            ? lastEquippedTool
            : PlayerUnlocks.FirstAvailableTool();

    public event Action OnToolChanged;

    private AbilityManager abilityManager;
    private PlayerAnimator playerAnimator;
    private PlayerStat stat;
    private PlayerController controller;

    private Blade blade;
    private Buster buster;
    private Rope rope;
    private Protect protect;
    private Blaster blaster;

    // 도구별 쿨타임 종료 시각 - 도구가 꺼져 있어도 시간이 흐름
    private readonly Dictionary<ToolType, float> cooldownUntil = new Dictionary<ToolType, float>();

    void Awake()
    {
        abilityManager = GetComponent<AbilityManager>();
        playerAnimator = GetComponent<PlayerAnimator>();
        stat = GetComponent<PlayerStat>();
        controller = GetComponent<PlayerController>();

        blade = GetComponentInChildren<Blade>(true);
        buster = GetComponentInChildren<Buster>(true);
        rope = GetComponentInChildren<Rope>(true);
        protect = GetComponentInChildren<Protect>(true);
        blaster = GetComponentInChildren<Blaster>(true);

        lastEquippedTool = defaultTool;
    }

    // ── 도구 전환 ─────────────────────────────
    public void SwitchTool(ToolType newTool)
    {
        if (currentTool == newTool) return;
        if (newTool != ToolType.None && !PlayerUnlocks.IsToolAvailable(newTool)) return;
        if (currentTool != ToolType.None)
            lastEquippedTool = currentTool;

        SetToolActive(currentTool, false);

        currentTool = newTool;
        isCurrentToolEnhanced = currentTool != ToolType.None
                             && abilityManager.IsToolEnhanced(currentTool);

        SetToolActive(currentTool, true);
        OnToolChanged?.Invoke();
    }

    public void ToggleTool()
    {
        if (currentTool != ToolType.None)
        {
            SwitchTool(ToolType.None);
            return;
        }

        ToolType target = NextToggleTool;
        if (target != ToolType.None)
            SwitchTool(target);
    }

    public void OnAbilityChanged(AbilityType ability)
    {
        isCurrentToolEnhanced = currentTool != ToolType.None
                             && abilityManager.IsToolEnhanced(currentTool);
    }

    // ── 공격 ──────────────────────────────────
    public void OnAttackInput()
    {
        if (stat.dead) return;
        if (controller != null && (controller.isDashing || controller.isSwimming)) return;
        if (currentTool == ToolType.None) return;

        // 쿨타임 중이면 모션도 재생하지 않음 (블레이드는 자체 입력 간격 사용)
        if (currentTool != ToolType.Blade && IsOnCooldown(currentTool)) return;

        playerAnimator.PlayAttackAnim(currentTool);

        switch (currentTool)
        {
            case ToolType.Blade: blade?.Attack(isCurrentToolEnhanced); break;
            case ToolType.Buster: buster?.Attack(isCurrentToolEnhanced); break;
            case ToolType.Rope: rope?.Attack(isCurrentToolEnhanced); break;
            case ToolType.Protect: protect?.Activate(isCurrentToolEnhanced); break;
            case ToolType.Blaster: blaster?.Attack(isCurrentToolEnhanced); break;
        }
    }

    // ── 쿨타임 ────────────────────────────────
    public bool IsOnCooldown(ToolType tool)
    {
        return cooldownUntil.TryGetValue(tool, out float until) && Time.time < until;
    }

    // UI 표시용 남은 시간
    public float GetRemainingCooldown(ToolType tool)
    {
        return cooldownUntil.TryGetValue(tool, out float until)
            ? Mathf.Max(0f, until - Time.time)
            : 0f;
    }

    // 기본 쿨타임을 넘기면 능력 배율까지 적용
    public void StartCooldown(ToolType tool, float baseCooldown)
    {
        cooldownUntil[tool] = Time.time + baseCooldown * GetCooldownMultiplier();
    }

    // ── 지면진동 배율 ─────────────────────────
    public float GetCastTimeMultiplier()
    {
        if (abilityManager.isAbilityActive &&
            abilityManager.currentAbility == AbilityType.GroundShock)
            return 1.5f;
        return 1f;
    }

    public float GetCooldownMultiplier()
    {
        if (abilityManager.isAbilityActive &&
            abilityManager.currentAbility == AbilityType.GroundShock)
            return 1.5f;
        return 1f;
    }

    private void SetToolActive(ToolType tool, bool active)
    {
        switch (tool)
        {
            case ToolType.None: break;
            case ToolType.Blade: blade?.gameObject.SetActive(active); break;
            case ToolType.Buster: buster?.gameObject.SetActive(active); break;
            case ToolType.Rope: rope?.gameObject.SetActive(active); break;
            case ToolType.Protect: protect?.gameObject.SetActive(active); break;
            case ToolType.Blaster: blaster?.gameObject.SetActive(active); break;
        }
    }
}