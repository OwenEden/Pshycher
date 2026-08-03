using UnityEngine;

public class EndPoint : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        StageTimer.Instance?.StopTimer();
        float clearTime = StageTimer.Instance != null ? StageTimer.Instance.elapsed : 0f;

        StageClearResult result = StageInfo.Current?.ClearStage(clearTime);

        Debug.Log($"스테이지 클리어 - {result?.stageId} / {StageTimer.Format(clearTime)}");
        ResultManager.Instance.ShowResult(result);
    }
}