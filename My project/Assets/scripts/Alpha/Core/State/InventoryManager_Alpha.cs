using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager_Alpha : MonoBehaviour
{
    public const int W = 3;
    public const int H = 3;

    [Serializable]
    public struct EquipInstance
    {
        public string defId;
        public int rarity;
        public BASE_WeaponData_Alpha affix; // ここは型に注意（下で説明）
        public Alpha_Effect_Base effect1;
        public Alpha_Effect_Base effect2;
        public Alpha_Effect_Base effect3;
    }

    [SerializeField] private EquipInstance[] equipInstance = new EquipInstance[W * H];

    public EquipInstance Get(int x, int y) => equipInstance[y * W + x];
    public void Set(int x, int y, EquipInstance v) => equipInstance[y * W + x] = v;

#if UNITY_EDITOR
    void OnValidate()
        {
            if (equipInstance == null || equipInstance.Length != W * H)
                equipInstance = new EquipInstance[W * H];
        }
#endif
    public void BattleStartEffect()
    {
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {

                EquipInstance instance = Get(x, y);
                // スロットが未設定（defId が null または空）の場合はスキップ
                if (string.IsNullOrEmpty(instance.defId))
                    continue;

                instance.effect1?.StartEffect(instance.rarity);
                instance.effect2?.StartEffect(instance.rarity);
                instance.effect3?.StartEffect(instance.rarity);
            }
        }
    }
}