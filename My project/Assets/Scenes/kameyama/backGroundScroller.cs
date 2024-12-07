using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 2.0f; // 背景のスクロール速度
    public float resetPositionY = -10.0f; // 背景がリセットされるY座標
    public float startPositionY = 20.0f; // 背景が再配置される開始位置Y座標
    public float appearFinalPositionY = 30f; // 最終的にDがたどり着くY座標
    public int copseLoopCount = 2; // copseをループさせる回数

    public GameObject backgroundA;
    public GameObject backgroundB;
    public GameObject backgroundC;

    public GameObject copse; // 雑木林
    public GameObject field; // 広場
    public GameObject player; // プレイヤーの参照

    private List<GameObject> backgrounds;
    public bool isCleared = false; // ゲームクリア状態
    private bool isTransitioning = false; // 遷移中かどうか
      public Camera mainCamera; // メインカメラ

    void Start()
    {
        // 背景オブジェクトをリストにまとめる
        backgrounds = new List<GameObject> { backgroundA, backgroundB, backgroundC };
        copse.SetActive(false); // 遷移用背景は初期非表示
        field.SetActive(false); // 最終背景も初期非表示
        StartCoroutine(ScrollBackgrounds());
    }

    void Update()
    {

    }

    IEnumerator ScrollBackgrounds()
    {
        foreach (GameObject bg in backgrounds)
        {
             Vector3 worldSize = bg.GetComponent<SpriteRenderer>().bounds.size;

        // ワールド座標をスクリーンサイズに変換
        Vector3 screenSize = mainCamera.WorldToScreenPoint(worldSize);

        // 画面上のサイズ（幅と高さ）を出力
        
            // 背景を下に移動
            bg.transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

            // 背景がリセット位置に達したら再配置
            if (bg.transform.position.y <= resetPositionY+(worldSize.y*0.5f))
            {
                // 背景を他の背景の上に繋げるように配置
                GameObject highestBg = GetHighestBackground();
                bg.transform.position = new Vector3(
                    highestBg.transform.position.x,
                    highestBg.transform.position.y + (startPositionY - resetPositionY),
                    highestBg.transform.position.z
                );
            }
        }
        yield break;
    }

    // 最も上に位置する背景を取得する
    GameObject GetHighestBackground()
    {
        GameObject highestBg = backgrounds[0];
        foreach (GameObject bg in backgrounds)
        {
            if (bg.transform.position.y > highestBg.transform.position.y)
            {
                highestBg = bg;
            }
        }
        return highestBg;
    }

    private IEnumerator TransitionToFinalBackground()
    {
        isTransitioning = true;


        // copse（雑木林）を開始し、A-Cの最後の背景につなげる
        copse.SetActive(true);
        GameObject lastBg = GetHighestBackground();
        copse.transform.position = new Vector3(
            lastBg.transform.position.x,
            lastBg.transform.position.y + (startPositionY - resetPositionY),
            lastBg.transform.position.z
        );

        for (int i = 0; i < copseLoopCount; i++)
        {
            while (copse.transform.position.y > resetPositionY)
            {
                copse.transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
                yield return null;
            }
            // copseを再配置
            copse.transform.position = new Vector3(
                copse.transform.position.x,
                startPositionY,
                copse.transform.position.z
            );
        }
        // 通常背景を停止
        foreach (GameObject bg in backgrounds)
        {
            bg.SetActive(false);
        }

        // copseを停止し、field（広場）を開始
        copse.SetActive(false);
        field.SetActive(true);
        field.transform.position = new Vector3(
            copse.transform.position.x,
            copse.transform.position.y + (startPositionY - resetPositionY),
            copse.transform.position.z
        );

        // 最終背景を目標位置に向かってスクロール
        while (field.transform.position.y > appearFinalPositionY)
        {
            field.transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
            yield return null;
        }

        // 最終背景の位置を固定
        field.transform.position = new Vector3(
            field.transform.position.x,
            appearFinalPositionY,
            field.transform.position.z
        );

        isTransitioning = false;
    }

    public void SetCleared(bool cleared)
    {
        isCleared = cleared;
    }
}
