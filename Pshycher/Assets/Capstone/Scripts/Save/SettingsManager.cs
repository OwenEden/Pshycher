using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SettingsData
{
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public bool vibration = true;
    public int language = 0;   // 0: 한국어, 1: English

    // 조작 UI 배치
    public bool controlSwapSides = false;             // 방향패드 ↔ 기능패드 반전
    public bool controlMirrorActionButtons = false;   // 기능패드 버튼 배치 반전
    public List<ControlLayoutEntry> controlLayout = new List<ControlLayoutEntry>();

    public ControlLayoutEntry GetLayout(string id) => controlLayout.Find(e => e.id == id);
}

[Serializable]
public class ControlLayoutEntry
{
    public string id;
    public float x;          // 이동 가능 범위 안의 가로 위치 (0 = 왼쪽 끝, 1 = 오른쪽 끝)
    public float y;          // 세로 위치 (0 = 아래 끝, 1 = 위 끝)
    public float scale = 1f;
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    public SettingsData data { get; private set; } = new SettingsData();

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "settings.json");

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
        if (!File.Exists(SavePath))
        {
            data = new SettingsData();
            return;
        }

        try
        {
            data = JsonUtility.FromJson<SettingsData>(File.ReadAllText(SavePath));
        }
        catch (Exception e)
        {
            Debug.LogError($"설정 로드 실패: {e.Message}");
            data = new SettingsData();
        }

        // 예전 설정 파일에는 이 필드가 없을 수 있음
        if (data.controlLayout == null)
            data.controlLayout = new List<ControlLayoutEntry>();
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {
            Debug.LogError($"설정 저장 실패: {e.Message}");
        }
    }
}