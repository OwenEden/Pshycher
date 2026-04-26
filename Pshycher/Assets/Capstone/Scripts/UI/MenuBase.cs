using UnityEngine;
using DG.Tweening;

public abstract class MenuBase : MonoBehaviour
{
    // Enter/Exit 애니메이션은 DOTween Tween을 반환하도록 변경했습니다.
    // 반환된 Tween이 null이 아닐 경우 호출자는 완료를 대기할 수 있습니다.
    public abstract Tween EnterAnimation();
    public abstract Tween ExitAnimation();
}
