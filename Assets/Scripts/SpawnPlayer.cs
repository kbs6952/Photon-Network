using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    public static SpawnPlayer instance;

    public GameObject playerPrefab;
    public Transform[] spawnPosition;
    private GameObject player;

    [Header("Respawn")]
    public GameObject deathEffect;
    public float respawnTime = 6f;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }
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
    public void Die()
    {
        StartCoroutine(nameof(DieCorutin));
    }

    private IEnumerator DieCorutin()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(respawnTime);

        PhotonNetwork.Destroy(player);
        Spawn();
    }



}
