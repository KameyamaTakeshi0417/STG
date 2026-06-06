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
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                spawnPos = player.transform.position;
            }

            GameObject prefab = Resources.Load<GameObject>("Objects/Bomb/Bomb_Basic");
            if (prefab != null)
            {
                Instantiate(prefab, spawnPos, Quaternion.identity);
                
                // ボム発動のカットインを再生
                if (Alpha.UI.UltCutInController.Instance != null && bombCutInSprite != null)
                {
                    Alpha.UI.UltCutInController.Instance.PlayCutIn(true, bombCutInSprite, bombCutInName);
                }
            }
            else
            {
                // Resourcesから見つからない場合は動的に生成（フォールバック）
                Debug.LogWarning("Bomb prefab not found! Creating a primitive Bomb_Basic GameObject.");
                GameObject bombObj = new GameObject("Bomb_Basic");
                bombObj.transform.position = spawnPos;
                bombObj.AddComponent<Bomb_Basic>();
            }
        }
    }
}
