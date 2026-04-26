using DG.Tweening;
using UnityEngine;

public class StateInfoUIAnimation : MenuBase
{
    [Header("Elements")]
    public RectTransform titleText;       // 위 → 중심
    public RectTransform AbilityPanel;
    public RectTransform CharacterPanel;
    public RectTransform ToolPanel;
    public RectTransform WeaponPanel;
    public RectTransform BackButton;   // 좌 → 중심
    public GameObject background;        // 배경 (애니메이션 없음)

    [Header("Animation Settings")]
    public float duration = 0.5f;
    public float offset = 300f;

    private Vector2 titleOrigin;
    private Vector2 AbilityOrigin;
    private Vector2 CharacterOrigin;
    private Vector2 ToolOrigin;
    private Vector2 WeaponOrigin;
    private Vector2 backButtonOrigin;

    private object tweenId = new object();

    private void Awake()
    {
        titleOrigin = titleText != null ? titleText.anchoredPosition : Vector2.zero;
        AbilityOrigin = AbilityPanel != null ? AbilityPanel.anchoredPosition : Vector2.zero;
        CharacterOrigin = CharacterPanel != null ? CharacterPanel.anchoredPosition : Vector2.zero;
        ToolOrigin = ToolPanel != null ? ToolPanel.anchoredPosition : Vector2.zero;
        WeaponOrigin = WeaponPanel != null ? WeaponPanel.anchoredPosition : Vector2.zero;
        backButtonOrigin = BackButton != null ? BackButton.anchoredPosition : Vector2.zero;
    }

    public override Tween EnterAnimation()
    {
        // 이전에 같은 ID로 생성된 트윈 제거
        DOTween.Kill(tweenId);

        // 초기 위치 설정
        if (titleText != null) titleText.anchoredPosition = titleOrigin + new Vector2(0, offset);
        //if (ExperimentPanel != null) ExperimentPanel.anchoredPosition = ExperimentOrigin + new Vector2(-offset, 0);
        //if (WeaponnPanel != null) WeaponnPanel.anchoredPosition = WeaponOrigin + new Vector2(-offset, 0);
        if (BackButton != null) BackButton.anchoredPosition = backButtonOrigin + new Vector2(offset, 0);

        // 시퀀스 구성 (중첩 대신 Insert로 오프셋 적용하여 원래 지연 방식 재현)
        var seq = DOTween.Sequence().SetId(tweenId);

        // Title 시작
        if (titleText != null)
            seq.Append(titleText.DOAnchorPos(titleOrigin, duration).SetEase(Ease.OutCubic));
        else
            seq.AppendInterval(duration);

        // BackButton은 시퀀스 시작 후 0.15초에 시작
        if (BackButton != null)
            seq.Insert(0.15f, BackButton.DOAnchorPos(backButtonOrigin, duration).SetEase(Ease.OutCubic));

        return seq;
    }

    public override Tween ExitAnimation()
    {
        // 이전에 같은 ID로 생성된 트윈 제거
        DOTween.Kill(tweenId);

        float exitDur = duration * 0.8f;

        // 동시에 나가는 애니메이션을 시퀀스에 병렬로 추가
        var seq = DOTween.Sequence().SetId(tweenId);
        bool anyAdded = false;

        if (titleText != null)
        {
            seq.Append(titleText.DOAnchorPos(titleOrigin + new Vector2(0, offset), exitDur).SetEase(Ease.InCubic));
            anyAdded = true;
        }

        if (BackButton != null)
        {
            if (!anyAdded)
                seq.Append(BackButton.DOAnchorPos(backButtonOrigin + new Vector2(-offset, 0), exitDur).SetEase(Ease.InCubic));
            else
                seq.Join(BackButton.DOAnchorPos(backButtonOrigin + new Vector2(-offset, 0), exitDur).SetEase(Ease.InCubic));
        }

        return seq;
    }
}
