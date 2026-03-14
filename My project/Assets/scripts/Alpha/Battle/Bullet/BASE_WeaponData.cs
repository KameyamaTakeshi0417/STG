using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Alpha_Weapon", menuName = "Game_Alpha/Weapon")]

public class BASE_WeaponData_Alpha : ScriptableObject
{
    public string itemName;
    public float WeaponCategory; // 0:雷管 1:薬莢 2:発射体
    public float itemHP;
    public float itemPower;
    public float itemSpeed;

    public float itemLange;
    public float itemSpan;

    [Tooltip("発射する弾のプレハブ")]
    public GameObject bulletPrefab;

    public string flavorText1;
    public string flavorText2;
    public string flavorText3;

    
}
