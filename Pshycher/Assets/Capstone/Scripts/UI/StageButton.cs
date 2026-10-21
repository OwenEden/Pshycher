using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageButton : MonoBehaviour
{
    [Header("연결할 스테이지")]
    public StageData stageData;

    [Header("UI 참조")]
    public Button button;
    public Image thumbnailImage;
    public TMP_Text labelText;
    public TMP_Text bestTimeText;
    public TMP_Text collectibleText;
    public GameObject clearMark;
    public GameObject lockMark;

    [Header("표시 설정")]
    public bool useDataLabel = false;   // true면 StageData의 이름으로 덮어씀

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClicked);
    }

    // MissionSelectUI에서 호출
    public void Refresh(SaveData save, StageDatabase database)
    {
        if (stageData == null)
        {
            Debug.LogWarning($"{name}: stageData가 연결되지 않았습니다.");
            if (button != null) button.interactable = false;
            return;
        }

        bool unlocked = database != null && database.IsUnlocked(stageData, save);

        // 씬에서 직접 넣은 텍스트를 유지하고 싶으면 useDataLabel을 꺼두세요
        if (useDataLabel && labelText != null)
            labelText.text = stageData.label;

        if (thumbnailImage != null && stageData.thumbnail != null)
            thumbnailImage.sprite = stageData.thumbnail;

        if (button != null)
            button.interactable = unlocked;

        if (lockMark != null)
            lockMark.SetActive(!unlocked);

        StageRecord record = save?.GetRecord(stageData.stageId);
        bool cleared = record != null && record.cleared;

        if (clearMark != null)
            clearMark.SetActive(cleared);

        if (bestTimeText != null)
            bestTimeText.text = cleared ? StageTimer.Format(record.bestClearTime) : "--:--.--";

        if (collectibleText != null)
        {
            int count = record != null ? record.collectedIds.Count : 0;
            collectibleText.text = unlocked ? $"{count}/{stageData.collectibleCount}" : "?";
        }
    }

    private void OnClicked()
    {
        if (stageData == null) return;
        MenuManager.Instance.EnterStage(stageData);
    }
}