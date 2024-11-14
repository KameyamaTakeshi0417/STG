using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.UI;
using System;
public class ItemPickUp : MonoBehaviour
{
    public ItemData itemData; // アイテムの情報を持つ ScriptableObject
    public string itemType;   // アイテムの種類（Bullet, Case, Primer）
    public int itemRarelity;
    public GameObject targetObj;

    public string accessAddress;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // プレイヤーに触れた場合、インベントリに追加し装備を更新
            EquipManager equipManager = GameObject.Find("GameManager").GetComponent<EquipManager>();
            InventoryManager inventoryManager=GameObject.Find("GameManager").GetComponent<InventoryManager>();
            if (equipManager != null)
            {
                GameObject targetObj=Resources.Load<GameObject>(accessAddress);
                equipManager.EquipItem(targetObj,targetObj.GetComponent<SpriteRenderer>().sprite, itemType);

                inventoryManager.AddAmmo(targetObj);

                Debug.Log("Picked up: " + itemData.itemName);
                 Destroy(gameObject);
            }

            // 自身を破壊
           
        }
    }

    public ItemPickUp(int rarelity)
    {
        itemRarelity = rarelity;
    }
}
