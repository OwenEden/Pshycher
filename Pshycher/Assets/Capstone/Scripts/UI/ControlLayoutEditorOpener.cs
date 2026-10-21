using UnityEngine;

public class ControlLayoutEditorOpener : MonoBehaviour
{
    public ControlLayoutEditor editorPrefab;

    [Tooltip("편집 화면이 열려 있는 동안 숨길 UI (옵션 창, 일시정지 창 등)")]
    public GameObject[] hideWhileEditing;

    private bool[] previousStates;

    // 버튼 OnClick에 연결
    public void Open()
    {
        if (editorPrefab == null) return;
        if (ControlLayoutEditor.Current != null) return;   // 중복 방지

        HideTargets();

        var editor = Instantiate(editorPrefab);
        editor.onClosed += RestoreTargets;
    }

    private void HideTargets()
    {
        if (hideWhileEditing == null) return;

        previousStates = new bool[hideWhileEditing.Length];
        for (int i = 0; i < hideWhileEditing.Length; i++)
        {
            var go = hideWhileEditing[i];
            if (go == null) continue;

            previousStates[i] = go.activeSelf;   // 원래 상태 기억
            go.SetActive(false);
        }
    }

    private void RestoreTargets()
    {
        if (hideWhileEditing == null || previousStates == null) return;

        for (int i = 0; i < hideWhileEditing.Length; i++)
        {
            var go = hideWhileEditing[i];
            if (go != null) go.SetActive(previousStates[i]);
        }

        previousStates = null;
    }
}