using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int HP;
    public UnityEngine.Vector3 spawnPos;
    public GameObject enemy;
    private Health hitPoint;
    public float coolTime = 3.5f;
    public EnemyManager enemyManager;
    // Start is called before the first frame update
    void Start()
    {
        hitPoint = GetComponent<Health>();
        hitPoint.setHP(HP);
        enemyManager = GameObject.Find("HPCanvas").GetComponent<EnemyManager>();
        StartCoroutine("createEnemy");
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hitPoint.getHP() <= 0) Destroy(this.gameObject);

    }
    private void CreateSpawnPos()
    {
        float randomPos;//乱数ベクトル作るための一時的なもの
        Vector3 scale = transform.lossyScale;
        randomPos = Random.Range(-1f, 1f);
        if (randomPos <= 0f)
        {
            randomPos = Random.Range(scale.x * (-1.0f), scale.x * (-0.5f));
        }
        else
        {
            randomPos = Random.Range(scale.x * 0.5f, scale.x * 1.0f);
        }
        spawnPos.x = randomPos;
        randomPos = Random.Range(-1f, 1f);
        if (randomPos <= 0f)
        {
            randomPos = Random.Range(scale.y * (-1.0f), scale.y * (-0.5f));
        }
        else
        {
            randomPos = Random.Range(scale.y * 0.5f, scale.y * 1.0f);
        }
        spawnPos.y = randomPos;
        spawnPos += transform.localPosition;
        return;
    }
    private IEnumerator createEnemy()
    {
        Vector3 createPos = transform.position + spawnPos;

        while (hitPoint.getHP() >= 0)
        {

            GameObject enemyPrefab = Instantiate(enemy, createPos, Quaternion.identity);//エネミー生成
            hitPoint.addHP(-4000);

            CreateSpawnPos();
            createPos = transform.position + spawnPos;//次のエネミー生成位置更新
            enemyManager.OnEnemySpawned(enemy);
            yield return new WaitForSeconds(coolTime);
        }
        yield return null;
    }
}
