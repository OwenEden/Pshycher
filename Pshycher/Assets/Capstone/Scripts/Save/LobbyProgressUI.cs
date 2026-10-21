using UnityEngine;
using TMPro;

public class LobbyProgressUI : MonoBehaviour
{
    [Header("데이터")]
    public StageDatabase database;

    [Header("UI 참조")]
    public TMP_Text missionText;      // 임무 진행
    public TMP_Text abilityText;      // 능력 상태
    public TMP_Text toolText;         // 도구 상태
    public TMP_Text collectibleText;  // 비밀 발견 진행도

    [Header("문구 형식")]
    public string missionFormat = "임무 진행 : {0} 진행 중";
    public string abilityFormat = "능력 상태 : {0} 획득";
    public string toolFormat = "도구 상태 : {0} 활성";
    public string collectibleFormat = "비밀 발견 진행도 : {0}/{1}";

    [Header("미해금 문구")]
    public string noAbilityLabel = "능력 상태 : 미획득";
    public string noToolLabel = "도구 상태 : 미획득";
    public string noProgressLabel = "1-???";

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
            SetTexts(noProgressLabel, AbilityType.None, ToolType.None, 0, 0);
            return;
        }

        string progress = GetCurrentMissionLabel(save);

        int collected = database != null ? database.GetCollectedCount(save) : 0;
        int total = database != null ? database.GetTotalCollectibles() : 0;

        SetTexts(
            progress,
            save.GetLastUnlockedAbility(),
            save.GetLastUnlockedTool(),
            collected,
            total
        );
    }

    // 클리어한 마지막 스테이지 다음 = 현재 진행 중인 스테이지
    private string GetCurrentMissionLabel(SaveData save)
    {
        if (database == null) return noProgressLabel;

        StageData nextStage = null;

        foreach (var s in database.GetStages(StageCategory.Main))
        {
            if (s == null) continue;

            var record = save.GetRecord(s.stageId);
            bool cleared = record != null && record.cleared;

            if (!cleared)
            {
                // 아직 안 깬 것 중 가장 앞선 것
                if (nextStage == null || s.world < nextStage.world ||
                   (s.world == nextStage.world && s.stage < nextStage.stage))
                    nextStage = s;
            }
        }

        return nextStage != null ? nextStage.stageId : "ALL CLEAR";
    }

    private void SetTexts(string mission, AbilityType ability, ToolType tool, int collected, int total)
    {
        if (missionText != null)
            missionText.text = string.Format(missionFormat, mission);

        if (abilityText != null)
        {
            abilityText.text = ability == AbilityType.None
                ? noAbilityLabel
                : string.Format(abilityFormat, GameNames.GetAbilityName(ability));
        }

        if (toolText != null)
        {
            toolText.text = tool == ToolType.None
                ? noToolLabel
                : string.Format(toolFormat, GameNames.GetToolName(tool));
        }

        if (collectibleText != null)
            collectibleText.text = string.Format(collectibleFormat, collected, total);
    }
}