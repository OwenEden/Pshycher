using UnityEngine;
using TMPro;

public class LobbyProgressUI : MonoBehaviour
{
    [Header("데이터")]
    public StageDatabase database;

    [Header("UI 참조")]
    public TMP_Text mainProgressText;   // "1-4"
    public TMP_Text toolText;           // "3/5"
    public TMP_Text abilityText;        // "2/9"
    public TMP_Text collectibleText;    // "12/30"

    [Header("전체 개수")]
    public int totalTools = 5;
    public int totalAbilities = 9;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        SaveData save = SaveManager.Instance?.current;

        if (save == null)
        {
            Debug.LogWarning("활성 슬롯이 없습니다.");
            SetTexts("-", $"0/{totalTools}", $"0/{totalAbilities}", "0/0");
            return;
        }

        string progress = save.GetMainProgressLabel();
        string tools = $"{save.unlockedTools.Count}/{totalTools}";
        string ability = $"{save.unlockedAbilities.Count}/{totalAbilities}";

        string collect = "0/0";
        if (database != null)
            collect = $"{database.GetCollectedCount(save)}/{database.GetTotalCollectibles()}";

        SetTexts(progress, tools, ability, collect);
    }

    private void SetTexts(string progress, string tools, string ability, string collect)
    {
        if (mainProgressText != null) mainProgressText.text = progress;
        if (toolText != null) toolText.text = tools;
        if (abilityText != null) abilityText.text = ability;
        if (collectibleText != null) collectibleText.text = collect;
    }
}