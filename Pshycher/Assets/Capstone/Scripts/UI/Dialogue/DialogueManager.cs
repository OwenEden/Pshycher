using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject go_dialogueBar;
    [SerializeField] GameObject go_NameBar;

    [SerializeField] TMP_Text txt_dialogue;
    [SerializeField] TMP_Text txt_name;

    bool isDialogue = false;    // 현재 대화중인지
    bool isNext = false;    // 특정 키 입력 대기를 위한 변수
    int dialogueCnt = 0;    // 대화 카운트. 한 캐릭터가 다 말하면 1 증가
    int contextCnt = 0; 	// 대사 카운트. 한 캐릭터가 여러 대사를 할 수 있다.

    public void ShowDialogue()
    {
        txt_dialogue.text = "";
        txt_name.text = "";

        SettingUI(true);
    }

    void SettingUI(bool p_flag)
    {
        go_dialogueBar.SetActive(p_flag);
        go_NameBar.SetActive(p_flag);
    }
}
