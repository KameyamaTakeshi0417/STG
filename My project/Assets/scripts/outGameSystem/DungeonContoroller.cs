using UnityEngine;

public class DungeonController : MonoBehaviour
{
    public int[,] dungeonGrid = new int[20, 3];  // 部屋情報を格納する20x3の二次元配列
    private int currentRow = 0;
    private int currentCol;

GameObject fieldObject;
GameObject fieldPrefab;
GameObject BOSSfield;
    void Start()
    {
        GenerateDungeon();  // ダンジョンを生成する
        currentCol = Random.Range(0, 3);  // スタート地点を1行目のランダムな列に設定
        Debug.Log("Start in Room: Row 1, Col " + (currentCol + 1));
        Debug.Log("Room value: " + dungeonGrid[currentRow, currentCol]);

        fieldObject=Resources.Load<GameObject>("Floor");
         fieldPrefab = Instantiate(fieldObject,new Vector3(0,0,0), Quaternion.identity); //フィールド生成
    }

    // ダンジョン生成
    void GenerateDungeon()
    {
        for (int row = 0; row < 20; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                dungeonGrid[row, col] = Random.Range(1, 4);  // 各部屋に1〜3のランダムな値を設定
            }
        }
    }

    // 移動先の部屋を決定する関数
    public void MoveToNextRoom(int nextRow, int nextCol)
    {
        if (IsMoveValid(nextRow, nextCol))
        {
            currentRow = nextRow;
            currentCol = nextCol;
            Debug.Log("Moved to Room: Row " + (currentRow + 1) + ", Col " + (currentCol + 1));
            Debug.Log("Room value: " + dungeonGrid[currentRow, currentCol]);

            if (currentRow == 19)
            {
                Debug.Log("Reached the final row!");
            }
        }
        else
        {
            Debug.Log("Invalid move!");
        }
    }

    // 移動が有効かどうかをチェックする
    private bool IsMoveValid(int nextRow, int nextCol)
    {

        // 隣の列または同じ列しか移動できない
        if (nextCol < 0 || nextCol > 2 || Mathf.Abs(nextCol - currentCol) > 1)
        {
            return false;
        }

        return true;
    }

    // 入力に基づいて移動を処理する（例: キーボード入力）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentCol < 2)
        {
            MoveToNextRoom(currentRow, currentCol + 1);  // 同じ行の右隣の部屋に移動
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && currentCol > 0)
        {
            MoveToNextRoom(currentRow, currentCol - 1);  // 同じ行の左隣の部屋に移動
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveToNextRoom(currentRow + 1, currentCol);  // 次の行の同じ列の部屋に移動
        }
    }
}
