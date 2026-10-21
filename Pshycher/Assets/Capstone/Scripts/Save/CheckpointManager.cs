using System.Collections;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("리스폰 설정")]
    public float respawnDelay = 0.3f;

    [Header("페이드 연출")]
    public float fadeDuration = 0.35f;
    public float blackHoldTime = 0.15f;   // 완전히 어두운 상태 유지 시간

    private SavePoint lastSavePoint;
    private bool isRespawning = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── 세이브 포인트 도달 ────────────────────
    public void Activate(SavePoint point)
    {
        lastSavePoint = point;

        var save = SaveManager.Instance;
        var info = StageInfo.Current;

        if (save == null || !save.hasActiveSlot || info == null)
        {
            Debug.LogWarning("체크포인트를 저장할 수 없습니다.");
            return;
        }

        Vector2 pos = point.GetRespawnPosition();

        var cp = save.current.checkpoint;
        cp.stageId = info.stageId;
        cp.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        cp.checkpointId = point.checkpointId;
        cp.x = pos.x;
        cp.y = pos.y;
        cp.faceRight = point.faceRight;
        cp.elapsedTime = StageTimer.Instance != null ? StageTimer.Instance.elapsed : 0f;

        cp.collectedIds.Clear();
        cp.collectedIds.AddRange(info.GetCollectedThisRun());

        save.Save();
        Debug.Log($"체크포인트 저장: {point.checkpointId} / {StageTimer.Format(cp.elapsedTime)}");
    }

    // ── 추락 ──────────────────────────────────
    public void OnPlayerFall(GameObject player, float healthLossRate)
    {
        if (isRespawning) return;

        var stat = player.GetComponent<PlayerStat>();
        if (stat == null) return;

        // 이번 추락으로 죽으면 리스폰 없이 즉시 게임오버
        if (stat.WillDieFromFall(healthLossRate))
        {
            FreezePlayer(player);
            stat.ApplyFallDamage(healthLossRate);
            ResultManager.Instance?.ShowGameOver();
            Debug.Log("추락으로 사망");
            return;
        }

        Debug.Log($"추락 - 체력 {healthLossRate * 100f}% 감소");
        StartCoroutine(RespawnRoutine(player, healthLossRate));
    }

    // ── 리스폰 ────────────────────────────────
    // healthLossRate가 음수면 체력 전체 회복, 0 이상이면 그 비율만큼 감소
    private IEnumerator RespawnRoutine(GameObject player, float healthLossRate)
    {
        isRespawning = true;

        var rb = player.GetComponent<Rigidbody2D>();
        var controller = player.GetComponent<PlayerController>();
        var stat = player.GetComponent<PlayerStat>();

        FreezePlayer(player);

        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        // 페이드 아웃
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOutRoutine(fadeDuration);

        // 화면이 가려진 동안 위치 이동 + 체력 처리
        GetRespawnInfo(out Vector2 pos, out bool faceRight);
        player.transform.position = pos;

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }
        if (controller != null)
        {
            controller.enabled = true;
            controller.SetFacing(faceRight);
        }

        if (healthLossRate < 0f)
            stat?.Revive();
        else
            stat?.ApplyFallDamage(healthLossRate);

        if (blackHoldTime > 0f)
            yield return new WaitForSeconds(blackHoldTime);

        // 페이드 인
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeInRoutine(fadeDuration);

        isRespawning = false;
        Debug.Log("리스폰 완료");
    }

    private void FreezePlayer(GameObject player)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        var controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;
    }

    private void GetRespawnInfo(out Vector2 pos, out bool faceRight)
    {
        if (lastSavePoint != null)
        {
            pos = lastSavePoint.GetRespawnPosition();
            faceRight = lastSavePoint.faceRight;
            return;
        }

        var start = StageInfo.Current?.startPoint;
        if (start != null)
        {
            pos = start.transform.position;
            faceRight = start.faceRight;
            return;
        }

        Debug.LogError("리스폰 위치를 찾을 수 없습니다.");
        pos = Vector2.zero;
        faceRight = true;
    }
}