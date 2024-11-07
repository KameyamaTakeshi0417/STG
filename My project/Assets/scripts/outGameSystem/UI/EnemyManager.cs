using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject healthBarPrefab; // HPバーのPrefab
public    GameObject[] enemyObjects;

    void Awake()
    {
        // 初期配置されているエネミーの処理
       enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObject in enemyObjects)
        {
            Health enemy = enemyObject.GetComponent<Health>();
            if (enemy != null)
            {
                CreateHealthBarForEnemy(enemyObject);
            }
        }
        GameObject.Find("GameManager").GetComponent<GameManager>().setCleared(false);
    }

    public void OnEnemySpawned(GameObject enemyObj)
    {
        
        if (enemyObj != null)
        {
            CreateHealthBarForEnemy(enemyObj);
        }
    }

    void CreateHealthBarForEnemy(GameObject enemy)
    {
        GameObject healthBar = Instantiate(healthBarPrefab, transform);
        
        
    }
}
