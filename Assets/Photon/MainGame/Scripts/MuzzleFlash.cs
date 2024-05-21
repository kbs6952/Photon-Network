using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public float muzzleCoolTime = 0.015f;
    private float muzzleCounter;

    private void OnEnable()     // 플레이어에 의해 활성화 됐을때
    {
        MuzzleReset();
    }

    private void MuzzleReset()
    {
        muzzleCounter = muzzleCoolTime;
    }

    private void Update()
    {      // Muzzle 시간을 계산하는 로직
        if (gameObject.activeSelf)
        {
            muzzleCounter -=Time.deltaTime;

            if(muzzleCounter <= 0)
            {
                gameObject.SetActive(false);
            }
        }


            //muzzleCounter -= Time.deltaTime;
            //if (Input.GetMouseButton(0))
            //{
            //if (muzzleCounter <= 0)
            //    muzzleCounter = muzzleCoolTime;
            //}
        
    }


    // Counter의 값을 CoolTime으로 초기화



    
}
