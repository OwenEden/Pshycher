using UnityEngine;

public enum RecordUnlockType
{
    Collectible,   // 해당 스테이지의 수집요소를 얻고 클리어
    StageClear,    // 해당 스테이지 클리어
    NotYet         // 해금 방식 미구현 (NPC 대화 등) — 항상 잠김
}

[CreateAssetMenu(fileName = "Record_", menuName = "Game/Record Data")]
public class RecordData : ScriptableObject
{
    [Header("분류")]
    public RecordCategoryData category;
    [Tooltip("카테고리 안에서의 순서")]
    public int order;

    [Header("표시")]
    public string listLabel = "정규1";
    public string title = "정규 임무 - 1";
    [TextArea(6, 15)] public string body;

    [Header("해금 조건")]
    public RecordUnlockType unlockType = RecordUnlockType.Collectible;
    public StageData stage;
    [Tooltip("스테이지 씬의 Collectible에 적은 ID와 같아야 함")]
    public string collectibleId;

    public bool IsUnlocked(SaveData save)
    {
        if (save == null) return true;   // 슬롯 없이 테스트할 때는 전부 표시
        if (unlockType == RecordUnlockType.NotYet || stage == null) return false;

        StageRecord record = save.GetRecord(stage.stageId);
        if (record == null) return false;

        switch (unlockType)
        {
            case RecordUnlockType.Collectible: return record.HasCollected(collectibleId);
            case RecordUnlockType.StageClear: return record.cleared;
            default: return false;
        }
    }
}