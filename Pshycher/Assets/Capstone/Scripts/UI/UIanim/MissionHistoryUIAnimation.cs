using DG.Tweening;
using UnityEngine;

public class MissionHistoryUIAnimation : MenuBase
{
    [Header("Elements")]
    public RectTransform titleText;      // 위 → 중심
    [Tooltip("CategoryView·ListView·DetailView를 감싸는 부모 — 페이드")]
    public RectTransform contents;
    public RectTransform BackButton;     // 좌 → 중심
    public GameObject background;        // 배경 (애니메이션 없음)

    [Header("Animation Settings")]
    public float duration = 0.5f;
    public float stagger = 0.05f;
    [Tooltip("화면 밖으로 나간 뒤 추가로 더 떨어뜨릴 여유 거리")]
    public float margin = 20f;

    private Vector2 titleOrigin;
    private Vector2 backButtonOrigin;

    // 등장 시 계산한 화면 밖 거리 — 퇴장 때 재사용
    private Vector2 titleOff;
    private Vector2 backOff;

    private CanvasGroup contentsGroup;

    private readonly object tweenId = new object();

    private void Awake()
    {
        titleOrigin = titleText != null ? titleText.anchoredPosition : Vector2.zero;
        backButtonOrigin = BackButton != null ? BackButton.anchoredPosition : Vector2.zero;

        if (contents != null)
        {
            contentsGroup = contents.GetComponent<CanvasGroup>();
            if (contentsGroup == null)
                contentsGroup = contents.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public override Tween EnterAnimation()
    {
        DOTween.Kill(tweenId);

        // 원위치에 놓고 화면 밖 거리 계산 (기기 해상도에 맞춰 매번 계산)
        if (titleText != null) titleText.anchoredPosition = titleOrigin;
        if (BackButton != null) BackButton.anchoredPosition = backButtonOrigin;

        titleOff = Offscreen(titleText, Vector2.up);
        backOff = Offscreen(BackButton, Vector2.left);

        // 시작 상태
        if (titleText != null) titleText.anchoredPosition = titleOrigin + titleOff;
        if (BackButton != null) BackButton.anchoredPosition = backButtonOrigin + backOff;
        if (contentsGroup != null) contentsGroup.alpha = 0f;

        var seq = DOTween.Sequence().SetId(tweenId);

        if (titleText != null)
            seq.Insert(0f, titleText.DOAnchorPos(titleOrigin, duration).SetEase(Ease.OutCubic));

        if (contentsGroup != null)
            seq.Insert(stagger, contentsGroup.DOFade(1f, duration).SetEase(Ease.OutCubic));

        if (BackButton != null)
            seq.Insert(stagger * 2f, BackButton.DOAnchorPos(backButtonOrigin, duration).SetEase(Ease.OutCubic));

        return seq;
    }

    public override Tween ExitAnimation()
    {
        DOTween.Kill(tweenId);

        float exitDur = duration * 0.8f;
        var seq = DOTween.Sequence().SetId(tweenId);

        if (titleText != null)
            seq.Insert(0f, titleText.DOAnchorPos(titleOrigin + titleOff, exitDur).SetEase(Ease.InCubic));

        if (contentsGroup != null)
            seq.Insert(0f, contentsGroup.DOFade(0f, exitDur).SetEase(Ease.InCubic));

        if (BackButton != null)
            seq.Insert(0f, BackButton.DOAnchorPos(backButtonOrigin + backOff, exitDur).SetEase(Ease.InCubic));

        return seq;
    }

    private Vector2 Offscreen(RectTransform rt, Vector2 dir)
    {
        return rt != null ? UIOffscreenUtil.GetOffscreenOffset(rt, dir, margin) : Vector2.zero;
    }
}