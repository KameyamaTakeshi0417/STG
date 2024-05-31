using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    public GameObject enemyPrefab; // 敵のPrefabを設定するためのパブリック変数

    void Start()
    {
        SetEnemy();
    }

    void SetEnemy()
    {
        Vector3 pos = new Vector3(0, 0, 0);
        float xOffset = 5 * (0.75f); // 幅の3/4
        float yOffset = 5 * (0.866f); // 高さの√3/2 ≈ 0.866
        int enemyCount = UnityEngine.Random.Range(2, 5);

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
            if (x >= 4)
            {//5個目真ん中にしてbreak。5以上出さないようにする...マズいか？拡張性が死ぬ可能性。
                pos = new Vector3(0, 0, 0);
                break;
            }
            GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);
            // オフセットを少し調整して、敵を適切に配置
            
            enemy.transform.localPosition = pos;
        }
    }
}
