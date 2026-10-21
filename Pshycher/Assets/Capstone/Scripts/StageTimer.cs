using UnityEngine;

public class StageTimer : MonoBehaviour
{
    public static StageTimer Instance { get; private set; }

    public float elapsed { get; private set; }
    public bool isRunning { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (isRunning)
            elapsed += Time.deltaTime;   // timeScale 0이면 함께 정지
    }

    public void StartTimer(float from = 0f)
    {
        elapsed = from;
        isRunning = true;
    }

    public void StopTimer() => isRunning = false;
    public void PauseTimer() => isRunning = false;
    public void ResumeTimer() => isRunning = true;

    // 00:00.00 형식
    public static string Format(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        int cs = Mathf.FloorToInt((seconds * 100f) % 100f);
        return $"{m:00}:{s:00}.{cs:00}";
    }
}