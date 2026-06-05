using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipUIManager_Alpha : MonoBehaviour
{
    public InventoryManager_Alpha inventoryManager;
    public Image[] equipUIs;
    public Sprite emptyImage;
    [Header("Highlight Settings")]
    [Tooltip("アクティブな武器スロットを強調する色")]
    public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
    [Tooltip("非アクティブ時の色")]
    public Color normalColor = Color.white;
    [Tooltip("アイコン自身ではなく、親オブジェクト（枠やマスク）の色を変更するかどうか")]
    public bool changeParentColor = true;

    private Player_Shooter_Alpha playerShooter;

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
    void Update()
    {
        if (playerShooter == null)
        {
            playerShooter = FindAnyObjectByType<Player_Shooter_Alpha>();
            if (playerShooter == null) return;
        }

        int activeGroup = playerShooter.currentWeaponGroup;

        for (int i = 0; i < equipUIs.Length; i++)
        {
            if (equipUIs[i] == null) continue;

            // スロットのインデックスから所属グループを判定
            // 9枠ある場合は3枠ごとにグループが切り替わる(0,1,2=Group0 / 3,4,5=Group1)
            // 3枠しかない場合はそのままのインデックスがグループになる
            int groupIndex = (equipUIs.Length == 3) ? i : (i / 3);
            bool isActive = (groupIndex == activeGroup);

            // 色を変更する対象のImageを取得
            Image targetImage = equipUIs[i];
            if (changeParentColor && equipUIs[i].transform.parent != null)
            {
                Image parentImage = equipUIs[i].transform.parent.GetComponent<Image>();
                if (parentImage != null)
                {
                    targetImage = parentImage;
                }
            }

            if (targetImage != null)
            {
                targetImage.color = isActive ? highlightColor : normalColor;
            }
        }
    }
}
