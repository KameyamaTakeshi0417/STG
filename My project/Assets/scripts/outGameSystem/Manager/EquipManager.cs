using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EquipManager : _Manager_Base
{
    public delegate void EquipChangedHandler(
        string updatedCategory,
        string activateType,
        Sprite newSprite
    ); //UIImageChangerにある
    public static event EquipChangedHandler OnEquipChanged;
    public GameObject EquipRelic1,
        EquipRelic2,
        EquipRelic3;

    void Awake()
    {
        // 初期装備のロード処理
        if (activeBullet == null)
            activeBullet = Resources.Load<GameObject>("Objects/Reward/NormalBullet");
        if (activeCase == null)
            activeCase = Resources.Load<GameObject>("Objects/Reward/NormalCase");
        if (activePrimer == null)
            activePrimer = Resources.Load<GameObject>("Objects/Reward/NormalPrimer");
        if (EquipRelic1 != null)
        {
            EquipRelic1.GetComponent<_Relic_Base>().EquipEffect();
        }
        if (EquipRelic2 != null)
        {
            EquipRelic2.GetComponent<_Relic_Base>().EquipEffect();
        }
        if (EquipRelic3 != null)
        {
            EquipRelic3.GetComponent<_Relic_Base>().EquipEffect();
        }
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

    public void EquipRelic(GameObject TargetRelic) { }

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
                        imageChange(item, "active");
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

                        imageChange(item, "active");
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

                        imageChange(item, "active");
                        activePrimer = item;
                    }
                    else if (subPrimer == null)
                        subPrimer = item;
                    else
                        decideScript.StartSelection(activePrimer, subPrimer, item);
                break;
            case "Relic":
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
        string activateType = "active";
        string tmpCategory = item.GetComponent<ItemPickUp>().itemType;
        Sprite tmpSprite = item.GetComponent<SpriteRenderer>().sprite;
        OnEquipChanged?.Invoke(tmpCategory, activateType, tmpSprite);
        bool subChanged = false;
        EquipStackDecide decideScript = gameObject.GetComponent<EquipStackDecide>();
        switch (tmpCategory)
        {
            case "Bullet":
                if (activeBullet != null)
                {
                    activeBullet = item;
                    // 新しいスプライトを設定
                    imageChange(item, activateType);
                }
                else { }

                break;
            case "Case":
                if (activeCase != null)
                {
                    activeCase = item;
                    // 新しいスプライトを設定
                    imageChange(item, activateType);
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
                    imageChange(item, activateType);
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
        OnEquipChanged?.Invoke(tmpCategory, "sub", tmpSprite);
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

    public void EquipItemtoRelic(GameObject item, string setType)
    {
        // 装備が変更された際にイベントを発行
        string activateType = setType; //first,second,third
        string tmpCategory = item.GetComponent<ItemPickUp>().itemType;
        Sprite tmpSprite = item.GetComponent<SpriteRenderer>().sprite;
        OnEquipChanged?.Invoke(tmpCategory, activateType, tmpSprite);
        bool subChanged = false;
        EquipStackDecide decideScript = gameObject.GetComponent<EquipStackDecide>();
        _Relic_Base targetRelicScript = null;
        switch (activateType)
        {
            case "first":

                if (EquipRelic1 != null)
                    EquipRelic1.GetComponent<_Relic_Base>().UnEquipEffect();
                EquipRelic1 = item;
                targetRelicScript = EquipRelic1.GetComponent<_Relic_Base>();
                targetRelicScript.EquipEffect();
                // 新しいスプライトを設定
                imageChange(item, activateType);
                break;
            case "second":
                if (EquipRelic2 != null)
                    EquipRelic2.GetComponent<_Relic_Base>().UnEquipEffect();
                EquipRelic2 = item;
                targetRelicScript = EquipRelic2.GetComponent<_Relic_Base>();
                targetRelicScript.EquipEffect();
                // 新しいスプライトを設定
                imageChange(item, activateType);
                break;
            case "third":
                if (EquipRelic3 != null)
                    EquipRelic3.GetComponent<_Relic_Base>().UnEquipEffect();
                EquipRelic3 = item;
                targetRelicScript = EquipRelic3.GetComponent<_Relic_Base>();
                targetRelicScript.EquipEffect();
                // 新しいスプライトを設定
                imageChange(item, activateType);
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

    public void imageChange(GameObject targetObj, string activateType)
    {
        string tmpCategory = targetObj.GetComponent<ItemPickUp>().itemType;
        UIImageChanger[] imageChangers = FindObjectsOfType<UIImageChanger>();
        Sprite tmpSprite = targetObj.GetComponent<SpriteRenderer>().sprite;
        OnEquipChanged?.Invoke(tmpCategory, activateType, tmpSprite);
        //レリックの処理を入れる。同じレリックって枠が3つあるのでどうしようね
        foreach (UIImageChanger imageChanger in imageChangers)
        {
            if (imageChanger.itemCategory == tmpCategory)
            {
                if (imageChanger.itemCategory == "Relic")
                {
                    return; //プレイヤーに手ずから装備させようねえ
                }
                else
                {
                    imageChanger.newSprite = tmpSprite;
                }
            }
        }
    }

    public void ReplaseEquip(string targetType, string targetCategory, GameObject targetObj)
    {
        string tmpCategory = targetObj.GetComponent<ItemPickUp>().itemType;
        UIImageChanger[] imageChangers = FindObjectsOfType<UIImageChanger>();
        Sprite tmpSprite = targetObj.GetComponent<SpriteRenderer>().sprite;
        OnEquipChanged?.Invoke(tmpCategory, targetType, tmpSprite);

        switch (targetType)
        {
            case "active":
                switch (targetCategory)
                {
                    case "Bullet":
                        activeBullet = targetObj;
                        break;
                    case "Case":
                        activeCase = targetObj;
                        break;
                    case "Primer":
                        activePrimer = targetObj;
                        break;
                }
                break;
            case "sub":
                switch (targetCategory)
                {
                    case "Bullet":
                        subBullet = targetObj;
                        break;
                    case "Case":
                        subCase = targetObj;
                        break;
                    case "Primer":
                        subPrimer = targetObj;
                        break;
                }
                break;
            //レリック処理
            case "first":
                EquipRelic1 = targetObj;
                break;
            case "second":
                EquipRelic2 = targetObj;
                break;
            case "third":
                EquipRelic3 = targetObj;
                break;
            default:
                break;
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
