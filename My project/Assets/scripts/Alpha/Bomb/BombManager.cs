using UnityEngine;


namespace Alpha.Bomb
{
    public class BombManager : MonoBehaviour
    {
        public static BombManager Instance { get; private set; }

        [Header("Cut-in Settings")]
        [Tooltip("ボム発動時のカットインに使う立ち絵スプライト")]
        public Sprite bombCutInSprite;
        
        [Tooltip("ボム発動時のカットインに表示するテキスト")]
        public string bombCutInName = "BOMB!";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            playerStatusManager_Alpha.OnGaugeLost += HandleGaugeLost;
        }

        private void OnDisable()
        {
            playerStatusManager_Alpha.OnGaugeLost -= HandleGaugeLost;
        }

        private void HandleGaugeLost()
        {
            Vector3 spawnPos = Vector3.zero;
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null)
            {
                spawnPos = player.transform.position;
            }

            GameObject prefab = Resources.Load<GameObject>("Objects/Bomb/Bomb_Basic");
            Bomb_Basic bombScript = null;

            if (prefab != null)
            {
                GameObject spawnedBomb = Instantiate(prefab, spawnPos, Quaternion.identity);
                bombScript = spawnedBomb.GetComponent<Bomb_Basic>();
            }
            else
            {
                // Resourcesから見つからない場合は動的に生成（フォールバック）
                Debug.LogWarning("Bomb prefab not found! Creating a primitive Bomb_Basic GameObject with visual.");
                GameObject bombObj = new GameObject("Bomb_Basic");
                bombObj.transform.position = spawnPos;
                bombScript = bombObj.AddComponent<Bomb_Basic>();

                // 見た目がないと起動したかわからないため、一時的な円グラフィックを追加
                SpriteRenderer sr = bombObj.AddComponent<SpriteRenderer>();
                // シンプルなスプライトをロードするか、白塗りにする
                // （簡易的にテクスチャを生成）
                Texture2D tex = new Texture2D(2, 2);
                tex.SetPixels(new Color[] { Color.cyan, Color.cyan, Color.cyan, Color.cyan });
                tex.Apply();
                // 1ユニット = 1ピクセル に設定して、scale = 1 が 1メートル四方になるようにする
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
                // 半透明のシアン色
                sr.color = new Color(0, 1, 1, 0.5f);
                // 背景に隠れないようにソートオーダーを最前面に設定
                sr.sortingOrder = 30000;
            }

            // ボム発動のカットインを再生
            if (Alpha.UI.UltCutInController.Instance != null && bombCutInSprite != null)
            {
                Alpha.UI.UltCutInController.Instance.PlayCutIn(true, bombCutInSprite, bombCutInName);
            }
            else if (Alpha.UI.UltCutInController.Instance != null)
            {
                // スプライトが未設定でもカットインは呼ぶ（名前だけでも表示するため）
                Alpha.UI.UltCutInController.Instance.PlayCutIn(true, null, bombCutInName);
            }

            // プレイヤーをボムの持続時間中無敵にする
            if (bombScript != null && player != null)
            {
                float totalDuration = bombScript.expandDuration + bombScript.persistDuration;
                PlayerHealth pHealth = player.GetComponent<PlayerHealth>();
                if (pHealth != null)
                {
                    pHealth.MakeInvincibleWithColliders(totalDuration);
                }
            }
        }
    }
}
