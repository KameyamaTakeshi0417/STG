using UnityEngine;
using UnityEditor;

public class DebugInventory
{
    [MenuItem("Tools/Debug Inventory Icons")]
    public static void DebugIcons()
    {
        if (InventoryManager_Alpha.Instance == null)
        {
            Debug.LogError("InventoryManager_Alpha.Instance is null. Play the game first!");
            return;
        }

        var list = InventoryManager_Alpha.Instance.equipInstance;
        Debug.Log($"Inventory has {list.Count} items.");
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            bool hasSeries = item.series != null;
            bool hasIcon = hasSeries && item.series.icon != null;
            string seriesName = hasSeries ? item.series.name : "null";
            string iconName = hasIcon ? item.series.icon.name : "null";
            
            Debug.Log($"Slot {i}: defId={item.defId}, series={seriesName}, icon={iconName}");
        }
    }
}
