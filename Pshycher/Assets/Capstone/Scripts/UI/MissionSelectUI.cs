using UnityEngine;
using UnityEngine.UI;

public class MissionSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class CategoryView
    {
        public StageCategory category;
        public Button categoryButton;
        public GameObject scrollRoot;   // ScrollRect 오브젝트
    }

    [Header("데이터")]
    public StageDatabase database;

    [Header("카테고리 (메인/능력/도구)")]
    public CategoryView[] categoryViews;

    [Header("초기 표시")]
    public StageCategory defaultCategory = StageCategory.Main;

    private StageCategory current;

    private void Awake()
    {
        foreach (var view in categoryViews)
        {
            if (view.categoryButton == null) continue;

            StageCategory captured = view.category;
            view.categoryButton.onClick.AddListener(() => ShowCategory(captured));
        }
    }

    private void OnEnable()
    {
        ShowCategory(defaultCategory);
    }

    public void ShowCategory(StageCategory category)
    {
        current = category;

        foreach (var view in categoryViews)
        {
            bool active = view.category == category;

            if (view.scrollRoot != null)
                view.scrollRoot.SetActive(active);

            if (view.categoryButton != null)
                view.categoryButton.interactable = !active;
        }

        RefreshButtons(category);
    }

    // 해당 카테고리의 버튼 상태 갱신
    private void RefreshButtons(StageCategory category)
    {
        CategoryView view = FindView(category);
        if (view == null || view.scrollRoot == null) return;

        SaveData save = SaveManager.Instance?.current;
        if (save == null)
        {
            Debug.LogWarning("활성 슬롯이 없습니다.");
            return;
        }

        // 켜진 ScrollView 아래의 버튼만 갱신 (비활성 포함)
        var buttons = view.scrollRoot.GetComponentsInChildren<StageButton>(true);
        foreach (var btn in buttons)
            btn.Refresh(save, database);
    }

    // 전체 갱신이 필요할 때
    public void RefreshAll()
    {
        SaveData save = SaveManager.Instance?.current;
        if (save == null) return;

        foreach (var view in categoryViews)
        {
            if (view.scrollRoot == null) continue;

            var buttons = view.scrollRoot.GetComponentsInChildren<StageButton>(true);
            foreach (var btn in buttons)
                btn.Refresh(save, database);
        }
    }

    private CategoryView FindView(StageCategory category)
    {
        foreach (var v in categoryViews)
            if (v.category == category) return v;
        return null;
    }
}