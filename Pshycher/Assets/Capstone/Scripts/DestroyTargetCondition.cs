using UnityEngine;

// 같은 오브젝트의 Breakable이 부서지면 충족
[RequireComponent(typeof(Breakable))]
public class DestroyTargetCondition : StageCondition
{
    private Breakable breakable;

    void Awake()
    {
        breakable = GetComponent<Breakable>();
        breakable.OnBroken += MarkMet;
    }

    void OnDestroy()
    {
        if (breakable != null)
            breakable.OnBroken -= MarkMet;
    }
}