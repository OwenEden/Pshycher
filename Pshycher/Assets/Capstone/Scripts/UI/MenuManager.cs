using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.SceneManagement;

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

    private MenuBase currentMenu;
    private bool isTransitioning = false;

    // 메뉴 이름 -> 인스턴스 매핑 (빠른 조회)
    private Dictionary<string, MenuBase> menuMap;

    private void Awake()
    {
        Instance = this;

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false; // 페이드 중 입력 차단을 제어하기 위해 사용
        }

        // 메뉴 맵 초기화
        menuMap = new Dictionary<string, MenuBase>(System.StringComparer.Ordinal);
        if (menus != null)
        {
            foreach (var m in menus)
            {
                if (m == null) continue;
                if (!menuMap.ContainsKey(m.name))
                    menuMap.Add(m.name, m);
                else
                    Debug.LogWarning($"Duplicate menu name detected: {m.name}");
            }
        }
    }

    private void Start()
    {
        // 씬 로드 시 기본 메뉴 자동 실행
        if (defaultMenu != null)
        {
            currentMenu = defaultMenu;
            currentMenu.gameObject.SetActive(true);
            currentMenu.EnterAnimation();
        }
    }

    public void SwitchMenu(string menuName)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(SwitchRoutine(menuName));
    }

    private IEnumerator SwitchRoutine(string menuName)
    {
        MenuBase nextMenu = null;

        if (menuMap != null)
            menuMap.TryGetValue(menuName, out nextMenu);

        if (nextMenu == null)
        {
            Debug.LogError($"Menu '{menuName}' not found.");
            isTransitioning = false;
            yield break;
        }

        //1. Exit Animation (시작만 하고 바로 진행)
        if (currentMenu != null)
        {
            // 반환된 Tween을 사용하지 않고 애니메이션을 시작만 함 (원래 동작 유지)
            currentMenu.ExitAnimation();
        }

        //2. Fade In (입력 차단) using DOTween
        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true;
            DOTween.Kill(fadeGroup);
            var fadeIn = fadeGroup.DOFade(1f, fadeDuration).SetEase(Ease.Linear);
            yield return fadeIn.WaitForCompletion();
        }
        else
        {
            // 페이드 Group이 없으면 짧게 대기
            yield return null;
        }

        //3. 메뉴 전환
        if (currentMenu != null)
            currentMenu.gameObject.SetActive(false);

        nextMenu.gameObject.SetActive(true);
        currentMenu = nextMenu;

        //4. Enter Animation
        currentMenu.EnterAnimation();

        //5. Fade Out (입력 허용) using DOTween
        if (fadeGroup != null)
        {
            DOTween.Kill(fadeGroup);
            var fadeOut = fadeGroup.DOFade(0f, fadeDuration).SetEase(Ease.Linear);
            yield return fadeOut.WaitForCompletion();
            fadeGroup.blocksRaycasts = false;
        }

        isTransitioning = false;
    }

    public void RETURNtoTitle()
    {
        // ExitAnimation이 끝난 뒤 씬 로드하도록 코루틴으로 처리
        StartCoroutine(ReturnToTitleRoutine());
    }

    private IEnumerator ReturnToTitleRoutine()
    {
        Tween exitTween = null;

        if (currentMenu != null)
        {
            exitTween = currentMenu.ExitAnimation();
        }

        if (exitTween != null)
        {
            // DOTween Tween이 완료될 때까지 대기
            yield return exitTween.WaitForCompletion();
        }
        else
        {
            // 애니메이션이 없으면 한 프레임 대기 (안전장치)
            yield return null;
        }
        SceneManager.LoadScene("Title");
        Debug.Log("Returning to Title Screen...");
    }
}
