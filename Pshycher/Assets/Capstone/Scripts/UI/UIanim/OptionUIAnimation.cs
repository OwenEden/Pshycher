using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OptionUIAnimation : MenuBase
{
    [Header("Animation Settings")]
    public float duration = 0.5f;

    [Header("Fade")]
    public CanvasGroup panelGroup;

    [Header("UI Controls")]
    public Button BackButton;
    public Slider bgmSlider;
    public Slider effectSlider;
    public Toggle vibrationToggle;

    private object tweenId = new object();
    private Tween fadeTween;

    void Awake()
    {
        if (panelGroup == null)
        {
            panelGroup = GetComponent<CanvasGroup>();
            if (panelGroup == null)
                panelGroup = gameObject.AddComponent<CanvasGroup>();
        }

        panelGroup.alpha = 0f;
        panelGroup.blocksRaycasts = false;
        panelGroup.interactable = false;

        fadeTween = panelGroup.DOFade(1f, duration)
                              .SetAutoKill(false)
                              .SetRecyclable(true)
                              .SetEase(Ease.OutCubic)
                              .SetId(tweenId)
                              .SetLink(gameObject)
                              .Pause();

        fadeTween.OnComplete(() =>
        {
            panelGroup.blocksRaycasts = true;
            panelGroup.interactable = true;
        });

        fadeTween.OnRewind(() =>
        {
            panelGroup.blocksRaycasts = false;
            panelGroup.interactable = false;
        });

        if (BackButton != null)
            BackButton.onClick.AddListener(OnBackPressed);
    }

    void OnDestroy()
    {
        if (BackButton != null)
            BackButton.onClick.RemoveListener(OnBackPressed);
    }

    // ── SettingsManager 연동 ──────────────────
    public void LoadCurrentSettings()
    {
        var data = SettingsManager.Instance?.data;
        if (data == null) return;

        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(data.bgmVolume);
        if (effectSlider != null) effectSlider.SetValueWithoutNotify(data.sfxVolume);
        if (vibrationToggle != null) vibrationToggle.SetIsOnWithoutNotify(data.vibration);
    }

    public void ApplyToData()
    {
        var mgr = SettingsManager.Instance;
        if (mgr == null) return;

        if (bgmSlider != null) mgr.data.bgmVolume = Mathf.Clamp01(bgmSlider.value);
        if (effectSlider != null) mgr.data.sfxVolume = Mathf.Clamp01(effectSlider.value);
        if (vibrationToggle != null) mgr.data.vibration = vibrationToggle.isOn;
    }

    public void SaveSettings()
    {
        ApplyToData();
        SettingsManager.Instance?.Save();
        Debug.Log("설정 저장 완료");
    }

    // 뒤로가기 — 저장 후 메인으로
    private void OnBackPressed()
    {
        if (fadeTween != null && fadeTween.IsPlaying()) return;
        SaveSettings();
        MenuManager.Instance.SwitchMenu(MenuManager.Instance.mainMenuName);
    }

    public override Tween EnterAnimation()
    {
        if (panelGroup == null) return null;

        LoadCurrentSettings();

        panelGroup.alpha = 0f;
        panelGroup.blocksRaycasts = false;
        panelGroup.interactable = false;

        fadeTween.Restart();
        return fadeTween;
    }

    public override Tween ExitAnimation()
    {
        if (panelGroup == null) return null;

        SaveSettings();   // 어떤 경로로 나가든 저장

        panelGroup.blocksRaycasts = false;
        panelGroup.interactable = false;

        fadeTween.PlayBackwards();
        return fadeTween;
    }
}