using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderBoardPlayer : MonoBehaviour
{
    public TMP_Text playerName, Kill, Death;
    
    public void SetPlayerInfo(string name,int kill, int death)
    {
        playerName.text = name;
        Kill.text=kill.ToString();
        Death.text=death.ToString();
    }
}
