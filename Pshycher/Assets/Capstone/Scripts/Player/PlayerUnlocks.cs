using System;

public static class PlayerUnlocks
{
    // 세이브 슬롯 없이 스테이지를 바로 실행한 경우(에디터 테스트) 전부 사용 가능
    private static bool NoSave =>
        SaveManager.Instance == null || !SaveManager.Instance.hasActiveSlot;

    private static StageData CurrentStage =>
        StageInfo.Current != null ? StageInfo.Current.stageData : null;

    // ── 도구 ──────────────────────────────────
    public static bool IsToolAvailable(ToolType tool)
    {
        if (tool == ToolType.None) return false;
        if (NoSave) return true;
        if (SaveManager.Instance.current.IsToolUnlocked(tool)) return true;

        var stage = CurrentStage;
        return stage != null && stage.tempTools != null
            && Array.IndexOf(stage.tempTools, tool) >= 0;
    }

    public static ToolType FirstAvailableTool()
    {
        foreach (ToolType t in Enum.GetValues(typeof(ToolType)))
            if (IsToolAvailable(t)) return t;
        return ToolType.None;
    }

    // ── 능력 ──────────────────────────────────
    public static bool IsAbilityAvailable(AbilityType ability)
    {
        if (ability == AbilityType.None) return false;
        if (NoSave) return true;
        if (SaveManager.Instance.current.IsAbilityUnlocked(ability)) return true;

        var stage = CurrentStage;
        return stage != null && stage.tempAbilities != null
            && Array.IndexOf(stage.tempAbilities, ability) >= 0;
    }
}