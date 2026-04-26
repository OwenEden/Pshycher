using UnityEngine;

public static class StageData
{
    public static bool IsUnlocked(int id)
    {
        return PlayerPrefs.GetInt($"StageUnlocked_{id}", id == 1 ? 1 : 0) == 1;
    }

    public static bool IsCleared(int id)
    {
        return PlayerPrefs.GetInt($"StageCleared_{id}", 0) == 1;
    }

    public static void ClearStage(int id)
    {
        PlayerPrefs.SetInt($"StageCleared_{id}", 1);
        PlayerPrefs.SetInt($"StageUnlocked_{id + 1}", 1);
    }
}