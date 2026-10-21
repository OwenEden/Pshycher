using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageInfo : MonoBehaviour
{
    public static StageInfo Current { get; private set; }

    [Header("스테이지 데이터")]
    public StageData stageData;

    [Header("시작 지점")]
    public StartPoint startPoint;

    // ── StageData에서 읽어오는 값 ──────────────
    public bool hasData => stageData != null;
    public string stageId => hasData ? stageData.stageId : "";
    public int world => hasData ? stageData.world : 0;
    public int stage => hasData ? stageData.stage : 0;
    public bool isMainStage => hasData && stageData.isMainStage;

    // 이번 플레이에서 획득한 수집요소
    private readonly HashSet<string> collectedThisRun = new HashSet<string>();
    private readonly List<string> allCollectibleIds = new List<string>();

    private void Awake()
    {
        Current = this;
    }

    private void Start()
    {
        CollectSceneCollectibles();
        ValidateData();
        RestoreOrPlace();
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    // ── 초기화 ────────────────────────────────
    private void CollectSceneCollectibles()
    {
        allCollectibleIds.Clear();
        var seen = new HashSet<string>();

        foreach (var c in FindObjectsByType<Collectible>(FindObjectsInactive.Include))
        {
            if (string.IsNullOrEmpty(c.collectibleId))
            {
                Debug.LogWarning($"[{stageId}] ID가 비어 있는 수집요소: {c.name}", c);
                continue;
            }

            if (!seen.Add(c.collectibleId))
            {
                Debug.LogWarning($"[{stageId}] 수집요소 ID 중복: {c.collectibleId} ({c.name})", c);
                continue;
            }

            allCollectibleIds.Add(c.collectibleId);
        }
    }

    // StageData와 실제 씬 내용이 맞는지 확인
    private void ValidateData()
    {
        if (!hasData)
        {
            Debug.LogError($"{SceneManager.GetActiveScene().name}: StageInfo에 StageData가 연결되지 않았습니다. 기록이 저장되지 않습니다.", this);
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(stageData.sceneName) && stageData.sceneName != sceneName)
        {
            Debug.LogWarning($"[{stageId}] StageData의 씬 이름({stageData.sceneName})과 현재 씬({sceneName})이 다릅니다.", stageData);
        }

        if (stageData.collectibleCount != allCollectibleIds.Count)
        {
            Debug.LogWarning($"[{stageId}] 수집요소 개수 불일치 — StageData: {stageData.collectibleCount}, 씬: {allCollectibleIds.Count}", stageData);
        }
    }

    // ── 시작 위치 / 체크포인트 복원 ────────────
    private void RestoreOrPlace()
    {
        var save = SaveManager.Instance;
        var cp = save?.current?.checkpoint;

        // 이 스테이지의 체크포인트가 있으면 복원
        if (hasData && cp != null && cp.isValid && cp.stageId == stageId)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                var rb = playerObj.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;

                playerObj.transform.position = new Vector3(cp.x, cp.y, 0f);
                playerObj.GetComponent<PlayerController>()?.SetFacing(cp.faceRight);
            }

            // 먹었던 수집요소 복원
            collectedThisRun.Clear();
            foreach (var id in cp.collectedIds)
                collectedThisRun.Add(id);

            // 복원된 수집요소 숨기기
            foreach (var c in FindObjectsByType<Collectible>(FindObjectsInactive.Include))
            {
                if (collectedThisRun.Contains(c.collectibleId))
                    c.gameObject.SetActive(false);
            }

            StageTimer.Instance?.StartTimer(cp.elapsedTime);
            Debug.Log($"[{stageId}] 체크포인트에서 재개 - {cp.checkpointId}");
            return;
        }

        PlacePlayer();
        StageTimer.Instance?.StartTimer();
    }

    private void PlacePlayer()
    {
        if (startPoint == null)
        {
            Debug.LogWarning($"[{stageId}] 시작 지점이 지정되지 않았습니다.");
            return;
        }

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            return;
        }

        var rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        playerObj.transform.position = startPoint.transform.position;
        playerObj.GetComponent<PlayerController>()?.SetFacing(startPoint.faceRight);
    }

    // ── 수집요소 ──────────────────────────────
    public IEnumerable<string> GetCollectedThisRun() => collectedThisRun;

    public void OnCollect(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        collectedThisRun.Add(id);
        Debug.Log($"수집요소 획득: {id} ({collectedThisRun.Count}/{allCollectibleIds.Count})");
    }

    public bool WasCollectedBefore(string id)
    {
        var save = SaveManager.Instance;
        if (!hasData || save == null || !save.hasActiveSlot) return false;

        StageRecord record = save.current.GetRecord(stageId);
        return record != null && record.HasCollected(id);
    }

    // ── 클리어 처리 (EndPoint에서 호출) ────────
    public StageClearResult ClearStage(float clearTime)
    {
        var result = new StageClearResult
        {
            stageId = stageId,
            clearTime = clearTime,
            totalCollectibles = allCollectibleIds.Count,
            collectedCount = collectedThisRun.Count,
            bestClearTime = clearTime
        };

        if (!hasData)
        {
            Debug.LogError("StageData가 없어 클리어 기록을 저장하지 않습니다.", this);
            return result;
        }

        var save = SaveManager.Instance;
        if (save == null || !save.hasActiveSlot)
        {
            Debug.LogWarning("활성 슬롯이 없어 기록이 저장되지 않습니다.");
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

        // 해금 처리 — 이번에 새로 해금된 것만 결과에 기록
        if (stageData.unlockTools != null)
            foreach (var tool in stageData.unlockTools)
                if (save.current.UnlockTool(tool))
                    result.unlockedTools.Add(tool);

        if (stageData.unlockAbilities != null)
            foreach (var ability in stageData.unlockAbilities)
                if (save.current.UnlockAbility(ability))
                    result.unlockedAbilities.Add(ability);

        result.collectedCount = record.collectedIds.Count;
        result.bestClearTime = record.bestClearTime;

        save.current.ClearCheckpoint();
        save.Save();
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

    // 이번 클리어로 새로 해금된 것 (이미 해금돼 있던 건 제외)
    public List<ToolType> unlockedTools = new List<ToolType>();
    public List<AbilityType> unlockedAbilities = new List<AbilityType>();
}