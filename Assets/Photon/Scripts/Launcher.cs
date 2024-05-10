using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEditor;
using TMPro;
using Photon.Realtime;
using UnityEngine.UIElements;
public class Launcher : MonoBehaviourPunCallbacks       // MonoBehaviourPunCallbacks : 포톤 네트워크 상태에 따라 콜백 인터페이스함수를 자동으로 등록하고 사용할수있게해주는 클래스
{


    [Header("메인")]
    public GameObject menuBtns;
    public GameObject loadingPanel;
    public TMP_Text loadingText;
    public TMP_Text currentStatus;

    [Header("방 생성")]
    public GameObject createRoomPanel;
    public TMP_InputField roomNameInput;

    [Header("방 정보")]
    public GameObject roomPanel;
    public TMP_Text roomNameText;
    public TMP_Text playerNickNameText;

    [Header("Photon RoomInfo")]
    // 방을 생성했을 때 방의 이름을 데이터로 파싱하는 클래스 RoomButton
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    private void Start()
    {
        
    }
    private void Update()
    {
        currentStatus.text = PhotonNetwork.NetworkClientState.ToString() + "\n" + "닉네임 :" + PhotonNetwork.NickName;
    }
    #region Photon Network Function

    // 버튼에 연결해줄 public 함수
    // 네트워크 상태가 변화했을 때 Call back 함수

    public void Connect()
    {
        CloseMenues();
        menuBtns.SetActive(true);
        loadingPanel.SetActive(true);
        loadingText.text = "서버에 접속 중....";

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        CloseMenues();
        menuBtns.SetActive(true);

        PhotonNetwork.JoinLobby();

        loadingText.text = "로비에 접속...";
    }
    public void DisConnect()
    {
        PhotonNetwork.Disconnect();
    }
   public override void OnJoinedLobby()
    {
        CloseMenues();
        menuBtns.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0,1000).ToString();
    }
    public void JoinRoom(RoomInfo roomInfo)
    {
       
    }
    public void CreateRoomPanel()
    {
        CloseMenues();
        createRoomPanel.SetActive(true);
    }
    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.LogWarning("방의 제목을 작성해주세요.");
        }
        else
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomNameInput.text, options);

            CloseMenues();
            loadingText.text = "방 생성 중....";
            loadingPanel.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenues();
        
        // 방 생성 패널 활성화
        roomPanel.SetActive(true);
        // 방 제목 : InputField 데이터
        roomNameText.text = $"방 제목 : {PhotonNetwork.CurrentRoom.Name}";
        // 방에 접속한 Client NickName 표시되는 기능, 닉네임 표시할 레이아웃
        playerNickNameText.text = PhotonNetwork.NickName;
        
    }
    public void OpenRoomBrowser()
    {
        CloseMenues();
    }
    public void CloseRoomBrowser()
    {
        CloseMenues();
        menuBtns.SetActive(true);
    }

    #endregion
    private void CloseMenues()
    {
        menuBtns.SetActive(false);
        loadingPanel.SetActive(false);
        createRoomPanel.SetActive(false);
    }

    #region Button
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
    #endregion
}
