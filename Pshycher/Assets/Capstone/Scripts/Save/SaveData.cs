using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string lastSavedTime = "";      // ISO 8601 (UTC)
    public float totalPlayTime = 0f;

    public List<StageRecord> stageRecords = new List<StageRecord>();
    public List<ToolType> unlockedTools = new List<ToolType>();
    public List<AbilityType> unlockedAbilities = new List<AbilityType>();

    // 체크포인트 이어하기용 예약 필드 (현재 미사용)
    public string checkpointStageId = "";
    public float checkpointX = 0f;
    public float checkpointY = 0f;

    // ── 스테이지 기록 ──────────────────────────
    public StageRecord GetRecord(string stageId)
    {
        return stageRecords.Find(r => r.stageId == stageId);
    }

    public StageRecord GetOrCreateRecord(string stageId, int world, int stage, bool isMain)
    {
        StageRecord record = GetRecord(stageId);
        if (record != null) return record;

        record = new StageRecord
        {
            stageId = stageId,
            world = world,
            stage = stage,
            isMainStage = isMain
        };
        stageRecords.Add(record);
        return record;
    }

    // ── 메인 스테이지 진행도 (클리어한 것 중 최고 번호) ──
    public string GetMainProgressLabel()
    {
        StageRecord best = null;

        foreach (var r in stageRecords)
        {
            if (!r.isMainStage || !r.cleared) continue;
            if (best == null || r.world > best.world ||
               (r.world == best.world && r.stage > best.stage))
                best = r;
        }

        return best == null ? "-" : $"{best.world}-{best.stage}";
    }

    // ── 해금 ──────────────────────────────────
    public bool IsToolUnlocked(ToolType tool) => unlockedTools.Contains(tool);
    public bool IsAbilityUnlocked(AbilityType ability) => unlockedAbilities.Contains(ability);

    public bool UnlockTool(ToolType tool)
    {
        if (tool == ToolType.None || unlockedTools.Contains(tool)) return false;
        unlockedTools.Add(tool);
        return true;
    }

    public bool UnlockAbility(AbilityType ability)
    {
        if (ability == AbilityType.None || unlockedAbilities.Contains(ability)) return false;
        unlockedAbilities.Add(ability);
        return true;
    }

    public DateTime GetLastSavedLocal()
    {
        if (DateTime.TryParse(lastSavedTime, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out DateTime utc))
            return utc.ToLocalTime();
        return DateTime.MinValue;
    }
}

[Serializable]
public class StageRecord
{
    public string stageId;          // "1-4"
    public int world;
    public int stage;
    public bool isMainStage = true;

    public bool cleared = false;
    public float lastClearTime = 0f;
    public float bestClearTime = 0f;

    public List<string> collectedIds = new List<string>();

    public bool HasCollected(string id) => collectedIds.Contains(id);
}

// Title Scene 슬롯 목록 표시용
public class SlotSummary
{
    public int slotIndex;
    public bool exists;
    public DateTime lastSaved;
    public string mainProgress = "-";
    public List<ToolType> unlockedTools = new List<ToolType>();
    public List<AbilityType> unlockedAbilities = new List<AbilityType>();
}