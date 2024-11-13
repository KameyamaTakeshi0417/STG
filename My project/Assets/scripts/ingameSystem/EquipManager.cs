using System.Collections.Generic;
using UnityEngine;

public class EquipManager : MonoBehaviour
{
    public ItemData activeBullet;  // 実際にプレイヤーに反映されるBulletデータ
    public ItemData stockBullet;   // ストックとして保持するBulletデータ

    public ItemData activeCase;    // 実際にプレイヤーに反映されるCaseデータ
    public ItemData stockCase;     // ストックとして保持するCaseデータ

    public ItemData activePrimer;  // 実際にプレイヤーに反映されるPrimerデータ
    public ItemData stockPrimer;   // ストックとして保持するPrimerデータ

    public float totalPower;  // 装備による加算後のパワー
    public float totalSpeed;  // 装備による加算後のスピード

    public ItemManager itemManager; // ItemManagerへの参照

    void Start()
    {
        // ItemManagerの参照を取得
        itemManager = gameObject.GetComponent<ItemManager>();
        if (itemManager == null)
        {
            Debug.LogError("ItemManagerが見つかりません。GameManagerに正しくアタッチされているか確認してください。");
            return;
        }

        // デフォルト装備の設定
        SetDefaultEquipments();

        CalculateStats(); // 装備状態をもとにステータスを計算
    }

    private void SetDefaultEquipments()
    {
        // Bulletが装備されていない場合、Normal_Bulletを装備
        if (activeBullet == null)
        {
            ItemData normalBulletItem = itemManager.itemList.Find(item => item.itemName == "Normal_Bullet" && item.itemRarelity == 1);
            if (normalBulletItem != null)
            {
                activeBullet = normalBulletItem;
            }
            else
            {
                Debug.LogWarning("Normal_BulletがItemManagerのリストに見つかりませんでした。新規に作成します。");
                activeBullet = CreateDefaultItem("Normal_Bullet", 1);
            }
        }

        // Caseが装備されていない場合、Normal_Caseを装備
        if (activeCase == null)
        {
            ItemData normalCaseItem = itemManager.itemList.Find(item => item.itemName == "Normal_Case" && item.itemRarelity == 1);
            if (normalCaseItem != null)
            {
                activeCase = normalCaseItem;
            }
            else
            {
                Debug.LogWarning("Normal_CaseがItemManagerのリストに見つかりませんでした。新規に作成します。");
                activeCase = CreateDefaultItem("Normal_Case", 1);
            }
        }

        // Primerが装備されていない場合、Normal_Primerを装備
        if (activePrimer == null)
        {
            ItemData normalPrimerItem = itemManager.itemList.Find(item => item.itemName == "Normal_Primer" && item.itemRarelity == 1);
            if (normalPrimerItem != null)
            {
                activePrimer = normalPrimerItem;
            }
            else
            {
                Debug.LogWarning("Normal_PrimerがItemManagerのリストに見つかりませんでした。新規に作成します。");
                activePrimer = CreateDefaultItem("Normal_Primer", 1);
            }
        }
    }

    // デフォルトのアイテムを作成するメソッド
    private ItemData CreateDefaultItem(string name, int rarity)
    {
        ItemData newItem = ScriptableObject.CreateInstance<ItemData>();
        newItem.itemName = name;
        newItem.itemRarelity = rarity;
        newItem.itemHP = 10; // 適切な初期値を設定してください
        newItem.itemPower = 5;
        newItem.itemSpeed = 2;

        Debug.Log($"新規デフォルトアイテム {name} を作成しました。");
        return newItem;
    }

    public void EquipItem(ItemData item, string type)
    {
        switch (type)
        {
            case "Bullet":
                if (item != null && item.itemPrefab.GetComponent<Bullet_Base>() != null)
                {
                    stockBullet = activeBullet;  // 現在のアクティブ装備をストックに移動
                    activeBullet = item;         // 新しいアイテムをアクティブ装備に
                }
                break;
            case "Case":
                if (item != null && item.itemPrefab.GetComponent<Case_Base>() != null)
                {
                    stockCase = activeCase;      // 現在のアクティブ装備をストックに移動
                    activeCase = item;           // 新しいアイテムをアクティブ装備に
                }
                break;
            case "Primer":
                if (item != null && item.itemPrefab.GetComponent<Primer_Base>() != null)
                {
                    stockPrimer = activePrimer;  // 現在のアクティブ装備をストックに移動
                    activePrimer = item;         // 新しいアイテムをアクティブ装備に
                }
                break;
            default:
                Debug.LogWarning("Invalid item type");
                break;
        }

        CalculateStats(); // 新しい装備に応じてステータスを再計算
    }

    private void CalculateStats()
    {
        // 初期化
        totalPower = 0;
        totalSpeed = 0;

        // アクティブなBullet, Case, Primerのステータスを加算
        if (activeBullet != null)
        {
            Bullet_Base bulletScript = activeBullet.itemPrefab.GetComponent<Bullet_Base>();
            if (bulletScript != null)
            {
                totalPower += bulletScript.getDmg();
                totalSpeed += bulletScript.getSpeed();
            }
        }

        if (activeCase != null)
        {
            Case_Base caseScript = activeCase.itemPrefab.GetComponent<Case_Base>();
            if (caseScript != null)
            {
                totalPower += caseScript.getDmg();
                totalSpeed += caseScript.getSpeed();
            }
        }

        if (activePrimer != null)
        {
            Primer_Base primerScript = activePrimer.itemPrefab.GetComponent<Primer_Base>();
            if (primerScript != null)
            {
                totalPower += primerScript.getDmg();
                totalSpeed += primerScript.getSpeed();
            }
        }

        Debug.Log("Total Power: " + totalPower);
        Debug.Log("Total Speed: " + totalSpeed);
    }

    public ItemData GetActiveCase() { return activeCase; }
}
