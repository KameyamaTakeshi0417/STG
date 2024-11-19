using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _PopUpDecideUI_Base : MonoBehaviour
{
    // Start is called before the first frame update
  public GameObject selectionCanvas; // 選択用の Canvas

    void Start()
    {
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false); // 初期状態で選択 UI を非表示にしておく
        }
    }
    // Update is called once per frame

    public virtual void StartSelection()
    {
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(true); // 選択 UI を表示
        }
        freezeGame();

    }
 public void freezeGame()
    {
        Time.timeScale = 0f; // ゲームの時間を停止
    }

    public void continueGame()
    {
        Time.timeScale = 1f; // ゲームの時間を再開
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false); // 選択 UI を非表示にする
        }
    }

}
