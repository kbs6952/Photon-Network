using UnityEngine;
using Photon.Realtime;
using TMPro;

public class RoomButtonInfo : MonoBehaviour
{
    public TMP_Text buttonText;   // ���� �̸�

    private RoomInfo info;        // ���� ����

    public void SetButtonInfo(RoomInfo inputInfo)
    {
        info = inputInfo;

        buttonText.text = info.Name;
    }

    public void ButtonOpenRoom()
    {
        Launcher.Instance.JoinRoom(info);
    }
}
