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

        // 画面幅（Screen Space - Camera 用）
        private float ScreenWidth => Screen.width;

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

            // スプライトとテキストをセット
            img.sprite = charSprite;
            txt.text   = skillName;

            // 初期位置は画面外（左 or 右）
            var imgRT = img.rectTransform;
            var txtRT = txt.rectTransform;
            float startX = direction * ScreenWidth * 0.5f; // 画面半分外側
            imgRT.anchoredPosition = new Vector2(startX, imgRT.anchoredPosition.y);
            txtRT.anchoredPosition = new Vector2(startX, txtRT.anchoredPosition.y);

            // アクティブ化
            img.gameObject.SetActive(true);
            txt.gameObject.SetActive(true);
            fadePanelCG.gameObject.SetActive(true);

            // 背景画像のアルファのみを操作して、子供のテキストに影響が出ないようにする
            Image bgImg = fadePanelImg != null ? fadePanelImg : fadePanelCG.GetComponent<Image>();
            if (bgImg != null)
            {
                fadePanelCG.alpha = 1f; // CanvasGroupは常に1にして子要素（文字など）を透けさせない
                Color c = bgImg.color;
                c.a = 0f;
                bgImg.color = c;
                bgImg.DOFade(0.4f, 0.3f).SetEase(Ease.Linear);
            }
            else
            {
                // Imageが無い場合のフォールバック
                fadePanelCG.alpha = 0f;
                fadePanelCG.DOFade(0.4f, 0.3f).SetEase(Ease.Linear);
            }

            // シーケンス作成
            Sequence seq = DOTween.Sequence();

            // 画像スライドイン（0.5秒, OutBack）
            seq.Append(imgRT.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutBack));

            // 文字は画像開始から 0.2 秒遅れ
            seq.AppendInterval(0.2f);
            seq.Append(txtRT.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutBack));

            // 必要なら短い待機（演出調整用）
            seq.AppendInterval(0.2f);

            // スライドアウト（1秒, InSine）
            float offX = -direction * ScreenWidth * 0.5f; // 逆方向に外へ
            seq.Append(imgRT.DOAnchorPosX(offX, 1f).SetEase(Ease.InSine));
            seq.Join(txtRT.DOAnchorPosX(offX, 1f).SetEase(Ease.InSine));

            // パネルフェードアウト → 非表示
            Tween fadeOutTween = bgImg != null ? bgImg.DOFade(0f, 0.3f) : fadePanelCG.DOFade(0f, 0.3f);
            seq.Append(fadeOutTween.OnComplete(() =>
            {
                SetAllActive(false);
            }));

            // 描画順（Sibling Index）の調整
            // 念のため背景パネルを手前に持ってきた後、画像とテキストをさらに手前に持ってくる
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
