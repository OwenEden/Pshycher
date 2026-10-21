using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EndPointManager : MonoBehaviour
{
    [Header("숨겨둘 EndPoint")]
    public GameObject endPoint;

    [Header("전부 충족해야 EndPoint가 나타남 (비우면 처음부터 표시)")]
    public StageCondition[] conditions;

    [Header("열렸을 때 (연출 연결용)")]
    public UnityEvent onOpened;

    public bool isOpened { get; private set; }

    private readonly HashSet<StageCondition> metConditions = new HashSet<StageCondition>();
    private int requiredCount;

    void Awake()
    {
        requiredCount = 0;
        if (conditions != null)
            foreach (var c in conditions)
                if (c != null) requiredCount++;

        // 조건이 있으면 시작할 때 숨김
        if (requiredCount > 0 && endPoint != null)
            endPoint.SetActive(false);
    }

    void Start()
    {
        if (requiredCount == 0)
        {
            Open();
            return;
        }

        foreach (var c in conditions)
        {
            if (c == null) continue;

            c.OnMet += HandleMet;
            if (c.isMet) metConditions.Add(c);
        }

        CheckOpen();
    }

    void OnDestroy()
    {
        if (conditions == null) return;
        foreach (var c in conditions)
            if (c != null) c.OnMet -= HandleMet;
    }

    private void HandleMet(StageCondition c)
    {
        metConditions.Add(c);
        Debug.Log($"조건 충족: {c.name} ({metConditions.Count}/{requiredCount})");
        CheckOpen();
    }

    private void CheckOpen()
    {
        if (!isOpened && metConditions.Count >= requiredCount)
            Open();
    }

    private void Open()
    {
        if (isOpened) return;
        isOpened = true;

        if (endPoint != null) endPoint.SetActive(true);
        onOpened?.Invoke();
        Debug.Log("EndPoint 등장");
    }
}