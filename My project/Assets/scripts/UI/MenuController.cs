using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject menuPanel; // メニューのパネル
    public GameObject boardStatus; // Board_Statusオブジェクト
    public GameObject boardReplacement; // Board_Replacementオブジェクト

    private Texture2D cursorTexture; // カーソルのテクスチャ
    private Vector2 cursorHotspot; // カーソルのホットスポット
    private bool isPaused = false;

    void Start()
    {
       
        // Resourcesフォルダからカーソルのテクスチャをロード
        cursorTexture = Resources.Load<Texture2D>("target");
        cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2); // カーソルの中央をホットスポットに設定
    }

    void Update()
    {
        // 'Escape'キーが押されたとき
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }

        // マウスカーソルを表示
        if (menuPanel.activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto); // カーソルアイコンを設定
        }
    }

    public void ToggleMenu()
    {
        // メニューを開く際にボードオブジェクトを非アクティブにする
        boardReplacement.SetActive(false);
        boardStatus.SetActive(false);
        // パネルの表示/非表示を切り替え
        menuPanel.SetActive(!menuPanel.activeSelf);
        
        // ポーズ状態を切り替える
        isPaused = menuPanel.activeSelf;
        Time.timeScale = isPaused ? 0f : 1f;

        // メニューが非表示になったときにカーソルを変更
        if (!menuPanel.activeSelf)
        {
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto); // カーソルアイコンを設定
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // カーソルアイコンをデフォルトに戻す
        }

        // プレイヤーの動きを制御するためにイベントを発行
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.SetPaused(isPaused);
        }
    }

    public void ActivateBoardReplacement()
    {
        if (boardReplacement != null)
        {
            boardReplacement.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Board_Replacement is not assigned.");
        }
    }

    public void UnActivateBoardReplacement()
    {
        if (boardReplacement != null)
        {
            boardReplacement.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Board_Replacement is not assigned.");
        }
    }

    public void ActivateBoardStatus()
    {
        if (boardStatus != null)
        {
            boardStatus.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Board_Status is not assigned.");
        }
    }

    public void UnActivateBoardStatus()
    {
        if (boardStatus != null)
        {
            boardStatus.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Board_Status is not assigned.");
        }
    }
}
