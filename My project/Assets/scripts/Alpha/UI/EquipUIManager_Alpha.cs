using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipUIManager_Alpha : MonoBehaviour
{
    public InventoryManager_Alpha inventoryManager;
    public Image[] equipUIs;
    public Sprite emptyImage;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < equipUIs.Length; i++)
        {
            if (equipUIs[i] == null) continue;

            if (inventoryManager != null && i < inventoryManager.equipInstance.Count)
            {
                var instance = inventoryManager.equipInstance[i];
                if (instance.series != null && instance.series.icon != null)
                {
                    equipUIs[i].sprite = instance.series.icon;
                    equipUIs[i].enabled = true; // アイコンがあれば表示
                }
                else
                {
                    equipUIs[i].sprite = emptyImage;
                }
            }
        }
    }

    // Update is called once per frame

}
