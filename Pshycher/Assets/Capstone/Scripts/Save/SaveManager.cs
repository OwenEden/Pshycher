using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const int SlotCount = 3;   // 늘릴 경우 이 값만 변경

    public SaveData current { get; private set; }
    public int currentSlot { get; private set; } = -1;
    public bool hasActiveSlot => current != null && currentSlot >= 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 씬에 배치하지 않아도 자동 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        var obj = new GameObject("[SaveManager]");
        obj.AddComponent<SaveManager>();
    }

    private static string GetPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"slot_{slot}.json");

    public static bool SlotExists(int slot) => File.Exists(GetPath(slot));

    // ── 파일 읽기 ─────────────────────────────
    private static SaveData ReadFile(int slot)
    {
        string path = GetPath(slot);
        if (!File.Exists(path)) return null;

        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Debug.LogError($"세이브 파일 로드 실패 (슬롯 {slot}): {e.Message}");
            return null;
        }
    }

    // ── Title Scene: 슬롯 요약 ────────────────
    public static SlotSummary GetSummary(int slot)
    {
        var summary = new SlotSummary { slotIndex = slot };

        SaveData data = ReadFile(slot);
        if (data == null) return summary;   // exists = false

        summary.exists = true;
        summary.lastSaved = data.GetLastSavedLocal();
        summary.mainProgress = data.GetMainProgressLabel();
        summary.unlockedTools = new List<ToolType>(data.unlockedTools);
        summary.unlockedAbilities = new List<AbilityType>(data.unlockedAbilities);
        return summary;
    }

    public static List<SlotSummary> GetAllSummaries()
    {
        var list = new List<SlotSummary>();
        for (int i = 0; i < SlotCount; i++)
            list.Add(GetSummary(i));
        return list;
    }

    // ── 새 게임 / 불러오기 ────────────────────
    public void NewGame(int slot)
    {
        currentSlot = slot;
        current = new SaveData();
        Save();
        Debug.Log($"새 게임 시작 - 슬롯 {slot}");
    }

    public bool LoadSlot(int slot)
    {
        SaveData data = ReadFile(slot);
        if (data == null)
        {
            Debug.LogWarning($"슬롯 {slot}에 저장 데이터가 없습니다.");
            return false;
        }

        current = data;
        currentSlot = slot;
        Debug.Log($"슬롯 {slot} 불러오기 완료 - 진행도 {data.GetMainProgressLabel()}");
        return true;
    }

    public void DeleteSlot(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path)) File.Delete(path);

        if (currentSlot == slot)
        {
            current = null;
            currentSlot = -1;
        }
        Debug.Log($"슬롯 {slot} 삭제");
    }

    // ── 저장 ──────────────────────────────────
    public void Save()
    {
        if (!hasActiveSlot)
        {
            Debug.LogWarning("활성 슬롯이 없어 저장할 수 없습니다.");
            return;
        }

        current.lastSavedTime = DateTime.UtcNow.ToString("o");

        try
        {
            File.WriteAllText(GetPath(currentSlot), JsonUtility.ToJson(current, true));
            Debug.Log($"저장 완료 - 슬롯 {currentSlot}");
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message}");
        }
    }

    private void Update()
    {
        if (hasActiveSlot)
            current.totalPlayTime += Time.unscaledDeltaTime;
    }
}