using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
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

    private EventCodes eventCodes;

    [SerializeField] List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;          // 포톤뷰.이즈마인 나 자신의 인덱스번호를 저장.

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
        UpdateStatsDisPlay();
    }
    public void ListPlayerSend()    // 마스터 클라이언트가 기억하고 있는 정보를 다른 클라이언트한테 뿌려주는 기능. PlayerInfo 패킷화해서 보내면된다.
    {
        object[] packet = new object[allPlayers.Count];

        for (int i = 0; i < allPlayers.Count; i++) 
        {
            object[] info = new object[4];

            info[0] = allPlayers[i].name;
            info[1] = allPlayers[i].actor;
            info[2] = allPlayers[i].kill;
            info[3] = allPlayers[i].death;

            packet[i] = info;
        }
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };


        PhotonNetwork.RaiseEvent((byte)EventCodes.ListPlayer, packet, raiseEventOptions, sendOptions);
    }
    public void ListPlayerReceive(object[] data) 
    {
        allPlayers.Clear();     // 기존 데이터를 덮어쓰기위해 지워줌

        for(int i = 0; i<data.Length; i++)
        {
            object[] info = (object[])data[i];

            PlayerInfo player = new PlayerInfo((string)info[0], (int)data[1], (int)data[2], (int)data[3]);

            allPlayers.Add(player);

            if(PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i;
            }
        }
    }
    public void UpdateStatsSend(int actorIndex,int statToUpdate,int amountToChange)
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

        for(int i=0; i< allPlayers.Count;i++)
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
            }
            if(i==index)
            {
                // UpdateView Kill, Death Text 변화하는 함수
                UpdateStatsDisPlay();
            }
            break;

        }
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
