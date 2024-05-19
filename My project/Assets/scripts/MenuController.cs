using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject menuPanel; // メニューのパネル
public GameObject boardStatus; // Board_Replacementオブジェクト
public GameObject boardReplacement; // Board_Replacementオブジェクト
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
            Time.timeScale = 0; // ゲームの時間を停止
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1; // ゲームの時間を再開
        }
    }

    public void ToggleMenu()
    {
        boardReplacement.SetActive(false);
        boardStatus.SetActive(false);
        // パネルの表示/非表示を切り替え
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
     public void ActivateBoard_Replacement()
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
         public void UnActivateBoard_Replacement()
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
         public void ActivateBoard_Status()
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
             public void UnActivateBoard_Status()
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