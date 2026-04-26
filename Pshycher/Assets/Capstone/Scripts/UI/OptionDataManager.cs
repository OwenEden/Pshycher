using UnityEngine;
using System.IO;

public class OptionDataManager : MonoBehaviour
{
    public static OptionDataManager Instance { get; private set; }

    private string fileName = "Option.json";
    public OptionData optionData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadOptionData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadOptionData()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            optionData = JsonUtility.FromJson<OptionData>(json);
        }
        else
        {
            ResetOptionData();
        }
    }

    public void SaveOptionData()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        string json = JsonUtility.ToJson(optionData, true);
        File.WriteAllText(path, json);
    }

    public void ResetOptionData()
    {
        optionData = new OptionData();
        SaveOptionData();
    }
}