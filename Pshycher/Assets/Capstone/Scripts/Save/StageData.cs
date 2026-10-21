using UnityEngine;

public enum StageCategory
{
    Main,       // 메인 스테이지 (좌우 스크롤)
    Ability,    // 능력 해금 (상하)
    Tool        // 도구 해금 (상하)
}

[CreateAssetMenu(fileName = "Stage_1-1", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("식별")]
    public int world = 1;
    public int stage = 1;
    public StageCategory category = StageCategory.Main;

    [Header("그룹 (능력/도구 스테이지용)")]
    public string groupName;   // "블레이드", "버스터", "철고화" ...

    [Header("씬")]
    public string sceneName;

    [Header("표시 정보")]
    public string displayName;
    [TextArea(2, 4)] public string description;
    public Sprite thumbnail;

    [Header("수집요소")]
    public int collectibleCount = 0;

    [Header("해금 조건 (추가 규칙은 여기에)")]
    [Tooltip("비워두면 같은 카테고리의 직전 스테이지 클리어만 확인")]
    public StageData[] requiredClears;

    [Header("클리어 보상")]
    public ToolType[] unlockTools = new ToolType[0];
    public AbilityType[] unlockAbilities = new AbilityType[0];

    [Header("이 스테이지 안에서만 임시 사용 (튜토리얼용)")]
    public ToolType[] tempTools = new ToolType[0];
    public AbilityType[] tempAbilities = new AbilityType[0];

    public string stageId => $"{world}-{stage}";
    public bool isMainStage => category == StageCategory.Main;
    public string label => string.IsNullOrEmpty(displayName) ? stageId : displayName;
}