using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;



public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    // MonoBehaviourPunCallbacks : Photon Netkwork 상태에 따라 CallBack Interface함수를
    // 자동으로 등록하고 사용할 수 있게 해주는 클래스
    [Header("메인")]
    public GameObject menuButtons;
    public GameObject loadingPanel;
    public TMP_Text loadingText;
    public TMP_Text currentStatus;

    [Header("방 생성")]
    public GameObject createRoomPanel;    
    public TMP_InputField roomNameInput;

    [Header("방 정보")]
    public GameObject roomPanel;
    public GameObject startButton;              // 방장만 보이도록 설정
    public TMP_Text roomNameText;
    public TMP_Text playerNickNameText;

    [Header("방 검색")]
    public GameObject roomBroswerPanel;
    //public TMP_InputField roomBroswerNameText;

    [Header("방 에러 패널")]
    public GameObject errorPanel;    // 에러가 발생했을 때 활성화되는 오브젝트
    public TMP_Text errorText;       // 에러에 해당하는 내용을 출력하는 변수

    [Header("닉네임 생성 패널")]
    public GameObject nickNamePanel;                      // 닉네임 생성 오브젝트
    public TMP_InputField nickNameInput;                  // 닉네임 작성하는 공간
    private bool hasSetNick = false;                      // 닉네임이 지정이 되어 있으면 반복을 피해주기 위한 Bool type 변수
    private const string PLAYERNAMEKEY = "playerName";    // PlayerPrefs 사용. 유니티 제공하는 간단한 데이터 저장 방식

    [Header("Photon RoomInfo")]
    // 방을 생성했을 때 방의 이름을 데이터로 파싱하는 클래스 RoomButton
    public RoomButtonInfo theRoomButtonInfo;
    private List<RoomButtonInfo> roomButtonList = new List<RoomButtonInfo>();
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    [Header("Photon Chat")]
    public TMP_Text[] ChatText;
    public TMP_InputField ChatInput;
    public PhotonView PV;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        SetResolution();
    }

    private void SetResolution() => Screen.SetResolution(960, 540, false);

    private void Start()
    {
        PhotonNetwork.OfflineMode = false;
    }

    private void Update()
    {
        currentStatus.text = PhotonNetwork.NetworkClientState.ToString() + "\n" + "닉네임 : " + PhotonNetwork.NickName;
    }

    private void CloseMenus()
    {
        menuButtons.SetActive(false);
        loadingPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
        roomBroswerPanel.SetActive(false);
        errorPanel.SetActive(false);
        nickNamePanel.SetActive(false);
    }

    #region Photon Network Function

    // 버튼에 연결해줄 public 함수
    // 네트워크 상태가 변화했을 때 Call back 함수

    public void Connect() 
    {
        CloseMenus();
        menuButtons.SetActive(true);
        loadingPanel.SetActive(true);
        loadingText.text = "서버에 접속 중...";

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        CloseMenus();
        menuButtons.SetActive(true);
        // 로비에 접속
        PhotonNetwork.JoinLobby();

        loadingText.text = "로비에 접속..";

        PhotonNetwork.AutomaticallySyncScene = true; // Room Update 및 동기화 기능 허용
    }

    public void DisConnect() => PhotonNetwork.Disconnect();

    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if (!hasSetNick)
        {
            CloseMenus();
            nickNamePanel.SetActive(true);

            // PlayerPrefs로 이미 작성한 닉네임 불러오는 코드
            if (PlayerPrefs.HasKey(PLAYERNAMEKEY))
            {
                nickNameInput.text = PlayerPrefs.GetString(PLAYERNAMEKEY);
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString(PLAYERNAMEKEY);  // PlayerPrefs
        }
    }

    public void CreateRoomPanel()
    {
        CloseMenus();
        createRoomPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.LogWarning("방의 제목을 작성해주세요!");
            // 팝업창. 방 생성 경고 팝업
        }
        else
        {
            // 방의 제목, 방에 들어올 수 있는 인원수, 방장 호스트
            RoomOptions option = new RoomOptions();
            option.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomNameInput.text, option);

            // 방 생성 패널 닫아준다. 로딩 패널을 열어준다.
            CloseMenus();
            loadingText.text = "방 생성 중...";
            loadingPanel.SetActive(true);

            // 방 생성이 되고 난 이후의 코드는 RoomCreateCallBack
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenus();                                        // 다른 메뉴 전부 닫기
        errorText.text = $"방 생성에 실패함 : {message}";      // 에러 내용 변수에 입력
        errorPanel.SetActive(true);                          // 에러 오브젝트 활성화
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CloseMenus();                                           // 다른 메뉴 전부 닫기
        errorText.text = $"빠른 참여에 실패함 : {message}";      // 에러 내용 변수에 입력
        errorPanel.SetActive(true);                             // 에러 오브젝트 활성화
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();

        // 방 생성 패널 활성화
        roomPanel.SetActive(true);
        // 방의 제목 : InputField 데이터 TMP_Text
        roomNameText.text = $"방 제목 : {PhotonNetwork.CurrentRoom.Name}";
        // 방에 접속한 Client NickName 표시 되는 기능 Nick Name 표시할 LayOut 

        ShowListAllPlayer();

        // 플레이어가 방에 입장하셨습니다

        ChatClear();
        if (PhotonNetwork.IsMasterClient) startButton.SetActive(true);
        else startButton.SetActive(false);
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        CloseMenus();
        loadingText.text = "방 접속 중...";
        loadingPanel.SetActive(true);       
    }

    private void ShowListAllPlayer()
    {
        // allPlayernames에 들어있는 모든 플레이어를 전부 방에 보여주겠다.

        foreach(var player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for(int i = 0; i<players.Length; i++)
        {
            TMP_Text newPlayerNickName = Instantiate(playerNickNameText,
            playerNickNameText.transform.parent);
            newPlayerNickName.text = players[i].NickName;
            newPlayerNickName.gameObject.SetActive(true);

            allPlayerNames.Add(newPlayerNickName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerNickName = Instantiate(playerNickNameText,
            playerNickNameText.transform.parent);
        newPlayerNickName.text = newPlayer.NickName;
        newPlayerNickName.gameObject.SetActive(true);

        allPlayerNames.Add(newPlayerNickName);
        PV.RPC(nameof(ChatRPC), RpcTarget.All, "<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ShowListAllPlayer();
        PV.RPC(nameof(ChatRPC), RpcTarget.All, "<color=red>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    public void ButtonLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "방을 나가는 중...";
        loadingPanel.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        ButtonReturnLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(var roomButton in roomButtonList)
        {
            Destroy(roomButton.gameObject);
        }
        roomButtonList.Clear();
        theRoomButtonInfo.gameObject.SetActive(false);

        for(int i=0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButtonInfo newButton = Instantiate(theRoomButtonInfo, 
                    theRoomButtonInfo.transform.parent);  // Button을 복사해서, Content 오브젝트 저장
                newButton.SetButtonInfo(roomList[i]);
                newButton.gameObject.SetActive(true);

                roomButtonList.Add(newButton);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.IsMasterClient) startButton.SetActive(true);
            else startButton.SetActive(false);
        }
    }

    public void OpenRoomBroswer()
    {
        CloseMenus();
        // 방 검색 패널 활성화.
        roomBroswerPanel.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    #endregion

    private void JoinOrCreateRoom()
    {
        string roomName = $"No.{Random.Range(0, 1000).ToString()}";
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 8 }, null);
    }

    #region Button

    public void ButtonStartGame()
    {
        SceneManager.LoadScene("New Scene");
    }
    public void ButtonJoinRandomRoom()
    {
        // 서버 내에 방이 한개라도 존재하면.. 해당 방에 랜덤으로 참여한다.
        if(PhotonNetwork.CountOfRooms <= 0)
           JoinOrCreateRoom();
        // 서버 내에 방이 없다면... 내가 방을 만든다.
        else   
            PhotonNetwork.JoinRandomRoom();
    
    }

    public void ButtonSetNickName()
    {
        // Nickname 인풋필드가 비어있는지 확인
        if (!string.IsNullOrEmpty(nickNameInput.text))
        {
            PhotonNetwork.NickName = nickNameInput.text;
            // Playerprefs 닉네임을 저장해두었다가 사용하는 코드
            PlayerPrefs.SetString(PLAYERNAMEKEY, nickNameInput.text);
            CloseMenus();
            menuButtons.SetActive(true);
            hasSetNick = true;
        } 
    }

    public void ButtonReturnLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit(); // 게임을 빌드 해야지만 테스트를 할 수 있다.
    }
    #endregion

    #region Phton Chat
    private void ChatClear()
    {
        ChatInput.text = string.Empty;

        for(int i =0; i<ChatText.Length; i++)
        {
            ChatText[i].text = string.Empty;
        }
        foreach(var text in ChatText)
        {
            text.text = string.Empty;
        }
    }
   
    public void Send()
    {
        string massage =$"{PhotonNetwork.NickName} : {ChatInput.text}";
        PV.RPC(nameof(ChatRPC), RpcTarget.All, massage);
        ChatInput.text = string.Empty;
    }
    [PunRPC]
    private void ChatRPC(string massage)
    {
        bool isChatFull = false;

        for(int i = 0; i< ChatText.Length; i++)
        {
            if (ChatText[i].text == string.Empty)
            {
                ChatText[i].text = massage;
                isChatFull = true;
                break;
            }
        }
        if (!isChatFull)
        {
            for(int i =1; i< ChatText.Length; i++)      // int i = 1.. 배열의 2번쨰 요소부터 끝까지..
            {
                ChatText[i - 1].text = ChatText[i].text;
            }

            ChatText[ChatText.Length - 1].text = massage;
        }
    }

    #endregion
}
