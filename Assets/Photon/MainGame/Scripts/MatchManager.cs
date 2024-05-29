using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager Instance;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public TMP_Text killText, deatText;

    // 3가지 이벤트, 플레이어가 방에 접속했을때...그걸 모든 플레이어한테 전송... 정보를 Update 갱신하다.

    public enum EventCodes : byte       // 미리 바이트로 이벤트를 코드로 작성하면, 따로 형변환하지 않기 때문에 에러가 덜 발생한다.
    {
        NewPlayer,
        ListPlayer,
        UpdateStats,
    }
    public enum GameState
    {
        Waiting,
        Playing,
        Ending,
    }

    private EventCodes eventCodes;

    [SerializeField] List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;          // 포톤뷰.이즈마인 나 자신의 인덱스번호를 저장.

    [Header("리더 보드")]
    public GameObject LeaderBoardPanel;
    public LeaderBoardPlayer instantLeaderBoard;
    private List<LeaderBoardPlayer> leaderBoardPlayers = new List<LeaderBoardPlayer>();

    [Header("엔딩")]
    public int killToWIn = 1;
    public float waitForEnding = 3f;
    public GameState gameState = GameState.Waiting;
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    void Start()
    {
        //포톤에 연결이 안되어있을때마다 로드씬실행
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);

        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (LeaderBoardPanel.activeInHierarchy)
            {
                LeaderBoardPanel.SetActive(false);
            }
            else
            {
                ShowLeaderBoard();
            }
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes eventCode = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            eventCodes = (EventCodes)photonEvent.Code;
            Debug.Log("수신받은 이벤트의 정보" + eventCodes);

            switch (eventCode)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayer:
                    ListPlayerReceive(data);
                    break;
                case EventCodes.UpdateStats:
                    UpdateStatsReceive(data);
                    break;
            }
        }
    }
    public void NewPlayerSend(string userName)
    {
        object[] playerInfo = new object[4] { userName, PhotonNetwork.LocalPlayer.ActorNumber, 0, 0 };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.MasterClient
        };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent((byte)EventCodes.NewPlayer, playerInfo, raiseEventOptions, sendOptions);

    }
    public void NewPlayerReceive(object[] data)
    {
        PlayerInfo playerInfo = new PlayerInfo((string)data[0], (int)data[1], (int)data[2], (int)data[3]);

        allPlayers.Add(playerInfo);

        ListPlayerSend();

    }
    public void ListPlayerSend()    // 마스터 클라이언트가 기억하고 있는 정보를 다른 클라이언트한테 뿌려주는 기능. PlayerInfo 패킷화해서 보내면된다.
    {
        object[] packet = new object[allPlayers.Count];

        packet[0] = gameState;

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] info = new object[4];

            info[0] = allPlayers[i].name;
            info[1] = allPlayers[i].actor;
            info[2] = allPlayers[i].kill;
            info[3] = allPlayers[i].death;

            packet[i+1] = info;
        }
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };


        PhotonNetwork.RaiseEvent((byte)EventCodes.ListPlayer, packet, raiseEventOptions, sendOptions);
    }
    public void ListPlayerReceive(object[] data)
    {
        allPlayers.Clear();     // 기존 데이터를 덮어쓰기위해 지워줌

        gameState = (GameState)data[0];

        for (int i = 1; i < data.Length; i++)
        {
            object[] info = (object[])data[i];

            PlayerInfo player = new PlayerInfo((string)info[0], (int)data[1], (int)data[2], (int)data[3]);

            allPlayers.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i-1;
                UpdateStatsDisPlay();
            }
        }

        StateCheck();
    }
    public void UpdateStatsSend(int actorIndex, int statToUpdate, int amountToChange)
    {
        object[] packet = new object[] { actorIndex, statToUpdate, amountToChange };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent((byte)EventCodes.UpdateStats, packet, raiseEventOptions, sendOptions);
    }
    public void UpdateStatsReceive(object[] data)
    {
        int actor = (int)data[0];
        int stat = (int)data[1];
        int amount = (int)data[2];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (stat)
                {
                    case 0:
                        allPlayers[i].kill += amount;
                        break;
                    case 1:
                        allPlayers[i].death += amount;
                        break;
                }
                if (i == index)
                {
                    // UpdateView Kill, Death Text 변화하는 함수
                    UpdateStatsDisPlay();
                }
                if (LeaderBoardPanel.activeInHierarchy)
                {
                    ShowLeaderBoard();
                }

                break;
            }
        }
        ScoreCheck();
    }

    private void UpdateStatsDisPlay()
    {
        if (allPlayers.Count > index)
        {
            killText.text = $"킬 수 : {allPlayers[index].kill}";
            deatText.text = $"데스 수 : {allPlayers[index].death}";
        }
        else
        {
            killText.text = $"킬 수 : 0";
            deatText.text = $"데스 수 : 0";
        }

    }
    void ShowLeaderBoard()
    {
        // 리더보드판넬 활성화
        LeaderBoardPanel.SetActive(false);
        foreach (var leaderBoardPlayer in leaderBoardPlayers)
        {
            Destroy(leaderBoardPlayer.gameObject);
        }
        leaderBoardPlayers.Clear();
        //데이터갱신
        instantLeaderBoard.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        foreach (var player in sorted)
        {
            // 리더보드 플레이어 클래스 <- 올 플레이어즈 안에 있는 데이터
            LeaderBoardPlayer leaderBoardPlayer = Instantiate(instantLeaderBoard, instantLeaderBoard.transform.parent);
            // 리더보드 플레이어 멤버 함수. Set 플레이어인포 실행
            leaderBoardPlayer.SetPlayerInfo(player.name, player.kill, player.death);
            // 리더보드 플레이어 객체를 리더보드 플레이어즈 리스트 추가.
            leaderBoardPlayers.Add(leaderBoardPlayer);
            // 객체를 SetActive 활성화한다.
            leaderBoardPlayer.gameObject.SetActive(true);
        }
    }
    private List<PlayerInfo> SortPlayers(List<PlayerInfo> allPlayers)
    {
        List<PlayerInfo> sortedList = new List<PlayerInfo>();

        // 받아온 리스트를 킬 수가 높은 순서대로 정렬한다.
        while (sortedList.Count < allPlayers.Count)
        {
            PlayerInfo selectedPlayer = allPlayers[0];
            int highest = -1;

            foreach (PlayerInfo player in allPlayers)
            {
                if (!sortedList.Contains(player))
                {
                    selectedPlayer = player;
                    highest = player.kill;
                }
            }
            sortedList.Add(selectedPlayer);
        }

        return sortedList;
    }
    #region 매칭종료

    void ScoreCheck()
    {
        bool isExistWinner = false;

        foreach (var player in allPlayers)
        {
            if (player.kill >= killToWIn && killToWIn > 0)
            {
                isExistWinner = true;
                break;
            }
        }
        // 체크한 유저가 있다면, 모든 유저에게 게임이 종료되었음을 알린다.
        if (isExistWinner)
        {
            if (PhotonNetwork.IsMasterClient && gameState != GameState.Ending)
            {
                gameState = GameState.Ending;
                ListPlayerSend();
            }
        }
        StartCoroutine(nameof(MatchEndCo));
    }
    void StateCheck()
    {
        if(gameState == GameState.Ending)
        {
            EndMatch();
        }
    }
    void EndMatch()
    {
        gameState = GameState.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            //PhotonNetwork.DestroyAll();
        }
        ShowLeaderBoard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private IEnumerator MatchEndCo()
    {
        yield return new WaitForSeconds(waitForEnding);
        PhotonNetwork.LeaveRoom(); 
    }
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(0);
    }

    #endregion
}


[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kill, death;

    public PlayerInfo(string name, int actor, int kill, int death)
    {
        this.name = name;
        this.actor = actor;
        this.kill = kill;
        this.death = death;
    }
}
