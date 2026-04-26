using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSlotUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public int slotIndex;

        public Button button;

        public Text missionText;
        public Text abilityText;
        public Text toolText;
        public Text secretText;
    }

    public SlotUI[] slots;

    private void OnEnable()
    {
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        foreach (var slot in slots)
        {
            SaveData data = SaveManager.Load(slot.slotIndex);

            if (data == null)
            {
                slot.missionText.text = "ºó ½½·Ô";
                slot.abilityText.text = "";
                slot.toolText.text = "";
                slot.secretText.text = "";

                slot.button.interactable = false;
            }
            else
            {
                slot.missionText.text = data.missionText;
                slot.abilityText.text = data.abilityText;
                slot.toolText.text = data.toolText;
                slot.secretText.text =
                    $"ºñ¹Ð ¹ß°ß ÁøÇàµµ : {data.secretFound}/{data.secretTotal}";

                slot.button.interactable = true;
            }
        }
    }

    public void SelectSlot(int slotIndex)
    {
        if (!SaveManager.HasSave(slotIndex))
            return;

        GameSession.CurrentSlot = slotIndex;

        SceneManager.LoadScene("Main Lobby");
    }
}