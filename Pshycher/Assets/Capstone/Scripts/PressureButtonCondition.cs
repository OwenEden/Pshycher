using DG.Tweening;
using UnityEngine;

// 위에서 밟으면 눌린 상태로 고정
[RequireComponent(typeof(Collider2D))]
public class PressureButtonCondition : StageCondition
{
    [Header("누를 수 있는 대상")]
    public string[] activatorTags = { "Player" };
    [Tooltip("체크하면 위에서 밟았을 때만 눌림 (옆이나 아래에서 닿으면 무시)")]
    public bool requireFromAbove = true;

    [Header("눌림 표시")]
    [Tooltip("납작해질 부분 (비우면 이 오브젝트). 피벗이 아래쪽이어야 바닥에 붙은 채로 눌림")]
    public Transform pressVisual;
    [Range(0.05f, 1f)] public float pressedScaleRatio = 0.4f;   // 원래 Y 스케일 대비 비율
    public float pressDuration = 0.08f;                         // 0이면 즉시

    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (pressVisual == null) pressVisual = transform;
    }

    void Reset()
    {
        // 컴포넌트를 처음 붙일 때 트리거로 설정
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isMet || !IsActivator(other)) return;

        // 대상의 발이 버튼 중앙보다 위에 있어야 밟은 것으로 인정
        if (requireFromAbove && other.bounds.min.y < col.bounds.center.y) return;

        MarkMet();
        ShowPressed();
    }

    private bool IsActivator(Collider2D other)
    {
        foreach (var tag in activatorTags)
            if (other.CompareTag(tag)) return true;
        return false;
    }

    private void ShowPressed()
    {
        float targetY = pressVisual.localScale.y * pressedScaleRatio;

        if (pressDuration > 0f)
        {
            pressVisual.DOScaleY(targetY, pressDuration)
                       .SetEase(Ease.OutQuad)
                       .SetLink(pressVisual.gameObject);
        }
        else
        {
            Vector3 s = pressVisual.localScale;
            s.y = targetY;
            pressVisual.localScale = s;
        }
    }
}