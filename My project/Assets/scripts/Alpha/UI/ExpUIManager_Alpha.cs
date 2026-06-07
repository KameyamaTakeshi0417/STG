using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Alpha.UI
{
    public class ExpUIManager_Alpha : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("合計経験値を表示するテキスト")]
        public TextMeshProUGUI totalExpText;
        
        [Tooltip("ポップアップテキストのプレハブ（DamageTextなどと同じ構造のもの）")]
        public GameObject expPopupPrefab;
        
        [Tooltip("ポップアップを生成する親オブジェクト（指定がなければこのオブジェクト）")]
        public RectTransform popupContainer;

        private void OnEnable()
        {
            playerStatusManager_Alpha.OnExpAdded += HandleExpAdded;
            UpdateTotalExpText();
        }

        private void OnDisable()
        {
            playerStatusManager_Alpha.OnExpAdded -= HandleExpAdded;
        }

        private void Start()
        {
            if (popupContainer == null)
            {
                popupContainer = GetComponent<RectTransform>();
            }
            UpdateTotalExpText();
        }

        private void HandleExpAdded(int amount)
        {
            UpdateTotalExpText();
            ShowPopup(amount);
        }

        private void UpdateTotalExpText()
        {
            if (totalExpText != null && playerStatusManager_Alpha.Instance != null)
            {
                totalExpText.text = playerStatusManager_Alpha.Instance.currentExp.ToString();
            }
        }

        private void ShowPopup(int amount)
        {
            if (expPopupPrefab == null || popupContainer == null) return;

            // ポップアップを生成
            GameObject popup = Instantiate(expPopupPrefab, popupContainer);
            
            // 新規作成したPopUpUITextスクリプトがアタッチされているか確認
            PopUpUIText popupScript = popup.GetComponent<PopUpUIText>();
            if (popupScript != null)
            {
                // PopUpUITextがアタッチされていれば、値を渡してアニメーションはスクリプトに任せる
                popupScript.Setup(amount);
                return;
            }

            // --- 互換性のため、PopUpUITextがない場合の処理を残す ---
            RectTransform rt = popup.GetComponent<RectTransform>();
            TextMeshProUGUI popupText = popup.GetComponent<TextMeshProUGUI>();

            if (rt == null || popupText == null)
            {
                Debug.LogError("[ExpUIManager] 設定されているPopup Prefabが正しくありません。UIのTextMeshProUGUIを持つプレハブを設定するか、PopUpUITextスクリプトをアタッチしてください。");
                Destroy(popup);
                return;
            }

            // ランダムなオフセットを加えて重なりを防ぐ
            Vector2 randomOffset = Random.insideUnitCircle * 50f;
            rt.anchoredPosition = randomOffset;

            if (amount < 0)
            {
                popupText.text = amount.ToString();
                popupText.color = Color.red;
            }
            else
            {
                popupText.text = $"+{amount}";
            }
            
            // アニメーション (上に移動しながらフェードアウト)
            rt.DOAnchorPosY(rt.anchoredPosition.y + 100f, 1f).SetEase(Ease.OutCubic);
            popupText.DOFade(0f, 1f).SetEase(Ease.InQuart).OnComplete(() => {
                Destroy(popup);
            });
        }
    }
}
