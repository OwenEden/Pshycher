using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OptionUIAnimation : MenuBase
{
    [Header("Animation Settings")]
    public float duration = 0.5f;
    public float offset = 300f;

    [Header("Fade")]
    public CanvasGroup panelGroup; // 전체 패널 페이드를 위한 CanvasGroup

    [Header("UI Controls (optional, Inspector에 연결)")]
    public Button BackButton;      // 뒤로 가기 버튼(Inspector에서 연결)
    public Slider bgmSlider;       // BGM 볼륨 슬라이더 (선택)
    public Slider effectSlider;    // 효과 볼륨 슬라이더 (선택)

    private object tweenId = new object();
    private Tween fadeTween;

    void Awake()
    {
        // CanvasGroup 확보
        if (panelGroup == null)
        {
            panelGroup = GetComponent<CanvasGroup>();
            if (panelGroup == null)
                panelGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 기본 상태
        panelGroup.alpha = 0f;
        gameObject.SetActive(false);
        panelGroup.blocksRaycasts = false;
        panelGroup.interactable = false;

        // 재사용 가능한 트윈 생성
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
            gameObject.SetActive(false);
        });

        // BackButton 이벤트 자동 연결 (Inspector에서 버튼을 할당하면 동작)
        if (BackButton != null)
            BackButton.onClick.AddListener(OnBackPressed);
    }

    void OnDestroy()
    {
        if (BackButton != null)
            BackButton.onClick.RemoveListener(OnBackPressed);
    }

    // Enter 시 UI에 현재 설정 반영(선택된 슬라이더가 있을 때)
    public void LoadCurrentSettings()
    {
        var mgr = OptionDataManager.Instance;
        if (mgr == null || mgr.optionData == null) return;

        if (bgmSlider != null) bgmSlider.value = mgr.optionData.BGMVolume;
        if (effectSlider != null) effectSlider.value = mgr.optionData.EffectVolume;
    }

    // UI -> OptionData (메모리상에만 반영, 필요시 Save 호출)
    public void ApplyToData()
    {
        var mgr = OptionDataManager.Instance;
        if (mgr == null) return;

        if (mgr.optionData == null) mgr.ResetOptionData();

        if (bgmSlider != null) mgr.optionData.BGMVolume = Mathf.Clamp01(bgmSlider.value);
        if (effectSlider != null) mgr.optionData.EffectVolume = Mathf.Clamp01(effectSlider.value);

        // 이벤트가 필요하면 OptionDataManager.OnOptionsChanged 통해 알릴 수 있음
        // mgr.OnOptionsChanged?.Invoke(mgr.OptionData);
    }

    // 저장하고 닫기 (Back 버튼으로도 호출됨)
    public void SaveAndClose()
    {
        ApplyToData();
        var mgr = OptionDataManager.Instance;
        if (mgr != null) mgr.SaveOptionData();
    }

    // 저장하지 않고 닫기
    public void CancelAndClose()
    {
        ExitAnimation();
    }

    // Back 버튼 눌렀을 때 자동 저장 후 닫기
    private void OnBackPressed()
    {
        // 중복 클릭 방지: 페이드 재생 중이면 무시
        if (fadeTween != null && fadeTween.IsPlaying()) return;
        Debug.Log("option saved");
        SaveAndClose();
    }

    // 전체 패널을 페이드 인
    public override Tween EnterAnimation()
    {
        if (panelGroup == null) return null;

        // 현재 저장된 값을 UI에 로드
        LoadCurrentSettings();

        gameObject.SetActive(true);
        panelGroup.alpha = 0f;
        panelGroup.blocksRaycasts = false;
        panelGroup.interactable = false;

        fadeTween.Restart();
        return fadeTween;
    }

    // 전체 패널을 페이드 아웃
    public override Tween ExitAnimation()
    {
        if (panelGroup == null) return null;

        // 설정값을 저장
        OnBackPressed();

        panelGroup.blocksRaycasts = false;
        panelGroup.interactable = false;

        fadeTween.PlayBackwards();
        return fadeTween;
    }
}
