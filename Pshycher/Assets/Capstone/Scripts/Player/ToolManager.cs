using UnityEngine;

public class ToolManager : MonoBehaviour
{
    public ToolType currentTool { get; private set; } = ToolType.None;
    public bool isCurrentToolEnhanced { get; private set; } = false;

    private AbilityManager abilityManager;
    private PlayerAnimator playerAnimator;
    private PlayerStat stat;

    private Blade blade;
    private Buster buster;
    private Rope rope;
    private Protect protect;
    private Blaster blaster;

    void Awake()
    {
        abilityManager = GetComponent<AbilityManager>();
        playerAnimator = GetComponent<PlayerAnimator>();
        stat = GetComponent<PlayerStat>();

        blade = GetComponent<Blade>();
        buster = GetComponent<Buster>();
        rope = GetComponent<Rope>();
        protect = GetComponent<Protect>();
        blaster = GetComponent<Blaster>();
    }

    public void SwitchTool(ToolType newTool)
    {
        if (currentTool == newTool) return;

        SetToolActive(currentTool, false);

        currentTool = newTool;
        isCurrentToolEnhanced = abilityManager.IsToolEnhanced(currentTool);

        SetToolActive(currentTool, true);

        Debug.Log($"도구 전환: {currentTool} / 강화: {isCurrentToolEnhanced}");
    }

    public void OnAbilityChanged(AbilityType ability)
    {
        // None 상태는 강화 대상 없음
        isCurrentToolEnhanced = currentTool != ToolType.None && abilityManager.IsToolEnhanced(currentTool);
        Debug.Log($"능력 변경 반영 - 현재 도구 [{currentTool}] 강화 여부: {isCurrentToolEnhanced}");
    }

    public void OnAttackInput()
    {
        if (stat.dead) return;

        // None 상태면 공격 불가
        if (currentTool == ToolType.None)
        {
            Debug.Log("도구 없음 - 공격 불가");
            return;
        }

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
            case ToolType.None: break; // 아무 처리 없음
            case ToolType.Blade: blade?.gameObject.SetActive(active); break;
            case ToolType.Buster: buster?.gameObject.SetActive(active); break;
            case ToolType.Rope: rope?.gameObject.SetActive(active); break;
            case ToolType.Protect: protect?.gameObject.SetActive(active); break;
            case ToolType.Blaster: blaster?.gameObject.SetActive(active); break;
        }
    }
}