using System.Collections.Generic;
using UnityEngine;

public class EquipManager : MonoBehaviour
{
    private ItemData activeBullet;  // メイン装備している弾丸
    public ItemData activeCase;    // メイン装備しているケース
    public ItemData activePrimer;  // メイン装備しているプライマー

    private ItemData subBullet;     // サブ装備している弾丸
    public ItemData subCase;       // サブ装備しているケース
    public ItemData subPrimer;     // サブ装備しているプライマー

    private bool useMainEquip = true; // 現在メイン装備を使用しているかどうか
    void Awake() // AwakeはMonoBehaviourのオブジェクトが生成された直後に呼ばれます
    {
        subBullet = Resources.Load<ItemData>("Assets/scripts/scriptableObject/Ammo/Bullet/NormalBullet");
        subCase = Resources.Load<ItemData>("scripts/scriptableObject/Ammo/NormalCase");
        subPrimer = Resources.Load<ItemData>("scripts/scriptableObject/Ammo/NormalPrimer");
        if (activeBullet == null)
        {
            activeBullet = Resources.Load<ItemData>("scripts/scriptableObject/Ammo/Bullet/NormalBullet");
            
        }
        if (activeCase == null)
        {
            activeCase = Resources.Load<ItemData>("scripts/scriptableObject/Ammo/NormalCase");
        }
        if (activePrimer == null)
        {
            activePrimer = Resources.Load<ItemData>("scripts/scriptableObject/Ammo/NormalPrimer");
        }
        if(activeBullet==null){Debug.Log("activeBullet Null");}
        if(subBullet==null){Debug.Log("subBullet Null");}
    }
    public void EquipItem(ItemData item, string type)
    {
        switch (type)
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
    }

    public void ToggleEquip()
    {
        useMainEquip = !useMainEquip;
        Debug.Log(useMainEquip ? "Main Equip Active" : "Sub Equip Active");
    }

    public ItemData GetActiveBullet()
    {
        return useMainEquip ? activeBullet : subBullet;
    }

    public ItemData GetActiveCase()
    {
        return useMainEquip ? activeCase : subCase;
    }

    public ItemData GetActivePrimer()
    {
        return useMainEquip ? activePrimer : subPrimer;
    }

}