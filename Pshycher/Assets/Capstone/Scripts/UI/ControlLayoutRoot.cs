using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ControlLayoutRoot : MonoBehaviour
{
    // 저장 시 모든 실제 조작 UI에 다시 적용하라는 신호
    public static event Action LayoutChanged;

    [Header("크기 조절 범위")]
    public float minScale = 0.6f;
    public float maxScale = 1.6f;

    [Header("노치 영역 피하기")]
    public bool respectSafeArea = true;

    [Header("편집 중 패드 영역 표시")]
    public Color editHighlightColor = new Color(1f, 1f, 1f, 0.12f);

    public bool isEditCopy { get; private set; }
    public bool swapSides { get; private set; }
    public bool mirrorContents { get; private set; }

    public event Action onApplied;

    private RectTransform rootRect;
    private Canvas canvas;
    private readonly List<ControlLayoutGroup> groups = new List<ControlLayoutGroup>();
    private bool pendingApply;

    private static readonly Vector3[] corners = new Vector3[4];

    public void MarkAsEditCopy() => isEditCopy = true;

    void Awake()
    {
        rootRect = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
        GetComponentsInChildren(true, groups);

        if (!isEditCopy)
            LayoutChanged += OnLayoutChanged;
    }

    void OnDestroy()
    {
        LayoutChanged -= OnLayoutChanged;
    }

    void Start()
    {
        if (isEditCopy) SetupEditMode();
        ApplySaved();
    }

    void OnEnable()
    {
        if (pendingApply)
        {
            pendingApply = false;
            ApplySaved();
        }
    }

    private void OnLayoutChanged()
    {
        if (isActiveAndEnabled) ApplySaved();
        else pendingApply = true;
    }

    // ── 적용 / 저장 ───────────────────────────
    public void ApplySaved()
    {
        Canvas.ForceUpdateCanvases();
        var data = SettingsManager.Instance?.data;

        // 기본값은 반전하지 않은 상태 기준으로 기록 (최초 1회)
        swapSides = false;
        foreach (var g in groups) g.CaptureDefaults();

        swapSides = data != null && data.controlSwapSides;

        foreach (var g in groups)
        {
            var entry = data?.GetLayout(g.groupId);

            if (entry != null) g.Apply(entry.x, entry.y, entry.scale);
            else if (swapSides) g.Apply(1f - g.defaultX, g.defaultY, g.defaultScale);
            else g.ApplyDefaults();
        }

        SetMirrorContents(data != null && data.controlMirrorActionButtons);

        onApplied?.Invoke();
    }

    public void ResetToDefault()
    {
        swapSides = false;
        foreach (var g in groups) g.ApplyDefaults();
        SetMirrorContents(false);
    }

    public void SaveToSettings()
    {
        var mgr = SettingsManager.Instance;
        if (mgr == null) return;

        mgr.data.controlSwapSides = swapSides;
        mgr.data.controlMirrorActionButtons = mirrorContents;
        mgr.data.controlLayout.Clear();

        foreach (var g in groups)
        {
            mgr.data.controlLayout.Add(new ControlLayoutEntry
            {
                id = g.groupId,
                x = g.x,
                y = g.y,
                scale = g.scale
            });
        }

        mgr.Save();
        LayoutChanged?.Invoke();
    }

    // ── 편집 모드 ─────────────────────────────
    private void SetupEditMode()
    {
        foreach (var g in groups)
        {
            // 평소 숨겨진 버튼(상하 등)도 표시
            for (int i = 0; i < g.transform.childCount; i++)
                g.transform.GetChild(i).gameObject.SetActive(true);

            // 버튼 사이 빈 곳도 잡을 수 있게 패드 영역에 반투명 그래픽 추가
            var img = g.GetComponent<Image>();
            if (img == null)
            {
                img = g.gameObject.AddComponent<Image>();
                img.color = editHighlightColor;
            }
            img.raycastTarget = true;

            g.gameObject.AddComponent<ControlPadDragHandler>().Init(g, this);
        }
    }

    // 드래그 — 원하는 중심 위치로 이동 (자기 쪽 절반 밖으로는 못 나감)
    public void MoveGroupTo(ControlLayoutGroup g, Vector2 desiredCenter)
    {
        GetGroupRect(g, out _, out Vector2 half);
        Rect b = GetBoundsFor(g);

        float tx = Mathf.InverseLerp(b.xMin + half.x, b.xMax - half.x, desiredCenter.x);
        float ty = Mathf.InverseLerp(b.yMin + half.y, b.yMax - half.y, desiredCenter.y);

        g.Apply(tx, ty, g.scale);
    }

    // ── 반전 ──────────────────────────────────
    public void SetSwapSides(bool on)
    {
        if (swapSides == on) return;
        swapSides = on;

        // 바깥쪽 가장자리 기준을 유지하도록 가로 위치를 뒤집음
        foreach (var g in groups)
            g.Apply(1f - g.x, g.y, g.scale);
    }

    public void SetMirrorContents(bool on)
    {
        mirrorContents = on;
        foreach (var g in groups)
            g.SetContentMirrored(on);
    }

    public PadSide GetSide(ControlLayoutGroup g)
    {
        if (!swapSides) return g.defaultSide;
        return g.defaultSide == PadSide.Left ? PadSide.Right : PadSide.Left;
    }

    // ── 조회 ──────────────────────────────────
    public ControlLayoutGroup GetGroup(string id) => groups.Find(g => g.groupId == id);

    public float ClampScale(float s) => Mathf.Clamp(s, minScale, maxScale);

    public Vector2 ScreenToLocal(Vector2 screen, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, screen, cam, out Vector2 local);
        return local;
    }

    public Vector2 GetGroupCenter(ControlLayoutGroup g)
    {
        GetGroupRect(g, out Vector2 center, out _);
        return center;
    }

    // ── 배치 계산 ─────────────────────────────
    public void PlaceGroup(ControlLayoutGroup g)
    {
        GetGroupRect(g, out Vector2 center, out Vector2 half);
        Rect bounds = GetBoundsFor(g);

        Vector2 target = new Vector2(
            RangeLerp(bounds.xMin + half.x, bounds.xMax - half.x, bounds.center.x, g.x),
            RangeLerp(bounds.yMin + half.y, bounds.yMax - half.y, bounds.center.y, g.y));

        g.rt.position += rootRect.TransformVector(target - center);
    }

    public Vector2 GetRangePosition(ControlLayoutGroup g)
    {
        GetGroupRect(g, out Vector2 center, out Vector2 half);
        Rect bounds = GetBoundsFor(g);

        return new Vector2(
            Mathf.InverseLerp(bounds.xMin + half.x, bounds.xMax - half.x, center.x),
            Mathf.InverseLerp(bounds.yMin + half.y, bounds.yMax - half.y, center.y));
    }

    private static float RangeLerp(float min, float max, float fallback, float t)
    {
        return min > max ? fallback : Mathf.Lerp(min, max, t);
    }

    private Rect GetBoundsFor(ControlLayoutGroup g)
    {
        Rect b = GetSafeBoundsLocal();
        float mid = rootRect.rect.center.x;

        return GetSide(g) == PadSide.Left
            ? Rect.MinMaxRect(b.xMin, b.yMin, Mathf.Min(mid, b.xMax), b.yMax)
            : Rect.MinMaxRect(Mathf.Max(mid, b.xMin), b.yMin, b.xMax, b.yMax);
    }

    private void GetGroupRect(ControlLayoutGroup g, out Vector2 center, out Vector2 half)
    {
        g.rt.GetWorldCorners(corners);
        Vector2 min = rootRect.InverseTransformPoint(corners[0]);
        Vector2 max = rootRect.InverseTransformPoint(corners[2]);
        center = (min + max) * 0.5f;
        half = (max - min) * 0.5f;
    }

    private Rect GetSafeBoundsLocal()
    {
        Rect r = rootRect.rect;
        if (!respectSafeArea) return r;

        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;

        Rect safe = Screen.safeArea;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, safe.min, cam, out Vector2 min);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, safe.max, cam, out Vector2 max);

        return Rect.MinMaxRect(
            Mathf.Max(r.xMin, min.x), Mathf.Max(r.yMin, min.y),
            Mathf.Min(r.xMax, max.x), Mathf.Min(r.yMax, max.y));
    }
}