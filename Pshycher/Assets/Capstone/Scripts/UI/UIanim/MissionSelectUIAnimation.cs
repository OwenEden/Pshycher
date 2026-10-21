using DG.Tweening;
using UnityEngine;

public class MissionSelectUIAnimation : MenuBase
{
    [Header("Elements")]
    public RectTransform titleText;        // 위 → 중심
    public RectTransform categoryButtons;  // 위 → 중심
    public RectTransform BackButton;       // 좌 → 중심
    public RectTransform scrollArea;       // 페이드

    [Header("Animation Settings")]
    public float duration = 0.5f;
    public float stagger = 0.05f;
    [Tooltip("화면 밖으로 나간 뒤 추가로 더 떨어뜨릴 여유 거리")]
    public float margin = 20f;

    private Vector2 titleOrigin;
    private Vector2 categoryOrigin;
    private Vector2 backButtonOrigin;
    private CanvasGroup scrollGroup;

    // 등장 시 계산한 화면 밖 거리 - 퇴장 때 재사용
    private Vector2 titleOff;
    private Vector2 categoryOff;
    private Vector2 backOff;

    private readonly object tweenId = new object();

    private void Awake()
    {
        titleOrigin = titleText != null ? titleText.anchoredPosition : Vector2.zero;
        categoryOrigin = categoryButtons != null ? categoryButtons.anchoredPosition : Vector2.zero;
        backButtonOrigin = BackButton != null ? BackButton.anchoredPosition : Vector2.zero;

        if (scrollArea != null)
        {
            scrollGroup = scrollArea.GetComponent<CanvasGroup>();
            if (scrollGroup == null)
                scrollGroup = scrollArea.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public override Tween EnterAnimation()
    {
        DOTween.Kill(tweenId);

        // 원위치에 놓고 화면 밖 거리 계산 (기기 해상도에 맞춰 매번 계산)
        ResetToOrigin();
        titleOff = Offscreen(titleText, Vector2.up);
        categoryOff = Offscreen(categoryButtons, Vector2.up);
        backOff = Offscreen(BackButton, Vector2.left);

        // 시작 위치
        if (titleText != null) titleText.anchoredPosition = titleOrigin + titleOff;
        if (categoryButtons != null) categoryButtons.anchoredPosition = categoryOrigin + categoryOff;
        if (BackButton != null) BackButton.anchoredPosition = backButtonOrigin + backOff;
        if (scrollGroup != null) scrollGroup.alpha = 0f;

        var seq = DOTween.Sequence().SetId(tweenId);

        if (titleText != null)
            seq.Insert(0f, titleText.DOAnchorPos(titleOrigin, duration).SetEase(Ease.OutCubic));

        if (categoryButtons != null)
            seq.Insert(stagger, categoryButtons.DOAnchorPos(categoryOrigin, duration).SetEase(Ease.OutCubic));

        if (BackButton != null)
            seq.Insert(stagger * 2f, BackButton.DOAnchorPos(backButtonOrigin, duration).SetEase(Ease.OutCubic));

        if (scrollGroup != null)
            seq.Insert(stagger * 2f, scrollGroup.DOFade(1f, duration).SetEase(Ease.OutCubic));

        return seq;
    }

    public override Tween ExitAnimation()
    {
        DOTween.Kill(tweenId);

        float exitDur = duration * 0.8f;
        var seq = DOTween.Sequence().SetId(tweenId);

        if (titleText != null)
            seq.Insert(0f, titleText.DOAnchorPos(titleOrigin + titleOff, exitDur).SetEase(Ease.InCubic));

        if (categoryButtons != null)
            seq.Insert(0f, categoryButtons.DOAnchorPos(categoryOrigin + categoryOff, exitDur).SetEase(Ease.InCubic));

        if (BackButton != null)
            seq.Insert(0f, BackButton.DOAnchorPos(backButtonOrigin + backOff, exitDur).SetEase(Ease.InCubic));

        if (scrollGroup != null)
            seq.Insert(0f, scrollGroup.DOFade(0f, exitDur).SetEase(Ease.InCubic));

        return seq;
    }

    private void ResetToOrigin()
    {
        if (titleText != null) titleText.anchoredPosition = titleOrigin;
        if (categoryButtons != null) categoryButtons.anchoredPosition = categoryOrigin;
        if (BackButton != null) BackButton.anchoredPosition = backButtonOrigin;
    }

    private Vector2 Offscreen(RectTransform rt, Vector2 dir)
    {
        return rt != null ? UIOffscreenUtil.GetOffscreenOffset(rt, dir, margin) : Vector2.zero;
    }
}