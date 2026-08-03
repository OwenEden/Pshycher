using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("UI 패널")]
    [SerializeField] private GameObject titleUIButtons;
    [SerializeField] private GameObject optionUI;
    [SerializeField] private GameObject loadSlotUI;

    [Header("타이틀 버튼")]
    public Button loadButton;

    [Header("UI Controls")]
    public Button optionBackButton;
    public Button loadSlotBackButton;
    public Slider bgmSlider;
    public Slider effectSlider;

    [Header("슬롯 (씬에 배치된 3개 연결)")]
    public SlotUI[] slotUIs;

    [Header("씬 이름")]
    public string lobbySceneName = "Main Lobby";

    // true면 빈 슬롯 선택 시 새 게임 시작
    private bool isNewGameMode = false;

    private void Awake()
    {
        if (loadSlotUI != null) loadSlotUI.SetActive(false);
        if (optionUI != null) optionUI.SetActive(false);

        if (optionBackButton != null)
            optionBackButton.onClick.AddListener(OnBackPressed);

        if (loadSlotBackButton != null)
            loadSlotBackButton.onClick.AddListener(HideLoadSlot);
    }

    private void Start()
    {
        LoadCurrentSettings();
        RefreshLoadButton();
    }

    // ── 세이브 파일 유무에 따라 Load 버튼 활성화 ──
    private void RefreshLoadButton()
    {
        bool anySlotExists = false;
        for (int i = 0; i < SaveManager.SlotCount; i++)
        {
            if (SaveManager.SlotExists(i)) { anySlotExists = true; break; }
        }

        if (loadButton != null)
            loadButton.interactable = anySlotExists;
    }

    // ── 타이틀 버튼 ───────────────────────────
    public void StartButton()
    {
        isNewGameMode = true;
        OpenSlotUI();
    }

    public void LoadButton()
    {
        isNewGameMode = false;
        OpenSlotUI();
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

    // ── 슬롯 UI ───────────────────────────────
    private void OpenSlotUI()
    {
        titleUIButtons.SetActive(false);
        loadSlotUI.SetActive(true);
        StartCoroutine(RefreshNextFrame());
    }

    private System.Collections.IEnumerator RefreshNextFrame()
    {
        yield return null;   // SlotUI의 Awake/OnEnable이 끝난 뒤
        RefreshAllSlots();
    }

    public void HideLoadSlot()
    {
        loadSlotUI.SetActive(false);
        titleUIButtons.SetActive(true);
    }

    private void RefreshAllSlots()
    {
        Debug.Log($"RefreshAllSlots 호출 - 슬롯 개수: {slotUIs?.Length ?? 0}");

        if (slotUIs == null || slotUIs.Length == 0)
        {
            Debug.LogError("slotUIs가 비어 있습니다. Inspector에서 연결해주세요.");
            return;
        }

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null)
            {
                Debug.LogError($"slotUIs[{i}]가 null입니다.");
                continue;
            }
            slotUIs[i].Refresh();
        }
    }

    // SlotUI에서 호출
    public void OnSlotSelected(int slotIndex, bool slotExists)
    {
        if (slotExists)
        {
            // 불러오기 모드에서 기존 슬롯 → 이어하기
            // 새 게임 모드에서 기존 슬롯 → 덮어쓰기 확인이 필요할 수 있음
            if (isNewGameMode)
            {
                Debug.Log($"슬롯 {slotIndex}에 이미 데이터가 있습니다. (덮어쓰기 확인 UI 필요)");
                return;
            }

            if (!SaveManager.Instance.LoadSlot(slotIndex)) return;
        }
        else
        {
            if (!isNewGameMode)
            {
                Debug.Log($"슬롯 {slotIndex}는 비어 있습니다.");
                return;
            }

            SaveManager.Instance.NewGame(slotIndex);
        }

        ScreenFader.Instance.LoadSceneWithFade(lobbySceneName);
    }

    // SlotUI에서 호출
    public void OnSlotDeleteRequested(int slotIndex)
    {
        // 삭제 확인 UI를 붙이려면 이 시점에 연결
        SaveManager.Instance.DeleteSlot(slotIndex);
        RefreshAllSlots();
        RefreshLoadButton();
        Debug.Log($"슬롯 {slotIndex} 삭제 완료");
    }

    // ── 설정 ──────────────────────────────────
    public void LoadCurrentSettings()
    {
        var data = SettingsManager.Instance?.data;
        if (data == null) return;

        if (bgmSlider != null) bgmSlider.value = data.bgmVolume;
        if (effectSlider != null) effectSlider.value = data.sfxVolume;
    }

    public void SaveAndClose()
    {
        var mgr = SettingsManager.Instance;
        if (mgr != null)
        {
            if (bgmSlider != null) mgr.data.bgmVolume = Mathf.Clamp01(bgmSlider.value);
            if (effectSlider != null) mgr.data.sfxVolume = Mathf.Clamp01(effectSlider.value);
            mgr.Save();
        }

        optionUI.SetActive(false);
    }

    private void OnBackPressed() => SaveAndClose();
}