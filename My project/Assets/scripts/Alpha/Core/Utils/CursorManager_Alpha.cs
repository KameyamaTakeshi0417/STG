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

        private bool isCombatMode = true;

        void Start()
        {
            SetCombatCursor();
        }

        void Update()
        {
            if (StageManager_Alpha.Instance == null) return;

            // 戦闘中かどうかの判定（TimeScaleが0の場合や、特定のStateの場合はUIモードにする）
            bool shouldBeCombat = Time.timeScale > 0f && 
                                  StageManager_Alpha.Instance.currentState != StageState_Alpha.Transition &&
                                  StageManager_Alpha.Instance.currentState != StageState_Alpha.StageClear &&
                                  StageManager_Alpha.Instance.currentState != StageState_Alpha.WaitToStartFirstHalf &&
                                  StageManager_Alpha.Instance.currentState != StageState_Alpha.WaitToStartSecondHalf;

            if (shouldBeCombat && !isCombatMode)
            {
                SetCombatCursor();
            }
            else if (!shouldBeCombat && isCombatMode)
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
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // デフォルトカーソルに戻す
            }
        }
    }
}
