using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public struct DungeonConstruct
{
    public int value;
}
public enum GameState { Menu, Playing, GameOver }
public class GameManager : MonoBehaviour
{// 3x19の構造体配列を定義
   public int battleCount = 0; // 現在のバトルカウント
    public List<int> initialNumbers = new List<int>(); // 現在の範囲内のインデックスを格納
    private DungeonConstruct[,] DungeonConstructArray = new DungeonConstruct[3, 19];
    public static GameManager Instance { get; private set; }

    public int score { get; private set; }

    public GameState currentState;

    public int NowRow, NowCol;
    public bool isCleared;

    public int getBattleCount() { return battleCount; }
    private void Awake()
    {
        isCleared = false;
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded; // シーンロードイベントを追加
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // イベントの解除
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // すべてのシーンでisClearedをfalseにリセット
        isCleared = false;
        Debug.Log($"Scene {scene.name} loaded. isCleared has been reset to false.");
    }
    // Start is called before the first frame update
    void Start()
    {
        currentState = GameState.Menu;
        score = 0;
        // ゲームの初期化処理
        // 配列にランダムな値を設定
        FillArrayWithRandomValues();
        // 配列の内容をコンソールに出力して確認
        // PrintArray();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ResetGame()
    {
        score = 0;
        // ゲームのリセット処理
    }
    public void EndGame()
    {
        currentState = GameState.GameOver;
        // ゲームオーバーの処理
    }
    void FillArrayWithRandomValues()
    {
        //int sceneType = 4; // 生成する数値の範囲（0から3）

        // 重み付きリストを作成。各数値とその重みを設定
        Dictionary<int, int> weightedNumbers = new Dictionary<int, int>()
    {
        { 0, 55 }, // {n,m}でnが対象の数値、mが重み。合計100にするのが良いか。
        { 1, 24 }, //0が通常戦闘部屋、1がイベント、2がエリートエネミー、3が休憩。4は宝物部屋、5はボス戦の番号となっておる。 
        { 2, 14 },
        { 3, 5 },
        { 4, 2 }
    };

        // 累積重みを計算
        int totalWeight = 0;
        foreach (var weight in weightedNumbers.Values)
        {
            totalWeight += weight;
        }

        // Randomクラスのインスタンスを作成
        System.Random random = new System.Random();

        // 配列をループして重み付けによるランダムな値を設定
        for (int i = 0; i < DungeonConstructArray.GetLength(0); i++)
        {
            for (int j = 0; j < DungeonConstructArray.GetLength(1); j++)
            {

                // 0から累積重みの範囲内で乱数を取得
                int randomValue = random.Next(0, totalWeight);

                // 重みをもとに数値を選択
                foreach (var kvp in weightedNumbers)
                {
                    if (randomValue < kvp.Value)
                    {
                        DungeonConstructArray[i, j].value = kvp.Key;
                        break;
                    }
                    randomValue -= kvp.Value;
                }
                if (j == 10)
                {
                    DungeonConstructArray[i, j].value = 4;
                }
            }
        }

        PrintArray();
    }


    void PrintArray()
    {
        // 配列の内容をコンソールに出力
        for (int i = 0; i < DungeonConstructArray.GetLength(0); i++)
        {
            string row = "Row " + i + ": ";
            for (int j = 0; j < DungeonConstructArray.GetLength(1); j++)
            {
                row += DungeonConstructArray[i, j].value + " ";
            }
            Debug.Log(row);
        }
    }



    public void ChangeScene(int num)
    {
        // シーン遷移直前に必要な処理を追加（例: 現在の状態リセット）
        isCleared = false;

        int nextRow = NowRow;
        int nextCol = num;
        int nextFloor;
        if (nextRow < 5)
        {
            nextFloor = DungeonConstructArray[nextCol, nextRow].value;
            NowRow += 1;
        }
        else
        {
            nextFloor = 5;
            NowRow = 0;
        }

        Debug.Log("loadSceneNumber:" + nextFloor);
        SceneManager.LoadScene("scene" + nextFloor);
    }
    public void setCleared(bool clear)
    {
        isCleared = clear;
    }
    public bool getCleared()
    {
        return isCleared;

    }
     public void UpdateBattleCount()
    {
        battleCount++;
        UpdateInitialNumbers();
    }

    // battleCountの範囲が変更されるたびに初期化するメソッド
    public void UpdateInitialNumbers()
    {
        initialNumbers.Clear();
        int startIndex, endIndex;

        if (battleCount < 2)
        {
            startIndex = 1; // インデックス1
            endIndex = 3;   // インデックス3
        }
        else
        {
            int range = ((battleCount - 2) / 3) * 3 + 2;
            startIndex = range;
            endIndex = range + 2;
        }

        // 範囲内のインデックスを初期化
        for (int i = startIndex; i <= endIndex; i++)
        {
            initialNumbers.Add(i);
        }
    }
}
