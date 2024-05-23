using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPosition;
    private GameObject player;
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            Spawn();
        }
    }    
   private Transform GetSpawnPosition()
    {
        int randomIndex = Random.Range(0, spawnPosition.Length);
        return spawnPosition[randomIndex];
    }
    
    private void Spawn()
    {
        player = PhotonNetwork.Instantiate(playerPrefab.name, GetSpawnPosition().position, Quaternion.identity);
    }
}
