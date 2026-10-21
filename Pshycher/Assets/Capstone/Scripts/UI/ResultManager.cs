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
    public TMP_Text windowTitleText;      // Clear / GameOver

    [Header("문구")]
    public string clearLabel = "Clear!";
    public string gameOverLabel = "End...?";

    [Header("클리어 전용 요소")]
    public GameObject clearOnlyGroup;     // 타임·수집요소 표시 묶음
    public TMP_Text clearTimeText;
    public TMP_Text bestTimeText;
    public TMP_Text collectibleText;

    [Header("버튼")]
    public GameObject backButton;
    public GameObject retryButton;
    public GameObject nextButton;         // 클리어 시에만

    [Header("플레이어 조작 UI")]
    public GameObject playerControlUI;    // 조작 버튼 묶음

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
    public bool isShowing { get; private set; }

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

    // ── 클리어 ────────────────────────────────
    public void ShowResult(StageClearResult result)
    {
        if (windowTitleText != null)
            windowTitleText.text = clearLabel;

        if (clearOnlyGroup != null) clearOnlyGroup.SetActive(true);
        if (nextButton != null) nextButton.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        if (backButton != null) backButton.SetActive(true);

        ApplyResultTexts(result);
        PlaySequence();
    }

    // ── 게임 오버 ─────────────────────────────
    public void ShowGameOver()
    {
        if (windowTitleText != null)
            windowTitleText.text = gameOverLabel;

        // 기록 관련 요소 숨김
        if (clearOnlyGroup != null) clearOnlyGroup.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);

        if (retryButton != null) retryButton.SetActive(true);
        if (backButton != null) backButton.SetActive(true);

        PlaySequence();
    }

    // ── 공통 연출 ─────────────────────────────
    private void PlaySequence()
    {
        // Pause 중이었다면 해제
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused)
            PauseManager.Instance.Resume();

        isShowing = true;

        StageTimer.Instance?.StopTimer();

        if (playerControlUI != null)
            playerControlUI.SetActive(false);

        resultPanel.SetActive(true);

        resultWindowText.anchoredPosition = Vector2.zero;
        resultWindowText.localScale = Vector3.zero;
        resultInfo.anchoredPosition = infoOriginalPos - new Vector2(0f, slideInOffset);
        resultInfo.gameObject.SetActive(false);

        Time.timeScale = 0f;   // 즉시 정지

        Sequence seq = DOTween.Sequence().SetUpdate(true);   // timeScale 무시하고 재생

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

    // ── 버튼 ──────────────────────────────────
    public void OnRetryPressed()
    {
        Time.timeScale = 1f;
        isShowing = false;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        ScreenFader.Instance.LoadSceneWithFade(scene);
    }

    public void OnBackPressed()
    {
        Time.timeScale = 1f;
        isShowing = false;

        ScreenFader.Instance.LoadSceneWithFade("Main Lobby");
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;   // 씬 전환 시 복구 보장
    }
}