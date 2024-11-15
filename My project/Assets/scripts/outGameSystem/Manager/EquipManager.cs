using System;
using UnityEngine;
using UnityEngine.UI;

public class EquipManager : MonoBehaviour
{
    public GameObject activeBullet;
    public GameObject activeCase;
    public GameObject activePrimer;

    public GameObject subBullet;
    public GameObject subCase;
    public GameObject subPrimer;

    private bool useMainEquip = true;
    public static event EquipChangedHandler OnEquipChanged;
    public delegate void EquipChangedHandler(string updatedCategory, Sprite newSprite);

    void Awake()
    {
        // 初期装備のロード処理
        activeBullet = Resources.Load<GameObject>("Objects/Reward/NormalBullet");
        activeCase = Resources.Load<GameObject>("Objects/Reward/NormalCase");
        activePrimer = Resources.Load<GameObject>("Objects/Reward/NormalPrimer");
    }

    public void EquipItem(GameObject item, Sprite newItemSprite, string category)
    {
        // 装備が変更された際にイベントを発行
        OnEquipChanged?.Invoke(category, newItemSprite);
        switch (category)
        {
            case "Bullet":
                if (useMainEquip)
                    activeBullet = item;
                else
                    subBullet = item;
                break;
            case "Case":
                if (useMainEquip)
                    activeCase = item;
                else
                    subCase = item;
                break;
            case "Primer":
                if (useMainEquip)
                    activePrimer = item;
                else
                    subPrimer = item;
                break;
            default:
                Debug.LogWarning("Invalid item type for equip");
                break;
        }
        // 新しいスプライトを設定
        UIImageChanger[] imageChangers = FindObjectsOfType<UIImageChanger>();
        foreach (UIImageChanger imageChanger in imageChangers)
        {
            if (imageChanger.itemCategory == category)
            {
                imageChanger.newSprite = newItemSprite;
            }
        }
    }

    public void ToggleEquip()
    {
        useMainEquip = !useMainEquip;
        Debug.Log(useMainEquip ? "Main Equip Active" : "Sub Equip Active");
    }

    public GameObject GetActiveBullet() => useMainEquip ? activeBullet : subBullet;

    public GameObject GetActiveCase() => useMainEquip ? activeCase : subCase;

    public GameObject GetActivePrimer() => useMainEquip ? activePrimer : subPrimer;
}
