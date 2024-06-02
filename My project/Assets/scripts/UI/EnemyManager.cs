using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject healthBarPrefab; // HPバーのPrefab
public    GameObject[] enemyObjects;

    void Start()
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
        EnemyHealthBar healthBarScript = healthBar.GetComponent<EnemyHealthBar>();
        healthBarScript.setEnemy(enemy);
        healthBarScript.offset = new Vector3(0, 1.5f, 0); // 適切なオフセットを設定
    }
}
