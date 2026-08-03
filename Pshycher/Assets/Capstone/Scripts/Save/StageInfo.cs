using System.Collections.Generic;
using UnityEngine;

public class StageInfo : MonoBehaviour
{
    public static StageInfo Current { get; private set; }

    [Header("스테이지 정의")]
    public int world = 1;
    public int stage = 1;
    public bool isMainStage = true;

    [Header("클리어 시 해금 (서브 스테이지용)")]
    public ToolType unlockTool = ToolType.None;
    public AbilityType unlockAbility = AbilityType.None;

    public string stageId => $"{world}-{stage}";

    // 이번 플레이에서 획득한 수집요소
    private readonly HashSet<string> collectedThisRun = new HashSet<string>();
    private List<string> allCollectibleIds = new List<string>();

    private void Awake()
    {
        Current = this;
    }

    private void Start()
    {
        // 씬 안의 수집요소 전체 ID 수집 (비활성 포함)
        allCollectibleIds.Clear();
        foreach (var c in FindObjectsByType<Collectible>(FindObjectsInactive.Include))
        {
            if (!string.IsNullOrEmpty(c.collectibleId))
                allCollectibleIds.Add(c.collectibleId);
        }

        StageTimer.Instance?.StartTimer();
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    public void OnCollect(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        collectedThisRun.Add(id);
        Debug.Log($"수집요소 획득: {id} ({collectedThisRun.Count}/{allCollectibleIds.Count})");
    }

    public bool WasCollectedBefore(string id)
    {
        var save = SaveManager.Instance;
        if (save == null || !save.hasActiveSlot) return false;

        StageRecord record = save.current.GetRecord(stageId);
        return record != null && record.HasCollected(id);
    }

    // EndPoint에서 호출
    public StageClearResult ClearStage(float clearTime)
    {
        var result = new StageClearResult
        {
            stageId = stageId,
            clearTime = clearTime,
            totalCollectibles = allCollectibleIds.Count
        };

        var save = SaveManager.Instance;
        if (save == null || !save.hasActiveSlot)
        {
            Debug.LogWarning("활성 슬롯이 없어 기록이 저장되지 않습니다.");
            result.collectedCount = collectedThisRun.Count;
            result.bestClearTime = clearTime;
            return result;
        }

        StageRecord record = save.current.GetOrCreateRecord(stageId, world, stage, isMainStage);

        // 베스트 타임 갱신
        if (!record.cleared || clearTime < record.bestClearTime)
        {
            record.bestClearTime = clearTime;
            result.isNewRecord = record.cleared;   // 첫 클리어는 신기록 표시 안 함
        }

        record.cleared = true;
        record.lastClearTime = clearTime;

        // 수집요소 누적 기록
        foreach (string id in collectedThisRun)
            if (!record.collectedIds.Contains(id))
                record.collectedIds.Add(id);

        // 해금 처리
        if (save.current.UnlockTool(unlockTool))
            result.unlockedTool = unlockTool;
        if (save.current.UnlockAbility(unlockAbility))
            result.unlockedAbility = unlockAbility;

        result.collectedCount = record.collectedIds.Count;
        result.bestClearTime = record.bestClearTime;

        save.Save();   // 자동 저장
        return result;
    }
}

public class StageClearResult
{
    public string stageId;
    public float clearTime;
    public float bestClearTime;
    public bool isNewRecord;
    public int collectedCount;
    public int totalCollectibles;
    public ToolType unlockedTool = ToolType.None;
    public AbilityType unlockedAbility = AbilityType.None;
}