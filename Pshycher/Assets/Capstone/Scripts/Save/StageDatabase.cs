using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Game/Stage Database")]
public class StageDatabase : ScriptableObject
{
    [Header("전체 스테이지 (순서대로)")]
    public List<StageData> stages = new List<StageData>();

    public StageData Find(string stageId) => stages.Find(s => s.stageId == stageId);

    // 메인/서브 필터링
    public List<StageData> GetStages(bool mainOnly)
    {
        var list = new List<StageData>();
        foreach (var s in stages)
        {
            if (mainOnly && !s.isMainStage) continue;
            list.Add(s);
        }
        return list;
    }

    // ── 해금 판정 (규칙 추가는 여기에) ──────────
    public bool IsUnlocked(StageData target, SaveData save)
    {
        if (target == null || save == null) return false;

        // 규칙 1: 명시된 선행 스테이지가 있으면 그것만 확인
        if (target.requiredClears != null && target.requiredClears.Length > 0)
        {
            foreach (var req in target.requiredClears)
            {
                if (req == null) continue;
                var record = save.GetRecord(req.stageId);
                if (record == null || !record.cleared) return false;
            }
            return true;
        }

        // 규칙 2: 기본 — 같은 분류(메인/서브)의 직전 스테이지 클리어
        StageData prev = GetPrevious(target);
        if (prev == null) return true;   // 첫 스테이지

        var prevRecord = save.GetRecord(prev.stageId);
        return prevRecord != null && prevRecord.cleared;
    }

    private StageData GetPrevious(StageData target)
    {
        StageData prev = null;

        foreach (var s in stages)
        {
            if (s == null || s == target) continue;
            if (s.isMainStage != target.isMainStage) continue;

            bool isBefore = s.world < target.world ||
                           (s.world == target.world && s.stage < target.stage);
            if (!isBefore) continue;

            if (prev == null || s.world > prev.world ||
               (s.world == prev.world && s.stage > prev.stage))
                prev = s;
        }

        return prev;
    }
    // 전체 수집요소 개수
    public int GetTotalCollectibles()
    {
        int total = 0;
        foreach (var s in stages)
            if (s != null) total += s.collectibleCount;
        return total;
    }

    // 획득한 수집요소 개수
    public int GetCollectedCount(SaveData save)
    {
        if (save == null) return 0;

        int count = 0;
        foreach (var s in stages)
        {
            if (s == null) continue;
            var record = save.GetRecord(s.stageId);
            if (record != null) count += record.collectedIds.Count;
        }
        return count;
    }
}