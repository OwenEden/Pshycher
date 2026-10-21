using DG.Tweening;
using UnityEngine;

public class StateInfoUIAnimation : MenuBase
{
    [Header("Elements")]
    public RectTransform titleText;        // 위 → 중심
    public RectTransform AbilityPanel;     // 페이드
    public RectTransform CharacterPanel;   // 페이드
    public RectTransform ToolPanel;        // 페이드
    public RectTransform WeaponPanel;      // 페이드
    public RectTransform BackButton;       // 좌 → 중심
    public GameObject background;          // 배경 (애니메이션 없음)

    [Header("Animation Settings")]
    public float duration = 0.5f;
    public float stagger = 0.05f;          // 요소 간 시차
    [Tooltip("화면 밖으로 나간 뒤 추가로 더 떨어뜨릴 여유 거리")]
    public float margin = 20f;

    private Vector2 titleOrigin;
    private Vector2 backButtonOrigin;

    // 등장 시 계산한 화면 밖 거리 — 퇴장 때 재사용
    private Vector2 titleOff;
    private Vector2 backOff;

    private CanvasGroup[] panelGroups;

    private readonly object tweenId = new object();

    private void Awake()
    {
        titleOrigin = titleText != null ? titleText.anchoredPosition : Vector2.zero;
        backButtonOrigin = BackButton != null ? BackButton.anchoredPosition : Vector2.zero;

        panelGroups = new[]
        {
            GetGroup(AbilityPanel),
            GetGroup(CharacterPanel),
            GetGroup(ToolPanel),
            GetGroup(WeaponPanel)
        };
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
        foreach (var g in panelGroups)
            if (g != null) g.alpha = 0f;

        var seq = DOTween.Sequence().SetId(tweenId);

        if (titleText != null)
            seq.Insert(0f, titleText.DOAnchorPos(titleOrigin, duration).SetEase(Ease.OutCubic));

        // 패널은 차례로 페이드 인
        float t = stagger;
        foreach (var g in panelGroups)
        {
            if (g == null) continue;
            seq.Insert(t, g.DOFade(1f, duration).SetEase(Ease.OutCubic));
            t += stagger;
        }

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

        foreach (var g in panelGroups)
            if (g != null)
                seq.Insert(0f, g.DOFade(0f, exitDur).SetEase(Ease.InCubic));

        if (BackButton != null)
            seq.Insert(0f, BackButton.DOAnchorPos(backButtonOrigin + backOff, exitDur).SetEase(Ease.InCubic));

        return seq;
    }

    private CanvasGroup GetGroup(RectTransform panel)
    {
        if (panel == null) return null;

        var group = panel.GetComponent<CanvasGroup>();
        if (group == null) group = panel.gameObject.AddComponent<CanvasGroup>();
        return group;
    }

    private Vector2 Offscreen(RectTransform rt, Vector2 dir)
    {
        return rt != null ? UIOffscreenUtil.GetOffscreenOffset(rt, dir, margin) : Vector2.zero;
    }
}