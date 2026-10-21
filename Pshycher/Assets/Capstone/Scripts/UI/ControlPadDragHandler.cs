using UnityEngine;
using UnityEngine.EventSystems;

// 편집 복제본의 패드에만 실행 중에 추가됨 — 실제 조작 UI에는 없음
public class ControlPadDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private ControlLayoutGroup group;
    private ControlLayoutRoot root;
    private Vector2 offset;   // 잡은 지점과 패드 중심의 차이

    public void Init(ControlLayoutGroup g, ControlLayoutRoot r)
    {
        group = g;
        root = r;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        Vector2 pointer = root.ScreenToLocal(e.position, e.pressEventCamera);
        offset = root.GetGroupCenter(group) - pointer;
    }

    public void OnDrag(PointerEventData e)
    {
        Vector2 pointer = root.ScreenToLocal(e.position, e.pressEventCamera);
        root.MoveGroupTo(group, pointer + offset);
    }
}