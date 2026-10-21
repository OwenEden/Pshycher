using UnityEngine;

public enum PadSide { Left, Right }

[RequireComponent(typeof(RectTransform))]
public class ControlLayoutGroup : MonoBehaviour
{
    [Header("패드 ID (dpad / actionPad)")]
    public string groupId = "dpad";

    [Header("기본 위치 (반전 시 반대쪽 절반 사용)")]
    public PadSide defaultSide = PadSide.Left;

    [Header("버튼 배치 반전 허용 (기능패드만 체크)")]
    public bool allowContentMirror = false;

    public RectTransform rt { get; private set; }

    // 현재 값
    public float x { get; private set; }
    public float y { get; private set; }
    public float scale { get; private set; } = 1f;
    public bool contentMirrored { get; private set; }

    // 프리팹 배치 기준 기본값
    public float defaultX { get; private set; }
    public float defaultY { get; private set; }
    public float defaultScale { get; private set; } = 1f;
    private bool defaultsCaptured;

    private ControlLayoutRoot root;

    void Awake()
    {
        rt = (RectTransform)transform;
        root = GetComponentInParent<ControlLayoutRoot>(true);
    }

    // ── 기본값 ────────────────────────────────
    public void CaptureDefaults()
    {
        if (defaultsCaptured) return;

        defaultScale = rt.localScale.x;
        Vector2 t = root.GetRangePosition(this);
        defaultX = t.x;
        defaultY = t.y;

        x = defaultX;
        y = defaultY;
        scale = defaultScale;
        defaultsCaptured = true;
    }

    // ── 위치·크기 ─────────────────────────────
    public void Apply(float nx, float ny, float s)
    {
        x = Mathf.Clamp01(nx);
        y = Mathf.Clamp01(ny);
        scale = root.ClampScale(s);

        rt.localScale = Vector3.one * scale;   // 크기를 먼저 바꿔야 이동 범위가 맞게 계산됨
        root.PlaceGroup(this);
    }

    public void ApplyDefaults() => Apply(defaultX, defaultY, defaultScale);

    // ── 버튼 배치 반전 ────────────────────────
    // 목표 상태와 다를 때만 뒤집음 (두 번 뒤집으면 원래대로)
    public void SetContentMirrored(bool on)
    {
        if (!allowContentMirror || contentMirrored == on) return;
        MirrorChildren();
        contentMirrored = on;
    }

    // 직속 자식 버튼들을 패드 중심 기준으로 좌우 대칭 이동
    private void MirrorChildren()
    {
        float centerX = rt.rect.center.x;

        for (int i = 0; i < rt.childCount; i++)
        {
            var child = rt.GetChild(i) as RectTransform;
            if (child == null) continue;

            Vector3 childCenter = rt.InverseTransformPoint(child.TransformPoint(child.rect.center));
            float dx = (2f * centerX - childCenter.x) - childCenter.x;

            child.position += rt.TransformVector(new Vector3(dx, 0f, 0f));
        }
    }
}