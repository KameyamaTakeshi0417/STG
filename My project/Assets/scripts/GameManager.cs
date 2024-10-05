using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct MyStruct
{
    public int value;
}
public enum GameState { Menu, Playing, GameOver }
public class GameManager : MonoBehaviour
{// 3x19の構造体配列を定義
    private MyStruct[,] myStructArray = new MyStruct[3, 19];
        public static GameManager Instance { get; private set; }

    public int score { get; private set; }

     public GameState currentState;
     private void Awake()
    {
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
        // Randomクラスのインスタンスを作成
        System.Random random = new System.Random();
        
        // 配列をループして1-5のランダムな値を設定
        for (int i = 0; i < myStructArray.GetLength(0); i++)
        {
            for (int j = 0; j < myStructArray.GetLength(1); j++)
            {
                myStructArray[i, j].value = random.Next(1, 6); // Nextの第二引数は上限+1を指定する
            }
        }
    }

    void PrintArray()
    {
        // 配列の内容をコンソールに出力
        for (int i = 0; i < myStructArray.GetLength(0); i++)
        {
            string row = "Row " + i + ": ";
            for (int j = 0; j < myStructArray.GetLength(1); j++)
            {
                row += myStructArray[i, j].value + " ";
            }
            Debug.Log(row);
        }
    }
}
