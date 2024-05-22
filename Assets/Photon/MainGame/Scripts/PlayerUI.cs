using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("과열 tltmxpa UI")]
    public GameObject overHeatTextObject;   // Canvas UI에서 사용하지 않는 오브젝트는 비활성화 하는 것이 더 성능상 효율적이다.
    public Slider currentWeaponSlider;

    public WeaponSlot[] allWeaponSlots; 
    private int currentWeaponIndex;

    public void SetWeaponSlot(int weaponIndex)
    {
        currentWeaponIndex = weaponIndex;

        for(int i = 0; i < allWeaponSlots.Length; i++)
        {
            SetImageAlpha(allWeaponSlots[i].weaponImage,0.5f);
        }
        SetImageAlpha(allWeaponSlots[currentWeaponIndex].weaponImage,1.0f);
    }
    private void SetImageAlpha(Image image,float alphaValue)
    {
        Color color = image.color;
        color.a = alphaValue;
        image.color = color;

    }
}
[System.Serializable]
public struct WeaponSlot
{
    // 클래스 생성자로 바꿔서...

    public Image weaponImage;
    public TMP_Text weaponNumber;
}
