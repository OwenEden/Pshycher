using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Title : MonoBehaviour
{
    [SerializeField] private GameObject titleUIButtons;
    [SerializeField] private GameObject optionUI;
    [SerializeField] private GameObject loadSlotUI;

    [Header("UI Controls (optional, Inspector에 연결)")]
    public Button optionBackButton;      // 뒤로 가기 버튼(Inspector에서 연결)
    public Button loadSlotBackButton;
    public Slider bgmSlider;       // BGM 볼륨 슬라이더 (선택)
    public Slider effectSlider;    // 효과 볼륨 슬라이더 (선택)

    private void Awake()
    {
        if (loadSlotUI != null)
            loadSlotUI.SetActive(false);
        if (optionUI != null)
            optionUI.SetActive(false);
        // BackButton 이벤트 자동 연결 (Inspector에서 버튼을 할당하면 동작)
        if (optionBackButton != null)
            optionBackButton.onClick.AddListener(OnBackPressed);
        if (loadSlotBackButton != null)
            loadSlotBackButton.onClick.AddListener(ShowLoadSlot);
    }
    public void Start()
    {
        LoadCurrentSettings();
    }

    public void StartButton()
    {
        SceneManager.LoadScene("Main Lobby");
    }
    public void LoadButton()
    {
            loadSlotUI.SetActive(true);
            titleUIButtons.SetActive(false);
    }
    public void OptionButton()
    {
        optionUI.SetActive(true);
    }
    public void QuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void ShowLoadSlot()
    {
        Debug.Log("hide load slot");
        loadSlotUI.SetActive(false);
        titleUIButtons.SetActive(true);
    }

    #region Option UI 
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
    }

    // 저장하고 닫기 (Back 버튼으로도 호출됨)
    public void SaveAndClose()
    {
        ApplyToData();
        var mgr = OptionDataManager.Instance;
        if (mgr != null) mgr.SaveOptionData();
    }

    private void OnBackPressed()
    {
        SaveAndClose();
        optionUI.SetActive(false);
    }
    #endregion
}
