using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<AmmoInstance> acquiredAmmoList = new List<AmmoInstance>();

    // Ammoをインベントリに追加
    public void AddAmmo(ItemData ammoData, int rarity)
    {
        AmmoInstance newAmmo = new AmmoInstance(ammoData, rarity);
        acquiredAmmoList.Add(newAmmo);

        Debug.Log($"Added {newAmmo.ammoData.itemName} with rarity {newAmmo.rarity} to inventory.");
    }
}
