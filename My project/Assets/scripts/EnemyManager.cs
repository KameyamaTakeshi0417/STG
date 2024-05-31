using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject healthBarPrefab; // HPバーのPrefab

    void Start()
    {
        // 初期配置されているエネミーの処理
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObject in enemyObjects)
        {
            Health enemy = enemyObject.GetComponent<Health>();
            if (enemy != null)
            {
                CreateHealthBarForEnemy(enemy);
            }
        }
    }

    public void OnEnemySpawned(GameObject enemyObject)
    {
        Health enemy = enemyObject.GetComponent<Health>();
        if (enemy != null)
        {
            CreateHealthBarForEnemy(enemy);
        }
    }

    void CreateHealthBarForEnemy(Health enemy)
    {
        GameObject healthBar = Instantiate(healthBarPrefab, transform);
        EnemyHealthBar healthBarScript = healthBar.GetComponent<EnemyHealthBar>();
        healthBarScript.enemy = enemy;
        healthBarScript.offset = new Vector3(0, 1.5f, 0); // 適切なオフセットを設定
    }
}
