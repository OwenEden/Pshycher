using UnityEngine;

public static class UIOffscreenUtil
{
    private static readonly Vector3[] corners = new Vector3[4];

    // target이 direction 쪽으로 캔버스 밖에 완전히 나가기 위한 anchoredPosition 이동량
    // direction은 상하좌우 중 하나 (Vector2.up 등)
    public static Vector2 GetOffscreenOffset(RectTransform target, Vector2 direction, float margin = 20f)
    {
        Canvas canvas = target.GetComponentInParent<Canvas>();
        if (canvas == null) return direction * 2000f;

        RectTransform canvasRect = canvas.rootCanvas.transform as RectTransform;

        // 캔버스 로컬 좌표로 target 영역 계산
        target.GetWorldCorners(corners);
        Vector2 min = canvasRect.InverseTransformPoint(corners[0]);
        Vector2 max = canvasRect.InverseTransformPoint(corners[2]);
        Rect screen = canvasRect.rect;

        float distance;
        if (direction.y > 0f) distance = screen.yMax - min.y;   // 위로
        else if (direction.y < 0f) distance = max.y - screen.yMin;   // 아래로
        else if (direction.x < 0f) distance = max.x - screen.xMin;   // 왼쪽
        else distance = screen.xMax - min.x;   // 오른쪽

        distance = Mathf.Max(0f, distance) + margin;

        // 캔버스 단위 → target 부모의 로컬 단위로 환산
        float scale = 1f;
        if (target.parent != null)
        {
            bool vertical = direction.y != 0f;
            float parentScale = vertical ? target.parent.lossyScale.y : target.parent.lossyScale.x;
            float canvasScale = vertical ? canvasRect.lossyScale.y : canvasRect.lossyScale.x;
            if (parentScale != 0f) scale = canvasScale / parentScale;
        }

        return direction.normalized * distance * scale;
    }
}