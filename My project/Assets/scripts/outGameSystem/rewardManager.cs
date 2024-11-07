using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rewardManager : MonoBehaviour
{
    public float checkInterval = 1.0f; // チェック間隔（秒）
    public GameObject treasureBox;
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().setCleared(false);
        // 定期的に敵の数をチェックするコルーチンを開始
        StartCoroutine(clearchecker());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator clearchecker()
    {
        while (GameObject.Find("GameManager").GetComponent<GameManager>().getCleared() == false)
        {
            yield return new WaitForSeconds(0.5f);
        }
        yield return showReward(3);
    }
    IEnumerator showReward(int numToGenerate)
    {
        List<GameObject> generatedObjects = new List<GameObject>();

        for (int i = -1; i < numToGenerate - 1; i++)
        {
            // treasureBoxを生成し、変数に格納
            GameObject newTreasureBox = Instantiate(treasureBox, new Vector3(i * 2, 0, 0), Quaternion.identity);

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
}
