using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GunType
{
    OverHeat, 
    BulletType,
}

public class Gun : MonoBehaviour
{
    public bool isAutomatic;
    public float fireCoolTIme = 0.1f;
    public GameObject MuzzleFlash;

    private GunType gunType;
    private float maxHeat;
    public float heatPerShot;
    public float shootDistance;

    public int gunPower;
    public int gunFOV;

}
