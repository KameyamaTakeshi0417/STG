using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct enemyList
{
    public string[] values; // データを格納する配列

    public enemyList(string[] initialValues)
    {
        values = new string[initialValues.Length];
        for (int i = 0; i < initialValues.Length; i++)
        {
            values[i] = initialValues[i];
        }
    }
}

public struct spawnPos
{
    public Vector3[] values; // データを格納する配列

    public spawnPos(Vector3[] initialValues)
    {
        values = new Vector3[initialValues.Length];
        for (int i = 0; i < initialValues.Length; i++)
        {
            values[i] = initialValues[i];
        }
    }
}

public class EnemyList : MonoBehaviour
{
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (SceneManager.GetActiveScene().name == "scene0")
            setEnemy(gameManager.getBattleCount());
    }

    void Awake() { }

    // Update is called once per frame
    void Update() { }

    public void setEnemy(int battleCount)
    {
        // 初期インデックスの更新
        gameManager.UpdateInitialNumbers();

        // initialNumbersからランダムに選択
        List<int> availableIndexes = gameManager.initialNumbers;

        if (availableIndexes.Count > 0)
        {
            int randomIndex = Random.Range(0, availableIndexes.Count);
            int selectedListIndex = availableIndexes[randomIndex];

            // 敵リストとスポーン位置を取得
            string[] enemiesToSpawn = enemyList1.enemyListArray[selectedListIndex].values;
            Vector3[] spawnPositions = enemySpawnPos.vector3Groups[enemiesToSpawn.Length];

            // 敵とスポーン位置の数が異なる場合、必要に応じて調整
            int spawnCount = enemiesToSpawn.Length;

            // 指定された敵を各スポーン位置に生成
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnEnemy(enemiesToSpawn[i], spawnPositions[i]);
            }

            // バトルカウントを更新
            gameManager.UpdateBattleCount();
        }
        else
        {
            Debug.LogWarning("No available enemy lists in the specified range.");
        }
    }

    public void SpawnEnemy(string name, Vector3 spawnPos)
    {
        GameObject enemyPrefab = Resources.Load<GameObject>("Objects/Enemy/" + name);
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Prefab with name " + name + " not found in Resources.");
        }
    }
}

public static class enemyList1
{
    public static enemyList[] enemyListArray = new enemyList[]
    {
        new enemyList(new string[] { "Spawner", "Spawner", "Spawner" }),
        new enemyList(new string[] { "ArmorBeetle", "ArmorBeetle", "ArmorBeetle", "ArmorBeetle" }),
        new enemyList(new string[] { "slime" }),
        new enemyList(new string[] { "slime", "ArmorBeetle", "ArmorBeetle" }),
        new enemyList(new string[] { "slime", "slime" }),
        new enemyList(
            new string[]
            {
                "ArmorBeetle",
                "ArmorBeetle",
                "ArmorBeetle",
                "ArmorBeetle",
                "ArmorBeetle",
                "ArmorBeetle",
                "ArmorBeetle",
                "ArmorBeetle",
            }
        ),
    };
}

public static class enemySpawnPos
{
    // 1〜10個のVector3を格納するリスト
    public static List<Vector3[]> vector3Groups = new List<Vector3[]>
    {
        new Vector3[] { new Vector3(0, 0, 0) }, // 配列1
        new Vector3[] { new Vector3(-3, 0, 0), new Vector3(3, 0, 0) }, // 配列2
        new Vector3[] { new Vector3(-4, 0, 0), new Vector3(0, 0, 0), new Vector3(4, 0, 0) }, // 配列3
        new Vector3[]
        {
            new Vector3(-6, 2, 0),
            new Vector3(6, 2, 0),
            new Vector3(-2, 0, 0),
            new Vector3(2, 0, 0),
        },
        new Vector3[]
        {
            new Vector3(-5, 2, 0),
            new Vector3(0, 2, 0),
            new Vector3(5, 0, 0),
            new Vector3(-2, 0, 0),
            new Vector3(2, 0, 0),
        },
        new Vector3[]
        {
            new Vector3(-6, 2, 0),
            new Vector3(0, 2, 0),
            new Vector3(6, 0, 0),
            new Vector3(-6, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(6, 0, 0),
        },
        new Vector3[]
        {
            new Vector3(-6, 2, 0),
            new Vector3(-3, 2, 0),
            new Vector3(3, 2, 0),
            new Vector3(6, 2, 0),
            new Vector3(-5, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(5, 0, 0),
        },
        new Vector3[]
        {
            new Vector3(-6, 2, 0),
            new Vector3(-3, 2, 0),
            new Vector3(3, 2, 0),
            new Vector3(6, 2, 0),
            new Vector3(-6, 0, 0),
            new Vector3(3, 0, 0),
            new Vector3(3, 0, 0),
            new Vector3(6, 0, 0),
        },
        new Vector3[]
        {
            new Vector3(-8, 2, 0),
            new Vector3(-4, 2, 0),
            new Vector3(0, 2, 0),
            new Vector3(4, 2, 0),
            new Vector3(8, 2, 0),
            new Vector3(-6, 0, 0),
            new Vector3(3, 0, 0),
            new Vector3(3, 0, 0),
            new Vector3(6, 0, 0),
        },
        new Vector3[]
        {
            new Vector3(-8, 2, 0),
            new Vector3(-4, 2, 0),
            new Vector3(0, 2, 0),
            new Vector3(4, 2, 0),
            new Vector3(8, 2, 0),
            new Vector3(-8, 0, 0),
            new Vector3(-4, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(4, 0, 0),
            new Vector3(8, 0, 0),
        },
    };
}
