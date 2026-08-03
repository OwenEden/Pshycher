using UnityEngine;
using TMPro;
using DG.Tweening;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance { get; private set; }

    [Header("결과창 UI")]
    public GameObject resultPanel;
    public RectTransform resultWindowText;
    public RectTransform resultInfo;

    [Header("결과 텍스트 (ResultInfo 자식)")]
    public TMP_Text clearTimeText;
    public TMP_Text bestTimeText;
    public TMP_Text collectibleText;

    [Header("애니메이션 설정")]
    public float scaleUpDuration = 0.5f;
    public float textMoveDelay = 0.3f;
    public float textMoveDuration = 0.4f;
    public float textMoveOffset = 100f;
    public float slideInDuration = 0.4f;
    public float slideInOffset = 300f;
    public Ease scaleUpEase = Ease.OutBack;
    public Ease textMoveEase = Ease.OutCubic;
    public Ease slideInEase = Ease.OutCubic;

    private Vector2 infoOriginalPos;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        infoOriginalPos = resultInfo.anchoredPosition;
    }

    public void ShowResult(StageClearResult result)
    {
        ApplyResultTexts(result);

        resultPanel.SetActive(true);

        resultWindowText.anchoredPosition = Vector2.zero;
        resultWindowText.localScale = Vector3.zero;
        resultInfo.anchoredPosition = infoOriginalPos - new Vector2(0f, slideInOffset);
        resultInfo.gameObject.SetActive(false);

        Sequence seq = DOTween.Sequence();

        seq.Append(resultWindowText
            .DOScale(Vector3.one, scaleUpDuration)
            .SetEase(scaleUpEase));

        seq.AppendInterval(textMoveDelay);
        seq.AppendCallback(() => resultInfo.gameObject.SetActive(true));

        seq.Append(resultWindowText
            .DOAnchorPosY(textMoveOffset, textMoveDuration)
            .SetEase(textMoveEase));

        seq.Join(resultInfo
            .DOAnchorPosY(infoOriginalPos.y, slideInDuration)
            .SetEase(slideInEase));
    }

    private void ApplyResultTexts(StageClearResult result)
    {
        if (result == null) return;

        if (clearTimeText != null)
            clearTimeText.text = StageTimer.Format(result.clearTime);

        if (bestTimeText != null)
            bestTimeText.text = result.isNewRecord
                ? $"NEW! {StageTimer.Format(result.bestClearTime)}"
                : StageTimer.Format(result.bestClearTime);

        if (collectibleText != null)
            collectibleText.text = $"{result.collectedCount} / {result.totalCollectibles}";
    }
}