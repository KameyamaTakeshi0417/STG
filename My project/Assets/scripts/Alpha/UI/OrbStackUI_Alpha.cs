using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Alpha.Data;

namespace Alpha.UI
{
    public class OrbStackUI_Alpha : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panel;
        public Transform stackContainer; // ScrollViewのContentなど
        
        [Header("Prefabs")]
        [Tooltip("レアリティ毎のオーブUIプレハブ（Index 0: Common, 1: Uncommon, 2: Rare, 3: Divine）")]
        public GameObject[] orbIconPrefabs = new GameObject[4];

        private List<GameObject> activeIcons = new List<GameObject>();
        private Queue<OrbData_Alpha> currentQueue;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void ShowStack(Queue<OrbData_Alpha> queue)
        {
            if (panel != null) panel.SetActive(true);
            
            currentQueue = queue;
            RefreshIcons();
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
            ClearIcons();
        }

        public void UpdateStackDisplay()
        {
            RefreshIcons();
        }

        private void RefreshIcons()
        {
            ClearIcons();

            if (currentQueue == null || orbIconPrefabs == null || stackContainer == null) return;

            // キューの中身を配列にして一覧表示
            OrbData_Alpha[] orbs = currentQueue.ToArray();
            for (int i = 0; i < orbs.Length; i++)
            {
                int qualityIndex = Mathf.Clamp(orbs[i].orbRarity - 1, 0, orbIconPrefabs.Length - 1);
                GameObject prefab = orbIconPrefabs[qualityIndex];

                if (prefab != null)
                {
                    GameObject icon = Instantiate(prefab, stackContainer);
                    activeIcons.Add(icon);
                }
            }
        }

        private void ClearIcons()
        {
            foreach (var icon in activeIcons)
            {
                if (icon != null) Destroy(icon);
            }
            activeIcons.Clear();
        }
    }
}
