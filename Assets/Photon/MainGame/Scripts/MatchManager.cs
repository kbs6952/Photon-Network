using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MatchManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //���濡 ������ �ȵǾ����������� �ε������
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
    }
}
        
