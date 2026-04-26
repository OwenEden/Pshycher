using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera c_VCam;
    
    void Start()
    {

    }

    // Update is called once per frame
/*    void Update()
    {
        if (GameManager.instance.isConversation)
        {
            c_VCam.m_Lens.OrthographicSize = 10;
        }
        else
        {
            c_VCam.m_Lens.OrthographicSize = 20;
        }

    }*/
}
