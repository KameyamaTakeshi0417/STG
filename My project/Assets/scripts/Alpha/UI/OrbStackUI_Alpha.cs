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
        public GameObject orbIconPrefab; // オーブのアイコンプレハブ

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

            if (currentQueue == null || orbIconPrefab == null || stackContainer == null) return;

            // キューの中身を配列にして一覧表示
            OrbData_Alpha[] orbs = currentQueue.ToArray();
            for (int i = 0; i < orbs.Length; i++)
            {
                GameObject icon = Instantiate(orbIconPrefab, stackContainer);
                activeIcons.Add(icon);
                
                // TODO: アイコンの色や画像をレアリティ(orbs[i].orbRarity)に合わせて変更する処理
                // Image img = icon.GetComponent<Image>();
                // img.color = GetRarityColor(orbs[i].orbRarity);
                
                // 一番上（i==0）のオーブはハイライトさせる等の演出が可能
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
