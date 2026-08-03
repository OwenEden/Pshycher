using UnityEngine;

[CreateAssetMenu(fileName = "Stage_1-1", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("식별")]
    public int world = 1;
    public int stage = 1;
    public bool isMainStage = true;

    [Header("씬")]
    public string sceneName;

    [Header("표시 정보")]
    public string displayName;
    [TextArea(2, 4)] public string description;
    public Sprite thumbnail;

    [Header("해금 조건 (추가 규칙은 여기에)")]
    [Tooltip("비워두면 직전 스테이지 클리어만 확인")]
    public StageData[] requiredClears;

    [Header("클리어 보상")]
    public ToolType unlockTool = ToolType.None;
    public AbilityType unlockAbility = AbilityType.None;
    
    [Header("수집요소")]
    public int collectibleCount = 0;   // 이 스테이지의 수집요소 총 개수

    public string stageId => $"{world}-{stage}";
    public string label => string.IsNullOrEmpty(displayName) ? stageId : displayName;
}