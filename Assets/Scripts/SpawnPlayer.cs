using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPosition;
    void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition.position,Quaternion.identity);
    }
}
