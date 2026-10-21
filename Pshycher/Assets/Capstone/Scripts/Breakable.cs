using System;
using UnityEngine;

public class Breakable : MonoBehaviour, LivingEntity
{
    [Header("내구도")]
    public float maxHealth = 30f;
    public float currentHealth { get; protected set; }
    public bool dead { get; protected set; }

    [Header("파괴 조건")]
    [Tooltip("비워두면 모든 도구로 파괴 가능")]
    public ToolType[] breakableBy;

    [Header("연출")]
    public GameObject breakEffect;      // 파티클 프리팹 (선택)
    public float destroyDelay = 0f;     // 이펙트 재생 시간만큼 지연

    [Header("파괴 시 활성화")]
    [Tooltip("EndPoint는 이곳이 아니라 Manager에서 따로 등록")]
    public GameObject[] activateOnBreak;   // 숨겨진 통로, 아이템 등

    private ToolManager toolManager;
    public event Action OnBroken;

    private void Awake()
    {
        currentHealth = maxHealth;
        toolManager = FindAnyObjectByType<ToolManager>();
    }

    public void OnDamage(float damage)
    {
        if (dead) return;

        // 지정된 도구로만 파괴 가능
        if (!CanBreakWithCurrentTool())
        {
            Debug.Log($"{gameObject.name}: 현재 도구로는 파괴할 수 없습니다.");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 내구도: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Break();
    }

    private bool CanBreakWithCurrentTool()
    {
        if (breakableBy == null || breakableBy.Length == 0) return true;
        if (toolManager == null) return true;

        foreach (var t in breakableBy)
            if (t == toolManager.currentTool) return true;

        return false;
    }

    private void Break()
    {
        dead = true;

        if (breakEffect != null)
            Instantiate(breakEffect, transform.position, Quaternion.identity);

        foreach (var obj in activateOnBreak)
            if (obj != null) obj.SetActive(true);

        Debug.Log($"{gameObject.name} 파괴됨");
        OnBroken?.Invoke();
        Destroy(gameObject, destroyDelay);
    }
}