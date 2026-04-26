using System.IO;
using UnityEditor.EditorTools;
using UnityEngine;

public static class SaveManager
{
    private static string GetPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    }

    public static void Save(int slot)
    {
        SaveData data = new SaveData();

        // 장비
        PlayerEquipmentSaver saver = Object.FindFirstObjectByType<PlayerEquipmentSaver>();
        if (saver != null)
            data.equipment = saver.GetSaveData();

        // UI용 데이터 (각 매니저에서 가져옴)
/*        data.missionText = MissionManager.Instance.GetMissionText();
        data.abilityText = AbilityManager.Instance.GetAbilityText();
        data.toolText = ToolManager.Instance.GetToolText();

        data.secretFound = SecretManager.Instance.CurrentCount;
        data.secretTotal = SecretManager.Instance.TotalCount;*/

        data.saveTime = System.DateTime.Now.ToString();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slot), json);
    }

    public static SaveData Load(int slot)
    {
        string path = GetPath(slot);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void Apply(int slot)
    {
        SaveData data = Load(slot);
        if (data == null) return;

        PlayerEquipmentSaver saver = Object.FindFirstObjectByType<PlayerEquipmentSaver>();
        if (saver != null)
            saver.LoadFromData(data.equipment);
    }

    public static bool HasSave(int slot)
    {
        return File.Exists(GetPath(slot));
    }
}