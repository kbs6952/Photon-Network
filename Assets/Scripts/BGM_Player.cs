using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM_Player : MonoBehaviour
{
    [SerializeField] private AudioSource[] allBGM;

    private int bgmIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        PlayRandomBGM();
    }

    public void PlayBGM(int index)
    {
        bgmIndex = index;

        StopAllBGM();

        
    }
    public void PlayRandomBGM()
    {
        int randomIndex = UnityEngine.Random.RandomRange(0, allBGM.Length);
        PlayBGM(randomIndex);
    }
    public void StopAllBGM()
    {
        foreach(var bgm in allBGM)
        {
            bgm.Stop();
        }
    }




}
