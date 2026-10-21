using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecordDatabase", menuName = "Game/Record Database")]
public class RecordDatabase : ScriptableObject
{
    [Header("카테고리 (표시 순서)")]
    public List<RecordCategoryData> categories = new List<RecordCategoryData>();

    [Header("전체 기록")]
    public List<RecordData> records = new List<RecordData>();

    public int TotalCount
    {
        get
        {
            int count = 0;
            foreach (var r in records) if (r != null) count++;
            return count;
        }
    }

    // 해금된 기록만, order 순으로
    public List<RecordData> GetUnlocked(RecordCategoryData category, SaveData save)
    {
        var list = new List<RecordData>();
        foreach (var r in records)
            if (r != null && r.category == category && r.IsUnlocked(save))
                list.Add(r);

        list.Sort((a, b) => a.order != b.order
            ? a.order.CompareTo(b.order)
            : string.CompareOrdinal(a.name, b.name));
        return list;
    }

    public int CountUnlocked(SaveData save)
    {
        int count = 0;
        foreach (var r in records)
            if (r != null && r.IsUnlocked(save)) count++;
        return count;
    }
}