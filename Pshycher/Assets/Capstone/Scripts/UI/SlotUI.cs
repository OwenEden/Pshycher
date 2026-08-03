using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    [Header("슬롯 번호 (0부터)")]
    public int slotIndex;

    [Header("UI 참조")]
    public Button slotButton;
    public TMP_Text progressText;      // "1-4" 또는 "새 게임"
    public TMP_Text lastSavedText;     // "2026-08-02 14:30"
    public TMP_Text toolCountText;     // "도구 3/5"
    public TMP_Text abilityCountText;  // "능력 2/9"
    public GameObject emptyMark;       // 빈 슬롯 표시용 오브젝트
    public Button deleteButton;        // 선택

    private TitleManager titleManager;
    private bool slotExists;

    private void Awake()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    private TitleManager Manager
    {
        get
        {
            if (titleManager == null)
                titleManager = FindAnyObjectByType<TitleManager>();
            return titleManager;
        }
    }

    public void Refresh()
    {
        SlotSummary summary = SaveManager.GetSummary(slotIndex);
        slotExists = summary.exists;

        if (emptyMark != null)
            emptyMark.SetActive(!slotExists);

        if (deleteButton != null)
            deleteButton.gameObject.SetActive(slotExists);

        if (!slotExists)
        {
            if (progressText != null) progressText.text = "새 게임";
            if (lastSavedText != null) lastSavedText.text = "";
            if (toolCountText != null) toolCountText.text = "";
            if (abilityCountText != null) abilityCountText.text = "";
            return;
        }

        if (progressText != null)
            progressText.text = summary.mainProgress;

        if (lastSavedText != null)
            lastSavedText.text = summary.lastSaved.ToString("yyyy-MM-dd HH:mm");

        // GravityShift 포함 9종, None 제외
        if (toolCountText != null)
            toolCountText.text = $"도구 {summary.unlockedTools.Count}/5";

        if (abilityCountText != null)
            abilityCountText.text = $"능력 {summary.unlockedAbilities.Count}/9";
    }

    private void OnSlotClicked()
    {
        Manager.OnSlotSelected(slotIndex, slotExists);
    }

    private void OnDeleteClicked()
    {
        Manager.OnSlotDeleteRequested(slotIndex);
    }
}