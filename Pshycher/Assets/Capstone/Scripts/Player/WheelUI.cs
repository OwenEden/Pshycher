using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 도구 휠과 능력 휠 모두 이 컴포넌트를 사용
// WheelMode로 구분
public enum WheelMode { Tool, Ability }

public class WheelUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("설정")]
    public WheelMode mode = WheelMode.Tool;
    public float activationHoldTime = 0.2f; // 휠 열리는 홀드 시간
    public float selectRadius = 40f;         // 중앙에서 선택 인식 반경

    [Header("슬롯 (Inspector에서 순서대로 연결)")]
    public GameObject[] slots;       // 각 슬롯 UI 오브젝트
    public Image[] slotHighlights;   // 선택된 슬롯 하이라이트 이미지

    private ToolManager toolManager;
    private AbilityManager abilityManager;

    private bool isHolding = false;
    private bool wheelOpen = false;
    private float holdTimer = 0f;
    private Vector2 pointerDownPos;
    private int hoveredIndex = -1;
    private int selectedIndex = 0;

    // 도구/능력 슬롯 수
    private int slotCount => mode == WheelMode.Tool ? 5 : 9;

    void Awake()
    {
        toolManager = FindFirstObjectByType<ToolManager>();
        abilityManager = FindFirstObjectByType<AbilityManager>();
        CloseWheel();
    }

    void Update()
    {
        if (!isHolding) return;

        holdTimer += Time.deltaTime;
        if (!wheelOpen && holdTimer >= activationHoldTime)
            OpenWheel();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTimer = 0f;
        pointerDownPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!wheelOpen)
        {
            // 짧게 탭 → 능력 온/오프 (능력 휠만)
            if (mode == WheelMode.Ability)
                abilityManager.ToggleAbility();
        }
        else
        {
            // 휠이 열린 상태에서 손 뗌 → 선택 확정
            if (hoveredIndex >= 0)
                ConfirmSelection(hoveredIndex);
        }

        isHolding = false;
        wheelOpen = false;
        CloseWheel();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!wheelOpen) return;

        Vector2 dir = eventData.position - pointerDownPos;

        if (dir.magnitude < selectRadius)
        {
            ClearHighlight();
            hoveredIndex = -1;
            return;
        }

        // 방향 벡터로 슬롯 인덱스 계산
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int index = Mathf.RoundToInt(angle / (360f / slotCount)) % slotCount;
        if (index != hoveredIndex)
        {
            hoveredIndex = index;
            UpdateHighlight(hoveredIndex);
        }
    }

    private void OpenWheel()
    {
        wheelOpen = true;
        foreach (var slot in slots)
            if (slot != null) slot.SetActive(true);

        hoveredIndex = -1;
        Debug.Log($"{mode} 휠 열림");
    }

    private void CloseWheel()
    {
        foreach (var slot in slots)
            if (slot != null) slot.SetActive(false);
        ClearHighlight();
    }

    private void ConfirmSelection(int index)
    {
        selectedIndex = index;

        if (mode == WheelMode.Tool)
            toolManager.SwitchTool((ToolType)index);
        else
            abilityManager.SwitchAbility((AbilityType)(index + 1)); // None(0) 제외

        Debug.Log($"{mode} 선택 확정: index {index}");
    }

    private void UpdateHighlight(int index)
    {
        ClearHighlight();
        if (index >= 0 && index < slotHighlights.Length && slotHighlights[index] != null)
            slotHighlights[index].color = new Color(1f, 1f, 0f, 0.6f); // 노란 하이라이트
    }

    private void ClearHighlight()
    {
        foreach (var h in slotHighlights)
            if (h != null) h.color = new Color(1f, 1f, 1f, 0.2f);
    }
}