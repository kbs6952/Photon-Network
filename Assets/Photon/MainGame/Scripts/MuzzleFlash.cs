using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public float muzzleCoolTime = 0.015f;
    private float muzzleCounter;

    private void OnEnable()     // �÷��̾ ���� Ȱ��ȭ ������
    {
        MuzzleReset();
    }

    private void MuzzleReset()
    {
        muzzleCounter = muzzleCoolTime;
    }

    private void Update()
    {      // Muzzle �ð��� ����ϴ� ����
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


    // Counter�� ���� CoolTime���� �ʱ�ȭ



    
}
