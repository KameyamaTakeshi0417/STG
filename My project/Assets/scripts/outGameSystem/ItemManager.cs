using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<ItemData> itemList = new List<ItemData>();

    // アイテムをリストに追加
    public void AddItemData(ItemData newItemData)
    {
        if (newItemData != null)
        {
            itemList.Add(newItemData);
            Debug.Log("Item data added: " + newItemData.itemName);
        }
    }

    // プレハブを参照してUI上にアイテムを生成
    public GameObject CreateItemFromData(ItemData itemData)
    {
        if (itemData != null && itemData.itemPrefab != null)
        {
            GameObject newItem = Instantiate(itemData.itemPrefab);
            newItem.name = itemData.itemName; // 名前をデータに合わせる
            Debug.Log("Item created from data: " + itemData.itemName);
            return newItem;
        }
        else
        {
            Debug.LogWarning("ItemData or itemPrefab is missing.");
        }
        return null;
    }

    // アイテムデータを使用してUI上に生成するタイミング
    public void DisplayItemInUI(ItemData itemData)
    {
        GameObject uiItem = CreateItemFromData(itemData);
        if (uiItem != null)
        {
            // UI Canvas の子オブジェクトとして配置するなどの処理を追加
            uiItem.transform.SetParent(GameObject.Find("UICanvas").transform, false);
        }
    }
}
