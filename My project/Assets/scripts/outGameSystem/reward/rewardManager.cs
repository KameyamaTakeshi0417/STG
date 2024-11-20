using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rewardManager : MonoBehaviour
{
    public float checkInterval = 1.0f; // チェック間隔（秒）
    public GameObject treasureBox;

    // Start is called before the first frame update
    private int[] normalRewardArray = new int[300];
    private int[] specialRewardArray = new int[12];
    public delegate void RewardTakeHandler();
    public static event RewardTakeHandler BoxisOpened;
    public int currentIndex;

    void Start()
    {
        FillNormalRewarArray();
    }

    // Update is called once per frame
    void Update() { }

    IEnumerator showReward(int numToGenerate)
    {
        List<GameObject> generatedObjects = new List<GameObject>();

        for (int i = -1; i < numToGenerate - 1; i++)
        {
            // treasureBoxを生成し、変数に格納
            GameObject newTreasureBox = Instantiate(
                treasureBox,
                new Vector3((i * 7), 0, 0),
                Quaternion.identity
            );

            // リストに格納して後で参照可能にする
            generatedObjects.Add(newTreasureBox);

            // 必要に応じて短い待機を入れる
            yield return new WaitForSeconds(0.1f);
        }

        // 生成したオブジェクトに対してGetComponentを実行
        foreach (GameObject obj in generatedObjects)
        {
            /*
            // 例えば、CustomScriptを取得して何らかの処理を行う
            CustomScript script = obj.GetComponent<CustomScript>();
            if (script != null)
            {
                script.DoSomething(); // スクリプト内の関数を呼び出し
            }
            */
        }
        generatedObjects.Clear();

        yield break;
    }

    void FillNormalRewarArray()
    {
        //int sceneType = 4; // 生成する数値の範囲（0から3）

        // 重み付きリストを作成。最上位のシークレットレアは特殊な方法でないと排出しないことにした。ギャラクシーなんちゃらの影響。
        Dictionary<int, int> weightedNumbers = new Dictionary<int, int>()
        { //弾丸70%、レリック39%
            { 0, 38 }, // コモンAmmo
            { 1, 25 }, //アンコモンAmmo
            { 2, 7 }, //レアAmmo
            { 3, 18 }, //ノーマルレリック
            { 4, 9 }, //アンコモンレリック
            { 5, 3 }, //レアレリック
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

        for (int j = 0; j < normalRewardArray.Length; j++)
        {
            // 0から累積重みの範囲内で乱数を取得
            int randomValue = random.Next(0, totalWeight);

            // 重みをもとに数値を選択
            foreach (var kvp in weightedNumbers)
            {
                if (randomValue < kvp.Value)
                {
                    normalRewardArray[j] = kvp.Key;
                    break;
                }
                randomValue -= kvp.Value;
            }
        }
        PrintArray();
    }

    void PrintArray()
    {
        // 配列の内容をコンソールに出力

        string row = "rewardIndex: ";
        for (int j = 0; j < normalRewardArray.GetLength(1); j++)
        {
            row += normalRewardArray[j] + " ";
        }
        Debug.Log(row);
    }

    public int[] getRewardValues(int callValue)
    {
        // 配列から値を取得してログに出力する
        int length = normalRewardArray.Length;

        // インデックスが配列の範囲を超えないように制限する
        if (currentIndex + (callValue - 1) < length)
        {
            int[] result = new int[callValue];

            for (int i = 0; i < callValue; i++)
            {
                result[i] = normalRewardArray[currentIndex + i];
                Debug.Log("Reward Value: " + result[i]);
            }

            // 次のインデックスの更新（callValueずつ進む）
            currentIndex += callValue;

            return result;
        }
        else
        {
            Debug.Log("End of rewards array.");

            // 配列の終わりに達した場合は空の配列を返す
            return new int[0];
        }
    }
}
