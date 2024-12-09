using System.Linq;
using UnityEngine;

public class Field : MonoBehaviour
{
    public GameObject enemyPrefab;
    private GameObject[] enemies;
    public FieldBoundary fieldBoundary;

    void Start()
    {
        SetEnemy();
        fieldBoundary.EnableBoundary(true); // 壁を有効にする
    }

    void Update()
    {
        CheckEnemies();
    }

    void SetEnemy()
    {
        Vector3 pos = new Vector3(0, 0, 0);
        // float xOffset = 5 * (0.75f);
        // float yOffset = 5 * (0.866f);
        int enemyCount = UnityEngine.Random.Range(2, 5);
        enemies = new GameObject[enemyCount];

        for (int x = 0; x < enemyCount; x++)
        {
            if (x % 2 == 1)
            {
                pos.x *= -1.0f;
            }
            if (x <= 2)
            {
                pos.y *= -1.0f;
            }
            if (x >= 4)
            {
                pos = new Vector3(0, 0, 0);
                break;
            }
            enemies[x] = Instantiate(
                enemyPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            enemies[x].transform.localPosition = pos;
        }
    }

    void CheckEnemies()
    {
        if (enemies.All(e => e == null || !e.activeInHierarchy))
        {
            fieldBoundary.EnableBoundary(false); // 全滅時に壁を無効化
        }
    }
}
