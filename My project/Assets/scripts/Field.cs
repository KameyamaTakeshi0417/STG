using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    public GameObject enemyObject;//SetEnemyで生成してるけど今後どうなるかはわからん。
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetEnemy(string enemyName)
    {
        Vector3 pos = new Vector3(0, 0, 0);
        float xOffset = 5 * (0.75f); // 幅の3/4
        float yOffset = 5 * (0.866f); // 高さの√3/2 ≈ 0.866
        int enemyCount = UnityEngine.Random.Range(2, 5);

enemyObject= Resources.Load<GameObject>(enemyName);
        for (int x = 0; x < enemyCount; x++)
        {
            // 基本の位置を計算
            pos.x = xOffset;
            pos.y = yOffset;

            // 奇数行の場合、yPosをオフセット
            if (x % 2 == 1)//一個ずつ位置ずらす
            {
                pos.x *= -1.0f;
            }
            if (x <= 2)
            {//3つめ以降は位置を変える
                pos.y *= -1.0f;
            }
            if (x <= 4)
            {//5個目真ん中にしてbreak。5以上出さないようにする...マズいか？拡張性が死ぬ可能性。
                pos = new Vector3(0, 0, 0);
                break;
            }
            Instantiate(enemyObject, pos, Quaternion.identity);
        }
    }
 
}
