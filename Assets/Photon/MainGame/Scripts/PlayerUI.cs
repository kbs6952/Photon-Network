using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("���� �ý��� UI")]
    public GameObject overHeatTextObject;   // Canvas UI���� ������� �ʴ� ������Ʈ�� ��Ȱ��ȭ �ϴ� ���� �� ���ɻ� ȿ�����̴�.
    public Slider currentWeaponSlider;

    public WeaponSlot[] allWeaponSlots; 
    private int currentWeaponIndex;

    [Header("���� ȭ��")]
    public GameObject deathScreenObj;
    public TMP_Text deathText;

    [Header("�÷��̾�")]
    public TMP_Text playerHpText;

    public void ShowDeathMassage(string killer)
    {
        deathScreenObj.SetActive(true);
        deathText.text = $"�÷��̾ {killer}���� �׾����ϴ�.";
    }

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
    // Ŭ���� �����ڷ� �ٲ㼭...

    public Image weaponImage;
    public TMP_Text weaponNumber;
}
