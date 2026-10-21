using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("설정")]
    public float defaultDuration = 0.5f;
    public Color fadeColor = Color.black;

    private CanvasGroup canvasGroup;
    private bool isFading;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        var obj = new GameObject("[ScreenFader]");
        obj.AddComponent<ScreenFader>();
    }

    private void BuildOverlay()
    {
        // Canvas
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;   // 항상 최상단

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        // 전체 화면 검은 이미지
        var imageObj = new GameObject("FadeImage", typeof(RectTransform), typeof(Image));
        imageObj.transform.SetParent(transform, false);

        var rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = imageObj.GetComponent<Image>();
        image.color = fadeColor;
        image.raycastTarget = false;
    }

    // 어두워짐
    public void FadeOut(float duration, Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(1f, duration, onComplete));
    }

    // 밝아짐
    public void FadeIn(float duration, Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(0f, duration, onComplete));
    }

    // 페이드 아웃 → 씬 로드 → 페이드 인
    public void LoadSceneWithFade(string sceneName, float duration = -1f)
    {
        if (isFading) return;
        if (duration < 0f) duration = defaultDuration;
        StartCoroutine(LoadRoutine(sceneName, duration));
    }

    private IEnumerator LoadRoutine(string sceneName, float duration)
    {
        isFading = true;
        canvasGroup.blocksRaycasts = true;   // 전환 중 입력 차단

        yield return FadeRoutine(1f, duration, null);

        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        yield return FadeRoutine(0f, duration, null);

        canvasGroup.blocksRaycasts = false;
        isFading = false;
    }

    private IEnumerator FadeRoutine(float target, float duration, Action onComplete)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;   // timeScale 0에서도 동작
            canvasGroup.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        canvasGroup.alpha = target;
        onComplete?.Invoke();
    }
    // 코루틴에서 yield return으로 대기 가능
    public IEnumerator FadeOutRoutine(float duration = -1f)
    {
        if (duration < 0f) duration = defaultDuration;
        canvasGroup.blocksRaycasts = true;
        yield return FadeRoutine(1f, duration, null);
    }

    public IEnumerator FadeInRoutine(float duration = -1f)
    {
        if (duration < 0f) duration = defaultDuration;
        yield return FadeRoutine(0f, duration, null);
        canvasGroup.blocksRaycasts = false;
    }
}