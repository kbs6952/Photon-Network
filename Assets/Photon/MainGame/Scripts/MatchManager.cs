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
        //포톤에 연결이 안되어있을때마다 로드씬실행
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
    }
}
        
