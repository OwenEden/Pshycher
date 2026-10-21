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

    [Header("슬롯 가득 참 안내 UI")]
    public GameObject slotFullPanel;
    public Button slotFullYesButton;
    public Button slotFullNoButton;

    [Header("덮어쓰기 확인 UI")]
    public GameObject overwritePanel;
    public Button overwriteYesButton;
    public Button overwriteNoButton;

    private int pendingOverwriteSlot = -1;

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

        if (slotFullPanel != null) slotFullPanel.SetActive(false);

        if (slotFullYesButton != null)
            slotFullYesButton.onClick.AddListener(OnSlotFullYes);

        if (slotFullNoButton != null)
            slotFullNoButton.onClick.AddListener(OnSlotFullNo);

        if (overwritePanel != null) overwritePanel.SetActive(false);

        if (overwriteYesButton != null)
            overwriteYesButton.onClick.AddListener(OnOverwriteYes);

        if (overwriteNoButton != null)
            overwriteNoButton.onClick.AddListener(OnOverwriteNo);
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
        int emptySlot = FindEmptySlot();

        if (emptySlot >= 0)
        {
            // 빈 슬롯에 바로 새 게임 생성
            SaveManager.Instance.NewGame(emptySlot);
            ScreenFader.Instance.LoadSceneWithFade(lobbySceneName);
            return;
        }

        // 전부 차 있으면 안내
        isNewGameMode = true;
        if (slotFullPanel != null)
            slotFullPanel.SetActive(true);
        else
            Debug.LogWarning("슬롯이 가득 찼지만 안내 UI가 연결되지 않았습니다.");
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
        isNewGameMode = false;
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
            if (isNewGameMode)
            {
                // 덮어쓰기 확인
                pendingOverwriteSlot = slotIndex;

                if (overwritePanel != null)
                    overwritePanel.SetActive(true);
                else
                    Debug.LogWarning("덮어쓰기 확인 UI가 연결되지 않았습니다.");
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
    private int FindEmptySlot()
    {
        for (int i = 0; i < SaveManager.SlotCount; i++)
            if (!SaveManager.SlotExists(i)) return i;
        return -1;
    }

    // 안내 UI - 슬롯 삭제하러 가기
    private void OnSlotFullYes()
    {
        if (slotFullPanel != null) slotFullPanel.SetActive(false);
        OpenSlotUI();
    }

    private void OnSlotFullNo()
    {
        if (slotFullPanel != null) slotFullPanel.SetActive(false);
        isNewGameMode = false;
    }

    // 안내 UI - 덮어쓰기 확인
    private void OnOverwriteYes()
    {
        if (pendingOverwriteSlot < 0) return;

        if (overwritePanel != null) overwritePanel.SetActive(false);

        // 기존 데이터를 지우고 새 게임 생성
        SaveManager.Instance.DeleteSlot(pendingOverwriteSlot);
        SaveManager.Instance.NewGame(pendingOverwriteSlot);

        pendingOverwriteSlot = -1;
        ScreenFader.Instance.LoadSceneWithFade(lobbySceneName);
    }

    private void OnOverwriteNo()
    {
        pendingOverwriteSlot = -1;
        if (overwritePanel != null) overwritePanel.SetActive(false);
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