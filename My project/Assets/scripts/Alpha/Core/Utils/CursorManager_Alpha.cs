using UnityEngine;
using Alpha.Flow;

namespace Alpha.Core.Utils
{
    public class CursorManager_Alpha : MonoBehaviour
    {
        [Header("Cursor Textures")]
        [Tooltip("戦闘中の標準カーソル（照準など）")]
        public Texture2D combatCursor;
        
        [Tooltip("戦闘以外のカーソル（UI選択用の矢印など）")]
        public Texture2D uiCursor;

        [Header("Settings")]
        public Vector2 combatHotspot = new Vector2(16, 16); // 画像の中心などを指定
        public Vector2 uiHotspot = Vector2.zero; // 左上なら(0,0)

        public static CursorManager_Alpha Instance { get; private set; }
        private bool isCombatMode = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            SetUICursor();
        }

        public void SetCombatMode(bool isCombat)
        {
            if (isCombat)
            {
                SetCombatCursor();
            }
            else
            {
                SetUICursor();
            }
        }

        public void SetCombatCursor()
        {
            isCombatMode = true;
            if (combatCursor != null)
            {
                Cursor.SetCursor(combatCursor, combatHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        public void SetUICursor()
        {
            isCombatMode = false;
            if (uiCursor != null)
            {
                Cursor.SetCursor(uiCursor, uiHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }
}
