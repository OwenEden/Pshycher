using System;
using UnityEngine;

public abstract class StageCondition : MonoBehaviour
{
    public bool isMet { get; private set; }
    public event Action<StageCondition> OnMet;

    // 자식 클래스에서 조건을 충족했을 때 호출 (한 번만 발동)
    protected void MarkMet()
    {
        if (isMet) return;
        isMet = true;
        OnMet?.Invoke(this);
    }
}