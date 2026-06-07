using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Alpha.UI
{
    public class PetalUIManager_Alpha : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("合計花弁数を表示するテキスト")]
        public TextMeshProUGUI totalPetalText;
        
        [Tooltip("ポップアップテキストのプレハブ（DamageTextなどと同じ構造のもの）")]
        public GameObject petalPopupPrefab;
        
        [Tooltip("ポップアップを生成する親オブジェクト（指定がなければこのオブジェクト）")]
        public RectTransform popupContainer;

        private void OnEnable()
        {
            playerStatusManager_Alpha.OnPetalAdded += HandlePetalAdded;
            UpdateTotalPetalText();
        }

        private void OnDisable()
        {
            playerStatusManager_Alpha.OnPetalAdded -= HandlePetalAdded;
        }

        private void Start()
        {
            if (popupContainer == null)
            {
                popupContainer = GetComponent<RectTransform>();
            }
            UpdateTotalPetalText();
        }

        private void HandlePetalAdded(int amount)
        {
            UpdateTotalPetalText();
            ShowPopup(amount);
        }

        private void UpdateTotalPetalText()
        {
            if (totalPetalText != null && playerStatusManager_Alpha.Instance != null)
            {
                totalPetalText.text = playerStatusManager_Alpha.Instance.currentPetals.ToString();
            }
        }

        private void ShowPopup(int amount)
        {
            if (petalPopupPrefab == null || popupContainer == null) return;

            // ポップアップを生成
            GameObject popup = Instantiate(petalPopupPrefab, popupContainer);
            
            // 新規作成したPopUpUITextスクリプトがアタッチされているか確認
            PopUpUIText popupScript = popup.GetComponent<PopUpUIText>();
            if (popupScript != null)
            {
                // PopUpUITextがアタッチされていれば、値を渡してアニメーションはスクリプトに任せる
                popupScript.value = amount;
                return;
            }

            // --- 互換性のため、PopUpUITextがない場合の処理を残す ---
            RectTransform rt = popup.GetComponent<RectTransform>();
            TextMeshProUGUI popupText = popup.GetComponent<TextMeshProUGUI>();

            if (rt == null || popupText == null)
            {
                Debug.LogError("[PetalUIManager] 設定されているPopup Prefabが正しくありません。UIのTextMeshProUGUIを持つプレハブを設定するか、PopUpUITextスクリプトをアタッチしてください。");
                Destroy(popup);
                return;
            }

            // ランダムなオフセットを加えて重なりを防ぐ
            Vector2 randomOffset = Random.insideUnitCircle * 50f;
            rt.anchoredPosition = randomOffset;

            popupText.text = $"+{amount}";
            
            // アニメーション (上に移動しながらフェードアウト)
            rt.DOAnchorPosY(rt.anchoredPosition.y + 100f, 1f).SetEase(Ease.OutCubic);
            popupText.DOFade(0f, 1f).SetEase(Ease.InQuart).OnComplete(() => {
                Destroy(popup);
            });
        }
    }
}
