using UnityEngine;

[System.Serializable]	// 커스텀 클래스를 인스펙터 창에서 수정하기 위해서 추가
public class Dialogue
{
    [Tooltip("대사 치는 캐릭터 이름")]	// 캐릭터 이름을 inspector 창에 띄움
    public string name; // 캐릭터 이름

    [Tooltip("대사 내용")]
    public string[] context; // 배열이라 여러 대사를 담을 수 있음.
}
