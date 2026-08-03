using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SettingsData
{
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public bool vibration = true;
    public int language = 0;   // 0: 한국어, 1: English
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    public SettingsData data { get; private set; } = new SettingsData();

    private static string Path =>
        System.IO.Path.Combine(Application.persistentDataPath, "settings.json");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        var obj = new GameObject("[SettingsManager]");
        obj.AddComponent<SettingsManager>();
    }

    public void Load()
    {
        if (!File.Exists(Path)) { data = new SettingsData(); return; }

        try
        {
            data = JsonUtility.FromJson<SettingsData>(File.ReadAllText(Path));
        }
        catch (Exception e)
        {
            Debug.LogError($"설정 로드 실패: {e.Message}");
            data = new SettingsData();
        }
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(Path, JsonUtility.ToJson(data, true));
            Debug.Log("설정 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"설정 저장 실패: {e.Message}");
        }
    }
}