using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween
using TMPro; // TextMeshPro

namespace Alpha.UI
{
    /// <summary>
    /// カットイン演出コントローラ。
    /// シーン上に配置した 1 枚の Canvas (UltCanvas) の子に
    ///   FadePanel (Image + CanvasGroup)
    ///   PlayerImg, EliteImg (Image)
    ///   PlayerTxt, EliteTxt (TMP_Text) を配置した構造を想定しています。
    ///   呼び出し側は <c>PlayCutIn(isPlayer, sprite, skillName)</c> を呼ぶだけで完結します。
    /// </summary>
    public class UltCutInController : MonoBehaviour
    {
        public static UltCutInController Instance { get; private set; }

        [Header("UI References (assign in Inspector)")]
        [SerializeField] private CanvasGroup fadePanelCG; // 背景パネル（透明度制御）
        [SerializeField] private Image fadePanelImg;      // 背景パネルの Image (optional)

        [SerializeField] private Image playerImg;        // プレイヤーキャラ画像
        [SerializeField] private Image eliteImg;         // エリートキャラ画像
        [SerializeField] private TMP_Text playerTxt;    // プレイヤー技名テキスト
        [SerializeField] private TMP_Text eliteTxt;     // エリート技名テキスト

        [Header("Image Stop Positions (0.0 ~ 1.0)")]
        [SerializeField] private Vector2 playerImgStopPos = new Vector2(0.25f, 0.5f);
        [SerializeField] private Vector2 eliteImgStopPos = new Vector2(0.75f, 0.5f);

        // 画面幅・高さ（Canvasの論理サイズを使用）
        private float ScreenWidth
        {
            get
            {
                if (playerImg != null && playerImg.canvas != null)
                {
                    return playerImg.canvas.GetComponent<RectTransform>().rect.width;
                }
                return Screen.width;
            }
        }
        
        private float ScreenHeight
        {
            get
            {
                if (playerImg != null && playerImg.canvas != null)
                {
                    return playerImg.canvas.GetComponent<RectTransform>().rect.height;
                }
                return Screen.height;
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // 初期は全非表示にしておく
            SetAllActive(false);
        }

        /// <summary>
        /// カットイン演出開始
        /// </summary>
        /// <param name="isPlayer">true = プレイヤー側、false = エリート側</param>
        /// <param name="charSprite">表示したいキャラスプライト</param>
        /// <param name="skillName">表示したい技名文字列</param>
        public void PlayCutIn(bool isPlayer, Sprite charSprite, string skillName)
        {
            // 目的オブジェクトを選択
            Image img = isPlayer ? playerImg : eliteImg;
            TMP_Text txt   = isPlayer ? playerTxt : eliteTxt;
            // プレイヤーは左 → 右、エリートは右 → 左 にスライド
            float direction = isPlayer ? -1f : 1f;

            // プレイヤーは左詰め、ボス（エリート）は右詰めに設定
            txt.alignment = isPlayer ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
            txt.enableWordWrapping = false;
            txt.overflowMode = TextOverflowModes.Overflow;

            // スプライトとテキストをセット
            img.sprite = charSprite;
            txt.text   = skillName;

            // テキストの実際の長さを計算
            txt.ForceMeshUpdate();
            float textWidth = txt.preferredWidth;

            var imgRT = img.rectTransform;
            var txtRT = txt.rectTransform;

            // 確実な位置計算のためにアンカーを画面中央に固定
            imgRT.anchorMin = new Vector2(0.5f, 0.5f);
            imgRT.anchorMax = new Vector2(0.5f, 0.5f);
            txtRT.anchorMin = new Vector2(0.5f, 0.5f);
            txtRT.anchorMax = new Vector2(0.5f, 0.5f);

            // 画像の目標位置（0-1の入力値を画面座標に変換）
            Vector2 normalizedImgPos = isPlayer ? playerImgStopPos : eliteImgStopPos;
            float imgTargetX = (normalizedImgPos.x - 0.5f) * ScreenWidth;
            float imgTargetY = (normalizedImgPos.y - 0.5f) * ScreenHeight;

            // テキストの目標位置を文字の長さから計算
            // ピボットを端に設定し、枠の幅を文字長と一致させる
            txtRT.pivot = new Vector2(isPlayer ? 0f : 1f, 0.5f);
            txtRT.sizeDelta = new Vector2(textWidth, txtRT.sizeDelta.y);
            
            // プレイヤーは左端(-ScreenWidth/2)、エリートは右端(ScreenWidth/2)にピタッと合わせる
            float txtTargetX = isPlayer ? (-ScreenWidth * 0.5f) : (ScreenWidth * 0.5f);
            float txtTargetY = txtRT.anchoredPosition.y; // Y座標は既存のものを維持

            // 初期位置（進行方向の逆の画面外）
            float startX = direction * ScreenWidth * 1.5f; 
            
            imgRT.anchoredPosition = new Vector2(startX, imgTargetY);
            txtRT.anchoredPosition = new Vector2(startX, txtTargetY);

            // アクティブ化
            img.gameObject.SetActive(true);
            txt.gameObject.SetActive(true);
            fadePanelCG.gameObject.SetActive(true);

            // 背景パネルフェードイン
            Image bgImg = fadePanelImg != null ? fadePanelImg : fadePanelCG.GetComponent<Image>();
            if (bgImg != null)
            {
                fadePanelCG.alpha = 1f;
                Color c = bgImg.color; c.a = 0f; bgImg.color = c;
                bgImg.DOFade(0.4f, 0.3f).SetEase(Ease.Linear);
            }
            else
            {
                fadePanelCG.alpha = 0f;
                fadePanelCG.DOFade(0.4f, 0.3f).SetEase(Ease.Linear);
            }

            Sequence seq = DOTween.Sequence();

            // スライドイン（画像）
            seq.Append(imgRT.DOAnchorPos(new Vector2(imgTargetX, imgTargetY), 0.5f).SetEase(Ease.OutBack));

            seq.AppendInterval(0.2f);
            
            // スライドイン（テキスト）
            seq.Append(txtRT.DOAnchorPos(new Vector2(txtTargetX, txtTargetY), 0.5f).SetEase(Ease.OutBack));

            seq.AppendInterval(0.2f);

            // スライドアウト：反対側へ画面幅分移動して完全に見えなくする
            float offX = -direction * ScreenWidth * 1.5f;
            seq.Append(imgRT.DOAnchorPosX(offX, 1f).SetEase(Ease.InSine));
            seq.Join(txtRT.DOAnchorPosX(offX, 1f).SetEase(Ease.InSine));

            // フェードアウト
            Tween fadeOutTween = bgImg != null ? bgImg.DOFade(0f, 0.3f) : fadePanelCG.DOFade(0f, 0.3f);
            seq.Append(fadeOutTween.OnComplete(() =>
            {
                SetAllActive(false);
            }));

            fadePanelCG.transform.SetAsLastSibling();
            img.transform.SetAsLastSibling();
            txt.transform.SetAsLastSibling();
        }

        /// <summary>
        /// 全 UI 要素を非表示にするヘルパー。
        /// </summary>
        private void SetAllActive(bool active)
        {
            fadePanelCG.gameObject.SetActive(active);
            playerImg.gameObject.SetActive(active);
            eliteImg.gameObject.SetActive(active);
            playerTxt.gameObject.SetActive(active);
            eliteTxt.gameObject.SetActive(active);
        }
    }
}
