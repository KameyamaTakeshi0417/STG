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

    GameObject tmpObj;

    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            tmpObj = activeBullet;
            activeBullet = subBullet;
            subBullet = tmpObj;
            EquipItem(activeBullet);
        }
        if (Input.GetKeyDown("2"))
        {
            tmpObj = activeCase;
            activeCase = subCase;
            subCase = tmpObj;
            EquipItem(activeCase);
        }
        if (Input.GetKeyDown("3"))
        {
            tmpObj = activePrimer;
            activePrimer = subPrimer;
            subPrimer = tmpObj;
            EquipItem(activePrimer);
        }
    }

    public void EquipItem(GameObject item)
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
                if (useMainEquip)
                    if (activeBullet == null)
                        activeBullet = item;
                    else if (subBullet == null)
                        subBullet = item;
                    else
                        decideScript.StartSelection(activeBullet, subBullet, item);

                subChanged = true;
                break;
            case "Case":
                if (useMainEquip)
                    if (activeCase == null)
                        activeCase = item;
                    else if (subCase == null)
                        subCase = item;
                    else
                        decideScript.StartSelection(activeCase, subCase, item);
                subChanged = true;
                break;
            case "Primer":
                if (useMainEquip)
                    if (activePrimer == null)
                        activePrimer = item;
                    else if (subPrimer == null)
                        subPrimer = item;
                    else
                        decideScript.StartSelection(activePrimer, subPrimer, item);
                subChanged = true;
                break;
            default:
                Debug.LogWarning("Invalid item type for equip");
                break;
        }
        if (subChanged == true)
        {
            return;
        }
        // 新しいスプライトを設定
        UIImageChanger[] imageChangers = FindObjectsOfType<UIImageChanger>();
        foreach (UIImageChanger imageChanger in imageChangers)
        {
            if (imageChanger.itemCategory == tmpCategory)
            {
                imageChanger.newSprite = tmpSprite;
            }
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
                }
                else { }

                break;
            case "Case":
                if (activeCase != null)
                {
                    activeCase = item;
                }
                else
                {
                    decideScript.StartSelection(activeCase, subCase, item);
                }
                break;
            case "Primer":
                if (activePrimer != null)
                {
                    activePrimer = item;
                }
                else
                {
                    decideScript.StartSelection(activePrimer, subPrimer, item);
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
        // 新しいスプライトを設定
        UIImageChanger[] imageChangers = FindObjectsOfType<UIImageChanger>();
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
