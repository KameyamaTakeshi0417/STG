using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipUIManager : _Manager_Base
{
    public GameObject Relic1UI;
    public GameObject Relic2UI;
    public GameObject Relic3UI;
    public delegate void EquipChangedHandler(string updatedCategory, Sprite newSprite);
    public static event EquipChangedHandler OnEquipChanged;

    // Start is called before the first frame update
    protected override void Start()
    {
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false); // 初期状態で選択 UI を非表示にしておく
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { 
            
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
}