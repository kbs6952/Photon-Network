using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEditor;
using TMPro;
using Photon.Realtime;
using UnityEngine.UIElements;
public class Launcher : MonoBehaviourPunCallbacks       // MonoBehaviourPunCallbacks : ���� ��Ʈ��ũ ���¿� ���� �ݹ� �������̽��Լ��� �ڵ����� ����ϰ� ����Ҽ��ְ����ִ� Ŭ����
{


    [Header("����")]
    public GameObject menuBtns;
    public GameObject loadingPanel;
    public TMP_Text loadingText;
    public TMP_Text currentStatus;

    [Header("�� ����")]
    public GameObject createRoomPanel;
    public TMP_InputField roomNameInput;

    [Header("�� ����")]
    public GameObject roomPanel;
    public TMP_Text roomNameText;
    public TMP_Text playerNickNameText;

    [Header("Photon RoomInfo")]
    // ���� �������� �� ���� �̸��� �����ͷ� �Ľ��ϴ� Ŭ���� RoomButton
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    private void Start()
    {
        
    }
    private void Update()
    {
        currentStatus.text = PhotonNetwork.NetworkClientState.ToString() + "\n" + "�г��� :" + PhotonNetwork.NickName;
    }
    #region Photon Network Function

    // ��ư�� �������� public �Լ�
    // ��Ʈ��ũ ���°� ��ȭ���� �� Call back �Լ�

    public void Connect()
    {
        CloseMenues();
        menuBtns.SetActive(true);
        loadingPanel.SetActive(true);
        loadingText.text = "������ ���� ��....";

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        CloseMenues();
        menuBtns.SetActive(true);

        PhotonNetwork.JoinLobby();

        loadingText.text = "�κ� ����...";
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
            Debug.LogWarning("���� ������ �ۼ����ּ���.");
        }
        else
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomNameInput.text, options);

            CloseMenues();
            loadingText.text = "�� ���� ��....";
            loadingPanel.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenues();
        
        // �� ���� �г� Ȱ��ȭ
        roomPanel.SetActive(true);
        // �� ���� : InputField ������
        roomNameText.text = $"�� ���� : {PhotonNetwork.CurrentRoom.Name}";
        // �濡 ������ Client NickName ǥ�õǴ� ���, �г��� ǥ���� ���̾ƿ�
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
