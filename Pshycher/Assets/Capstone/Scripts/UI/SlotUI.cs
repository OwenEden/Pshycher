using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    [Header("슬롯 번호 (0부터)")]
    public int slotIndex;

    [Header("UI 참조")]
    public Button slotButton;
    public TMP_Text progressText;      // 임무 진행
    public TMP_Text lastSavedText;
    public TMP_Text toolCountText;     // 도구 상태
    public TMP_Text abilityCountText;  // 능력 상태
    public GameObject emptyMark;
    public Button deleteButton;

    [Header("문구 형식")]
    public string missionFormat = "임무 : {0} 완료";
    public string abilityFormat = "능력 : {0} 획득";
    public string toolFormat = "도구 : {0} 활성";

    [Header("미해금 문구")]
    public string noAbilityLabel = "능력 : 미획득";
    public string noToolLabel = "도구 : 미획득";

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
        {
            progressText.text = summary.mainProgress == "-"
                ? "임무 : 시작 전"
                : string.Format(missionFormat, summary.mainProgress);
        }

        if (lastSavedText != null)
            lastSavedText.text = summary.lastSaved.ToString("yyyy-MM-dd HH:mm");

        // 마지막으로 해금한 도구 / 능력
        ToolType lastTool = summary.unlockedTools.Count > 0
            ? summary.unlockedTools[summary.unlockedTools.Count - 1]
            : ToolType.None;

        AbilityType lastAbility = summary.unlockedAbilities.Count > 0
            ? summary.unlockedAbilities[summary.unlockedAbilities.Count - 1]
            : AbilityType.None;

        if (toolCountText != null)
        {
            toolCountText.text = lastTool == ToolType.None
                ? noToolLabel
                : string.Format(toolFormat, GameNames.GetToolName(lastTool));
        }

        if (abilityCountText != null)
        {
            abilityCountText.text = lastAbility == AbilityType.None
                ? noAbilityLabel
                : string.Format(abilityFormat, GameNames.GetAbilityName(lastAbility));
        }
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