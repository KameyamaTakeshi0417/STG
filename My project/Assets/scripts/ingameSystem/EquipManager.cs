using System.Collections.Generic;
using UnityEngine;

public class EquipManager : MonoBehaviour
{
    public GameObject activeBullet;  // メイン装備している弾丸
    public GameObject activeCase;    // メイン装備しているケース
    public GameObject activePrimer;  // メイン装備しているプライマー

    public GameObject subBullet;     // サブ装備している弾丸
    public GameObject subCase;       // サブ装備しているケース
    public GameObject subPrimer;     // サブ装備しているプライマー

    private bool useMainEquip = true; // 現在メイン装備を使用しているかどうか
    void Awake() // AwakeはMonoBehaviourのオブジェクトが生成された直後に呼ばれます
    {
        subBullet = Resources.Load<GameObject>("Objects/Reward/NormalBullet");
        subCase = Resources.Load<GameObject>("Objects/Reward/NormalBullet");
        subPrimer = Resources.Load<GameObject>("Objects/Reward/NormalBullet");
        if (activeBullet == null)
        {
            activeBullet = Resources.Load<GameObject>("Objects/Reward/NormalBullet");
            
        }
        if (activeCase == null)
        {
            activeCase = Resources.Load<GameObject>("Objects/Reward/NormalBullet");
        }
        if (activePrimer == null)
        {
            activePrimer = Resources.Load<GameObject>("Objects/Reward/NormalBullet");
        }
        if(activeBullet==null){Debug.Log("activeBullet Null");}
        if(subBullet==null){Debug.Log("subBullet Null");}
    }
    public void EquipItem(GameObject item, string type)
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

    public GameObject GetActiveBullet()
    {
        return useMainEquip ? activeBullet : subBullet;
    }

    public GameObject GetActiveCase()
    {
        return useMainEquip ? activeCase : subCase;
    }

    public GameObject GetActivePrimer()
    {
        return useMainEquip ? activePrimer : subPrimer;
    }

}