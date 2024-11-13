using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<GameObject> AmmoObjectList = new List<GameObject>();

    // Ammoをインベントリに追加
    public void AddAmmo(GameObject ammoData)
    {
        AmmoObjectList.Add(ammoData);
        ammoData.GetComponent<ItemPickUp>().itemRarelity = AmmoObjectList.IndexOf(ammoData);
        //        Debug.Log($"Added {newAmmo.ammoData.itemName} with rarity {newAmmo.rarity} to inventory.");
    }
}
