using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 武器効果の種類を区別するEnum（今後効果を増やす場合はここに追加）
public enum Alpha_EffectType
{
    None = 0,
    SampleEffect = 1,
    Explosion = 2,
    Homing = 3,
    Volt = 4
}

[CreateAssetMenu(fileName = "Alpha_Weapon", menuName = "Game_Alpha/Weapon")]
public class BASE_WeaponData_Alpha : ScriptableObject
{
    public string itemName;
    public Alpha_EffectType effectType; // このアイテムが持つ効果
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
