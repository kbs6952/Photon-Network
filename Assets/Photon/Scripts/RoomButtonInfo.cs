using UnityEngine;
using Photon.Realtime;
using TMPro;

public class RoomButtonInfo : MonoBehaviour
{
    public TMP_Text buttonText;   // 방의 이름

    private RoomInfo info;        // 방의 정보

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
