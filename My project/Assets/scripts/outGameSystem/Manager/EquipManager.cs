using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EquipManager : _Manager_Base
{
    public delegate void EquipChangedHandler(string updatedCategory, Sprite newSprite); //UIImageChangerにある
    public static event EquipChangedHandler OnEquipChanged;

    void Awake()
    {
        // 初期装備のロード処理
        activeBullet = Resources.Load<GameObject>("Objects/Reward/NormalBullet");
        activeCase = Resources.Load<GameObject>("Objects/Reward/NormalCase");
        activePrimer = Resources.Load<GameObject>("Objects/Reward/NormalPrimer");
    }

    void Update()
    {
        if (Input.GetKeyDown("3"))
        {
            tmpObj = activeBullet;
            activeBullet = subBullet;
            subBullet = tmpObj;
            EquipItemtoMain(activeBullet);
        }
        if (Input.GetKeyDown("2"))
        {
            tmpObj = activeCase;
            activeCase = subCase;
            subCase = tmpObj;
            EquipItemtoMain(activeCase);
        }
        if (Input.GetKeyDown("1"))
        {
            tmpObj = activePrimer;
            activePrimer = subPrimer;
            subPrimer = tmpObj;
            EquipItemtoMain(activePrimer);
        }
    }

    public void EquipItem(GameObject item)
    {
        // 装備が変更された際にイベントを発行
        string tmpCategory = item.GetComponent<ItemPickUp>().itemType;
        Sprite tmpSprite = item.GetComponent<SpriteRenderer>().sprite;

        bool subChanged = false;
        EquipStackDecide decideScript = gameObject.GetComponent<EquipStackDecide>();
        switch (tmpCategory)
        {
            case "Bullet":
                if (useMainEquip)
                    if (activeBullet == null)
                    {
                        activeBullet = item;
                        // 新しいスプライトを設定
                        imageChange(item);
                    }
                    else if (subBullet == null)
                        subBullet = item;
                    else
                        decideScript.StartSelection(activeBullet, subBullet, item);
                break;
            case "Case":
                if (useMainEquip)
                    if (activeCase == null)
                    {
                        // 新しいスプライトを設定

                        imageChange(item);
                        activeCase = item;
                    }
                    else if (subCase == null)
                        subCase = item;
                    else
                        decideScript.StartSelection(activeCase, subCase, item);
                break;
            case "Primer":
                if (useMainEquip)
                    if (activePrimer == null)
                    {
                        // 新しいスプライトを設定

                        imageChange(item);
                        activePrimer = item;
                    }
                    else if (subPrimer == null)
                        subPrimer = item;
                    else
                        decideScript.StartSelection(activePrimer, subPrimer, item);
                break;
            default:
                Debug.LogWarning("Invalid item type for equip");
                break;
        }
        if (subChanged == true)
        {
            return;
        }
    }

    public void EquipItemtoMain(GameObject item)
    {
        // 装備が変更された際にイベントを発行
        string tmpCategory = item.GetComponent<ItemPickUp>().itemType;
        Sprite tmpSprite = item.GetComponent<SpriteRenderer>().sprite;
        OnEquipChanged?.Invoke(tmpCategory, tmpSprite);
        bool subChanged = false;
        EquipStackDecide decideScript = gameObject.GetComponent<EquipStackDecide>();
        switch (tmpCategory)
        {
            case "Bullet":
                if (activeBullet != null)
                {
                    activeBullet = item;
                    // 新しいスプライトを設定
                    imageChange(item);
                }
                else { }

                break;
            case "Case":
                if (activeCase != null)
                {
                    activeCase = item;
                    // 新しいスプライトを設定
                    imageChange(item);
                }
                else
                {
                    //
                }
                break;
            case "Primer":
                if (activePrimer != null)
                {
                    activePrimer = item;
                    // 新しいスプライトを設定
                    imageChange(item);
                }
                else
                {
                    //  decideScript.StartSelection(activePrimer, subPrimer, item);
                }
                break;
            default:
                Debug.LogWarning("Invalid item type for equip");
                break;
        }
        if (subChanged == true)
        {
            return;
        }
    }

    public void EquipItemtoSub(GameObject item)
    {
        // 装備が変更された際にイベントを発行
        string tmpCategory = item.GetComponent<ItemPickUp>().itemType;
        Sprite tmpSprite = item.GetComponent<SpriteRenderer>().sprite;
        //  OnEquipChanged?.Invoke(tmpCategory, tmpSprite);
        bool subChanged = false;
        EquipStackDecide decideScript = gameObject.GetComponent<EquipStackDecide>();
        switch (tmpCategory)
        {
            case "Bullet":
                if (subBullet != null)
                {
                    subBullet = item;
                    // 新しいスプライトを設定
                    //   imageChange(item);
                }
                else { }

                break;
            case "Case":
                if (subCase != null)
                {
                    subCase = item;
                    // 新しいスプライトを設定
                    // imageChange(item);
                }
                else
                {
                    //
                }
                break;
            case "Primer":
                if (subPrimer != null)
                {
                    subPrimer = item;
                    // 新しいスプライトを設定
                    // imageChange(item);
                }
                else
                {
                    //  decideScript.StartSelection(activePrimer, subPrimer, item);
                }
                break;
            default:
                Debug.LogWarning("Invalid item type for equip");
                break;
        }
        if (subChanged == true)
        {
            return;
        }
    }

    public void imageChange(GameObject targetObj)
    {
        string tmpCategory = targetObj.GetComponent<ItemPickUp>().itemType;
        UIImageChanger[] imageChangers = FindObjectsOfType<UIImageChanger>();
        Sprite tmpSprite = targetObj.GetComponent<SpriteRenderer>().sprite;
        OnEquipChanged?.Invoke(tmpCategory, tmpSprite);
        foreach (UIImageChanger imageChanger in imageChangers)
        {
            if (imageChanger.itemCategory == tmpCategory)
            {
                imageChanger.newSprite = tmpSprite;
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
