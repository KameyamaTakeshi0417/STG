using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public ItemData itemData; // アイテムの情報を持つ ScriptableObject
    public string itemType;   // アイテムの種類（Bullet, Case, Primer）
    public int itemRarelity;
    public GameObject bulletObj;
    public string accessAddress;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // プレイヤーに触れた場合、インベントリに追加し装備を更新
            EquipManager equipManager = GameObject.Find("GameManager").GetComponent<EquipManager>();
            if (equipManager != null)
            {
                equipManager.EquipItem(itemData, itemType);
                Debug.Log("Picked up: " + itemData.itemName);
            }

            // 自身を破壊
            Destroy(gameObject);
        }
    }
    public ItemPickUp(int rarelity)
    {
        itemRarelity = rarelity;
    }
}
