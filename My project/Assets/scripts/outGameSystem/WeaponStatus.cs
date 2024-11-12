using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewItemData", menuName = "Game/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int itemRarelity;
    public float itemHP;
    public float itemPower;
    public float itemSpeed;

    public float itemDamage;
    public float itemLange;
    public float itemSpan;
    public GameObject itemPrefab; // 必要に応じて参照できるようにする
    public GameObject itemUI;

    public string flavorText1;
    public string flavorText2;
    public string flavorText3;
}
