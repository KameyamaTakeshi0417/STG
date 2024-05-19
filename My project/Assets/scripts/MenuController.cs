using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject menuPanel; // メニューのパネル

    void Update()
    {
        // 'del'キーが押されたとき
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
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void ToggleMenu()
    {
        // パネルの表示/非表示を切り替え
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
}