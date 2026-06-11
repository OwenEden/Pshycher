using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public AbilityType currentAbility { get; private set; } = AbilityType.None;
    public bool isAbilityActive { get; private set; } = false;

    // 3단 점프를 부여할 능력 - 미정이므로 Inspector에서 설정 가능하도록 노출
    [Header("점프 설정")]
    public AbilityType tripleJumpAbility = AbilityType.None;

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
    }

    public void SwitchAbility(AbilityType newAbility)
    {
        if (currentAbility == newAbility) return;

        if (isAbilityActive)
            DeactivateAbility();

        currentAbility = newAbility;
        Debug.Log($"능력 전환: {currentAbility}");
    }

    public void ToggleAbility()
    {
        if (currentAbility == AbilityType.None) return;

        if (isAbilityActive)
            DeactivateAbility();
        else
            ActivateAbility();
    }

    private void ActivateAbility()
    {
        isAbilityActive = true;
        stat.ApplyStatModifiers(currentAbility);
        toolManager.OnAbilityChanged(currentAbility);
        playerAnimator.PlayAbilityAnim(currentAbility);

        // 3단 점프 능력이면 최대 점프 횟수 3으로 설정
        if (tripleJumpAbility != AbilityType.None && currentAbility == tripleJumpAbility)
            controller.SetMaxJumpCount(3);

        Debug.Log($"능력 활성화: {currentAbility}");
    }

    private void DeactivateAbility()
    {
        isAbilityActive = false;
        stat.ApplyStatModifiers(AbilityType.None);
        toolManager.OnAbilityChanged(AbilityType.None);

        // 능력 해제 시 기본 2단 점프로 복귀
        controller.SetMaxJumpCount(2);

        Debug.Log($"능력 비활성화: {currentAbility}");
    }

    public bool IsToolEnhanced(ToolType tool)
    {
        if (!isAbilityActive) return false;

        switch (currentAbility)
        {
            case AbilityType.IronHardening:
                return tool == ToolType.Blade || tool == ToolType.Buster;
            case AbilityType.FluidBody:
                return tool == ToolType.Rope || tool == ToolType.Blaster;
            case AbilityType.WindForm:
                return tool == ToolType.Buster || tool == ToolType.Blaster;
            case AbilityType.Plasma:
                return tool == ToolType.Blade || tool == ToolType.Buster;
            case AbilityType.FlameCharge:
                return tool == ToolType.Blade || tool == ToolType.Protect;
            case AbilityType.AquaStep:
                return tool == ToolType.Protect;
            case AbilityType.GroundShock:
                return tool == ToolType.Buster || tool == ToolType.Blaster;
            case AbilityType.AirSlash:
                return tool == ToolType.Blade || tool == ToolType.Rope;
            default:
                return false;
        }
    }
}