using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Panels")]
    public MenuBase[] menus;

    [Header("Fade CanvasGroup")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.5f;

    [Header("Default Menu On Scene Load")]
    public MenuBase defaultMenu;

    [Header("씬 이름")]
    public string titleSceneName = "Title";

    private MenuBase currentMenu;
    private bool isTransitioning = false;
    private Dictionary<string, MenuBase> menuMap;

    // 뒤로가기용 - 메인 메뉴 이름
    public string mainMenuName => defaultMenu != null ? defaultMenu.name : "";

    private void Awake()
    {
        Instance = this;

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        menuMap = new Dictionary<string, MenuBase>(System.StringComparer.Ordinal);
        if (menus != null)
        {
            foreach (var m in menus)
            {
                if (m == null) continue;
                if (!menuMap.ContainsKey(m.name))
                    menuMap.Add(m.name, m);
                else
                    Debug.LogWarning($"메뉴 이름 중복: {m.name}");
            }
        }
    }

    private void Start()
    {
        // defaultMenu 외 전부 비활성화
        foreach (var m in menus)
        {
            if (m == null) continue;
            m.gameObject.SetActive(m == defaultMenu);
        }

        if (defaultMenu != null)
        {
            currentMenu = defaultMenu;
            currentMenu.EnterAnimation();
        }
    }

    public void SwitchMenu(string menuName)
    {
        if (isTransitioning) return;
        if (currentMenu != null && currentMenu.name == menuName) return;

        isTransitioning = true;
        StartCoroutine(SwitchRoutine(menuName));
    }

    // 뒤로가기 - 항상 메인으로
    public void BackToMain()
    {
        SwitchMenu(mainMenuName);
    }

    private IEnumerator SwitchRoutine(string menuName)
    {
        MenuBase nextMenu = null;
        menuMap?.TryGetValue(menuName, out nextMenu);

        if (nextMenu == null)
        {
            Debug.LogError($"메뉴 '{menuName}'를 찾을 수 없습니다.");
            isTransitioning = false;
            yield break;
        }

        if (currentMenu != null)
            currentMenu.ExitAnimation();

        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true;
            DOTween.Kill(fadeGroup);
            yield return fadeGroup.DOFade(1f, fadeDuration).SetEase(Ease.Linear).WaitForCompletion();
        }
        else yield return null;

        if (currentMenu != null)
            currentMenu.gameObject.SetActive(false);

        nextMenu.gameObject.SetActive(true);
        currentMenu = nextMenu;
        currentMenu.EnterAnimation();

        if (fadeGroup != null)
        {
            DOTween.Kill(fadeGroup);
            yield return fadeGroup.DOFade(0f, fadeDuration).SetEase(Ease.Linear).WaitForCompletion();
            fadeGroup.blocksRaycasts = false;
        }

        isTransitioning = false;
    }

    // -- 씬 전환 -------------------------------------------------
    public void ReturnToTitle()
    {
        StartCoroutine(SceneChangeRoutine(titleSceneName));
    }

    public void EnterStage(StageData stage)
    {
        if (stage == null || string.IsNullOrEmpty(stage.sceneName))
        {
            Debug.LogError("스테이지 씬 이름이 비어 있습니다.");
            return;
        }
        StartCoroutine(SceneChangeRoutine(stage.sceneName));
    }

    private IEnumerator SceneChangeRoutine(string sceneName)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        Tween exitTween = currentMenu != null ? currentMenu.ExitAnimation() : null;

        if (exitTween != null) yield return exitTween.WaitForCompletion();
        else yield return null;

        // 타이틀과 동일한 페이드 사용
        ScreenFader.Instance.LoadSceneWithFade(sceneName);
    }
}