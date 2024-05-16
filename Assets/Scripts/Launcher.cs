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

    // MonoBehaviourPunCallbacks : Photon Netkwork ���¿� ���� CallBack Interface�Լ���
    // �ڵ����� ����ϰ� ����� �� �ְ� ���ִ� Ŭ����
    [Header("����")]
    public GameObject menuButtons;
    public GameObject loadingPanel;
    public TMP_Text loadingText;
    public TMP_Text currentStatus;

    [Header("�� ����")]
    public GameObject createRoomPanel;    
    public TMP_InputField roomNameInput;

    [Header("�� ����")]
    public GameObject roomPanel;
    public GameObject startButton;              // ���常 ���̵��� ����
    public TMP_Text roomNameText;
    public TMP_Text playerNickNameText;

    [Header("�� �˻�")]
    public GameObject roomBroswerPanel;
    //public TMP_InputField roomBroswerNameText;

    [Header("�� ���� �г�")]
    public GameObject errorPanel;    // ������ �߻����� �� Ȱ��ȭ�Ǵ� ������Ʈ
    public TMP_Text errorText;       // ������ �ش��ϴ� ������ ����ϴ� ����

    [Header("�г��� ���� �г�")]
    public GameObject nickNamePanel;                      // �г��� ���� ������Ʈ
    public TMP_InputField nickNameInput;                  // �г��� �ۼ��ϴ� ����
    private bool hasSetNick = false;                      // �г����� ������ �Ǿ� ������ �ݺ��� �����ֱ� ���� Bool type ����
    private const string PLAYERNAMEKEY = "playerName";    // PlayerPrefs ���. ����Ƽ �����ϴ� ������ ������ ���� ���

    [Header("Photon RoomInfo")]
    // ���� �������� �� ���� �̸��� �����ͷ� �Ľ��ϴ� Ŭ���� RoomButton
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
        currentStatus.text = PhotonNetwork.NetworkClientState.ToString() + "\n" + "�г��� : " + PhotonNetwork.NickName;
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

    // ��ư�� �������� public �Լ�
    // ��Ʈ��ũ ���°� ��ȭ���� �� Call back �Լ�

    public void Connect() 
    {
        CloseMenus();
        menuButtons.SetActive(true);
        loadingPanel.SetActive(true);
        loadingText.text = "������ ���� ��...";

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        CloseMenus();
        menuButtons.SetActive(true);
        // �κ� ����
        PhotonNetwork.JoinLobby();

        loadingText.text = "�κ� ����..";

        PhotonNetwork.AutomaticallySyncScene = true; // Room Update �� ����ȭ ��� ���
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

            // PlayerPrefs�� �̹� �ۼ��� �г��� �ҷ����� �ڵ�
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
            Debug.LogWarning("���� ������ �ۼ����ּ���!");
            // �˾�â. �� ���� ��� �˾�
        }
        else
        {
            // ���� ����, �濡 ���� �� �ִ� �ο���, ���� ȣ��Ʈ
            RoomOptions option = new RoomOptions();
            option.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomNameInput.text, option);

            // �� ���� �г� �ݾ��ش�. �ε� �г��� �����ش�.
            CloseMenus();
            loadingText.text = "�� ���� ��...";
            loadingPanel.SetActive(true);

            // �� ������ �ǰ� �� ������ �ڵ�� RoomCreateCallBack
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenus();                                        // �ٸ� �޴� ���� �ݱ�
        errorText.text = $"�� ������ ������ : {message}";      // ���� ���� ������ �Է�
        errorPanel.SetActive(true);                          // ���� ������Ʈ Ȱ��ȭ
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CloseMenus();                                           // �ٸ� �޴� ���� �ݱ�
        errorText.text = $"���� ������ ������ : {message}";      // ���� ���� ������ �Է�
        errorPanel.SetActive(true);                             // ���� ������Ʈ Ȱ��ȭ
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();

        // �� ���� �г� Ȱ��ȭ
        roomPanel.SetActive(true);
        // ���� ���� : InputField ������ TMP_Text
        roomNameText.text = $"�� ���� : {PhotonNetwork.CurrentRoom.Name}";
        // �濡 ������ Client NickName ǥ�� �Ǵ� ��� Nick Name ǥ���� LayOut 

        ShowListAllPlayer();

        // �÷��̾ �濡 �����ϼ̽��ϴ�

        ChatClear();
        if (PhotonNetwork.IsMasterClient) startButton.SetActive(true);
        else startButton.SetActive(false);
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        CloseMenus();
        loadingText.text = "�� ���� ��...";
        loadingPanel.SetActive(true);       
    }

    private void ShowListAllPlayer()
    {
        // allPlayernames�� ����ִ� ��� �÷��̾ ���� �濡 �����ְڴ�.

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
        PV.RPC(nameof(ChatRPC), RpcTarget.All, "<color=yellow>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ShowListAllPlayer();
        PV.RPC(nameof(ChatRPC), RpcTarget.All, "<color=red>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }

    public void ButtonLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "���� ������ ��...";
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
                    theRoomButtonInfo.transform.parent);  // Button�� �����ؼ�, Content ������Ʈ ����
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
        // �� �˻� �г� Ȱ��ȭ.
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
        // ���� ���� ���� �Ѱ��� �����ϸ�.. �ش� �濡 �������� �����Ѵ�.
        if(PhotonNetwork.CountOfRooms <= 0)
           JoinOrCreateRoom();
        // ���� ���� ���� ���ٸ�... ���� ���� �����.
        else   
            PhotonNetwork.JoinRandomRoom();
    
    }

    public void ButtonSetNickName()
    {
        // Nickname ��ǲ�ʵ尡 ����ִ��� Ȯ��
        if (!string.IsNullOrEmpty(nickNameInput.text))
        {
            PhotonNetwork.NickName = nickNameInput.text;
            // Playerprefs �г����� �����صξ��ٰ� ����ϴ� �ڵ�
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

        Application.Quit(); // ������ ���� �ؾ����� �׽�Ʈ�� �� �� �ִ�.
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
            for(int i =1; i< ChatText.Length; i++)      // int i = 1.. �迭�� 2���� ��Һ��� ������..
            {
                ChatText[i - 1].text = ChatText[i].text;
            }

            ChatText[ChatText.Length - 1].text = massage;
        }
    }

    #endregion
}
