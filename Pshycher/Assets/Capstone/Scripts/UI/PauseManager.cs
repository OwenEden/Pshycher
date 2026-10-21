using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("패널")]
    public GameObject pausePanel;
    public GameObject optionPanel;

    [Header("플레이어 조작 UI")]
    public GameObject playerControlUI;

    [Header("설정 UI")]
    public Slider bgmSlider;
    public Slider effectSlider;
    public Toggle vibrationToggle;

    [Header("씬 이름")]
    public string lobbySceneName = "Main Lobby";

    public bool isPaused { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;   // 씬 전환 시 복구 보장
        if (Instance == this) Instance = null;
    }

    // ── 일시정지 ──────────────────────────────
    public void Pause()
    {
        // 결과창이 떠 있으면 무시
        if (isPaused) return;
        if (ResultManager.Instance != null && ResultManager.Instance.isShowing) return;

        isPaused = true;

        StageTimer.Instance?.PauseTimer();
        Time.timeScale = 0f;

        if (playerControlUI != null) playerControlUI.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;

        if (optionPanel != null) optionPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (playerControlUI != null) playerControlUI.SetActive(true);

        Time.timeScale = 1f;
        StageTimer.Instance?.ResumeTimer();
    }

    // ── 버튼 ──────────────────────────────────
    public void OnResumePressed() => Resume();

    public void OnRestartPressed()
    {
        Time.timeScale = 1f;
        isPaused = false;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        ScreenFader.Instance.LoadSceneWithFade(scene);
    }

    public void OnLobbyPressed()
    {
        Time.timeScale = 1f;
        isPaused = false;

        ScreenFader.Instance.LoadSceneWithFade(lobbySceneName);
    }

    // ── 설정 ──────────────────────────────────
    public void OnOptionPressed()
    {
        LoadSettings();

        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(true);
    }

    public void OnOptionBackPressed()
    {
        SaveSettings();

        if (optionPanel != null) optionPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    private void LoadSettings()
    {
        var data = SettingsManager.Instance?.data;
        if (data == null) return;

        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(data.bgmVolume);
        if (effectSlider != null) effectSlider.SetValueWithoutNotify(data.sfxVolume);
        if (vibrationToggle != null) vibrationToggle.SetIsOnWithoutNotify(data.vibration);
    }

    private void SaveSettings()
    {
        var mgr = SettingsManager.Instance;
        if (mgr == null) return;

        if (bgmSlider != null) mgr.data.bgmVolume = Mathf.Clamp01(bgmSlider.value);
        if (effectSlider != null) mgr.data.sfxVolume = Mathf.Clamp01(effectSlider.value);
        if (vibrationToggle != null) mgr.data.vibration = vibrationToggle.isOn;

        mgr.Save();
    }
}