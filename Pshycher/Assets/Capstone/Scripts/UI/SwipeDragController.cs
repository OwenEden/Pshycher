using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class SwipeDragController : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference pressAction;
    public InputActionReference deltaAction;
    public InputActionReference positionAction;

    [Header("Carousels")]
    public VerticalCarousel[] carousels;

    [Header("Drag")]
    public float dragSensitivity = 1f;

    private VerticalCarousel activeCarousel;
    private bool isDragging;

    private void OnEnable()
    {
        pressAction.action.started += OnPress;
        pressAction.action.canceled += OnRelease;

        pressAction.action.Enable();
        deltaAction.action.Enable();
        positionAction.action.Enable();
    }

    private void OnDisable()
    {
        pressAction.action.started -= OnPress;
        pressAction.action.canceled -= OnRelease;

        pressAction.action.Disable();
        deltaAction.action.Disable();
        positionAction.action.Disable();
    }

    void Update()
    {
        if (!isDragging || activeCarousel == null) return;

        float deltaY = deltaAction.action.ReadValue<Vector2>().y;
        activeCarousel.MoveAll(deltaY * dragSensitivity);
    }

    void OnPress(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = positionAction.action.ReadValue<Vector2>();
        activeCarousel = FindTouchedCarousel(screenPos);

        if (activeCarousel != null)
        {
            isDragging = true;
            DOTween.Kill(activeCarousel);
        }
    }

    void OnRelease(InputAction.CallbackContext ctx)
    {
        if (!isDragging || activeCarousel == null) return;

        isDragging = false;
        activeCarousel.SnapToCenter();
        activeCarousel = null;
    }

    VerticalCarousel FindTouchedCarousel(Vector2 screenPos)
    {
        foreach (var carousel in carousels)
        {
            RectTransform rect = carousel.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    rect, screenPos))
            {
                return carousel;
            }
        }
        return null;
    }
}
