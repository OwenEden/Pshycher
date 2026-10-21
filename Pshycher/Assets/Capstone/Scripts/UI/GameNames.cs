public static class GameNames
{
    public static string GetToolName(ToolType tool)
    {
        switch (tool)
        {
            case ToolType.Blade: return "블레이드";
            case ToolType.Buster: return "버스터";
            case ToolType.Rope: return "로프";
            case ToolType.Protect: return "프로텍트";
            case ToolType.Blaster: return "블래스터";
            default: return "없음";
        }
    }

    public static string GetAbilityName(AbilityType ability)
    {
        switch (ability)
        {
            case AbilityType.IronHardening: return "철고화";
            case AbilityType.FluidBody: return "수액화";
            case AbilityType.WindForm: return "풍기화";
            case AbilityType.Plasma: return "플라즈마";
            case AbilityType.FlameCharge: return "화염돌진";
            case AbilityType.AquaStep: return "수륙발판";
            case AbilityType.GroundShock: return "지면진동";
            case AbilityType.AirSlash: return "공기참격";
            default: return "없음";
        }
    }
}