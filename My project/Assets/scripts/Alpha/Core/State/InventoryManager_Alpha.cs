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
}