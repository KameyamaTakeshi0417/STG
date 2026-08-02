using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Alpha.Data;

namespace Alpha.UI
{
    public class OrbOpeningUI_Alpha : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("中央でアニメーションさせるための親コンテナ（画面中央に配置）")]
        public Transform animationContainer;

        [Header("Prefabs")]
        [Tooltip("レアリティ毎のオーブUIプレハブ（Index 0: Common, 1: Uncommon, 2: Rare, 3: Divine）")]
        public GameObject[] orbPrefabs = new GameObject[4];

        private GameObject currentOrbInstance;

        public void PlayAnimation(OrbData_Alpha orbData, Action onComplete)
        {
            if (orbPrefabs == null || orbPrefabs.Length == 0 || animationContainer == null)
            {
                onComplete?.Invoke();
                return;
            }

            int qualityIndex = Mathf.Clamp(orbData.orbRarity - 1, 0, orbPrefabs.Length - 1);
            GameObject prefab = orbPrefabs[qualityIndex];

            if (prefab == null)
            {
                onComplete?.Invoke();
                return;
            }

            // 以前のものが残っていれば削除
            if (currentOrbInstance != null) Destroy(currentOrbInstance);

            // オーブのUIプレハブを生成
            currentOrbInstance = Instantiate(prefab, animationContainer);
            currentOrbInstance.transform.localPosition = Vector3.zero;
            currentOrbInstance.transform.localScale = Vector3.one;

            // アニメーション：グググ(Shake) -> パーン！(Scale)
            currentOrbInstance.transform.DOShakePosition(1.0f, strength: 30f, vibrato: 30)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    currentOrbInstance.transform.DOScale(3f, 0.2f).SetUpdate(true).SetEase(Ease.OutQuad);
                    
                    var images = currentOrbInstance.GetComponentsInChildren<Image>();
                    foreach (var img in images)
                    {
                        img.DOFade(0f, 0.2f).SetUpdate(true);
                    }

                    DOVirtual.DelayedCall(0.2f, () => {
                        Destroy(currentOrbInstance);
                        onComplete?.Invoke();
                    }).SetUpdate(true);
                });
        }
    }
}
