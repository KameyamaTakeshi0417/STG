using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Manager_Base : MonoBehaviour
{
    public GameObject selectionCanvas;
    public GameObject rootBullet;
    public GameObject rootCase;
    public GameObject rootPrimer;

    public GameObject reefBullet;
    public GameObject reefCase;
    public GameObject reefPrimer;

    public GameObject flowerBullet;
    public GameObject flowerCase;
    public GameObject flowerPrimer;
    protected GameObject tmpObj;
    protected bool useMainEquip = true;

    // Start is called before the first frame update

    protected void canvasIsfalse()
    {
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false); // 初期状態で選択 UI を非表示にしておく
        }
    }

    protected void freezeGame()
    {
        Time.timeScale = 0f; // ゲームの時間を停止
    }

    protected void UnfreezeGame()
    {
        Time.timeScale = 1f; // ゲームの時間を停止
    }
    // Update is called once per frame
}
