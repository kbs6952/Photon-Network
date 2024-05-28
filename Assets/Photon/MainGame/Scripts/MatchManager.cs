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

    // 3���� �̺�Ʈ, �÷��̾ �濡 ����������...�װ� ��� �÷��̾����� ����... ������ Update �����ϴ�.

    public enum EventCodes : byte       // �̸� ����Ʈ�� �̺�Ʈ�� �ڵ�� �ۼ��ϸ�, ���� ����ȯ���� �ʱ� ������ ������ �� �߻��Ѵ�.
    {
        NewPlayer,
        ListPlayer,
        UpdateStats,
    }

    private EventCodes eventCodes;

    [SerializeField] List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;          // �����.����� �� �ڽ��� �ε�����ȣ�� ����.

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
        //���濡 ������ �ȵǾ����������� �ε������
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
            Debug.Log("���Ź��� �̺�Ʈ�� ����" + eventCodes);

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
    public void ListPlayerSend()    // ������ Ŭ���̾�Ʈ�� ����ϰ� �ִ� ������ �ٸ� Ŭ���̾�Ʈ���� �ѷ��ִ� ���. PlayerInfo ��Ŷȭ�ؼ� ������ȴ�.
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
        allPlayers.Clear();     // ���� �����͸� ��������� ������

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
                // UpdateView Kill, Death Text ��ȭ�ϴ� �Լ�
                UpdateStatsDisPlay();
            }
            break;

        }
    }

    private void UpdateStatsDisPlay()
    {
        if (allPlayers.Count > index)
        {
            killText.text = $"ų �� : {allPlayers[index].kill}";
            deatText.text = $"���� �� : {allPlayers[index].death}";
        }
        else
        {
            killText.text = $"ų �� : 0";
            deatText.text = $"���� �� : 0";
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
