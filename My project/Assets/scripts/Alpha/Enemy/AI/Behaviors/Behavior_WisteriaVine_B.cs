using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Enemy.Wisteria;

[System.Serializable]
public class VineB_SpawnConfig
{
    [Tooltip("trueにすると、画面端ではなくこのエネミー自身の座標を起点としてツタを生成します。")]
    public bool spawnFromEnemy = false;

    [Tooltip("生成開始位置。(-1, 1)が左上、(1, -1)が右下。(spawnFromEnemyがfalseの時のみ有効)")]
    public Vector2 startScreenPos = new Vector2(1.2f, 0.8f);
    
    [Tooltip("生成方向（true=右へ向かって生成、false=左へ向かって生成）")]
    public bool generateToRight = false;
}

[CreateAssetMenu(fileName = "New WisteriaVine B", menuName = "EnemyAI/Behaviors/WisteriaVine B")]
public class Behavior_WisteriaVine_B : EnemyBehaviorData_Base
{
    [Header("Wisteria Vine Settings")]
    [Tooltip("ツタブロックのプレハブ")]
    public GameObject vineBlockPrefab;
    [Tooltip("砲台タイプBのプレハブ")]
    public GameObject turretBPrefab;

    [Header("Spawn Configurations")]
    [Tooltip("同時に生成するツタの設定リスト（要素数を増やすと複数本同時に生成されます）")]
    public List<VineB_SpawnConfig> spawnConfigs = new List<VineB_SpawnConfig> { new VineB_SpawnConfig() };
    
    [Tooltip("ツタブロックを生成する間隔（距離）")]
    public float blockSpacing = 1.0f;
    [Tooltip("次のブロックを生成するまでの時間（秒）。0なら一瞬で全て生成。")]
    public float generationDelay = 0.05f;
    
    [Tooltip("砲台を生成する間隔（ツタブロック何個ごとに生成するか）。0以下の場合は生成しない。")]
    public int turretSpawnInterval = 5;

    [Tooltip("生成したツタがすべて破壊された後、再度ツタを生成するまでの待機時間（秒）。0未満の場合は1度しか生成しません。")]
    public float restartDelay = -1f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (vineBlockPrefab == null || spawnConfigs.Count == 0) yield break;

        if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;

        while (true)
        {
            List<GameObject> allSpawnedBlocks = new List<GameObject>();
            int activeGenerations = spawnConfigs.Count;

            foreach (var config in spawnConfigs)
            {
                ai.StartCoroutine(GenerateVineRoutine(ai, config, allSpawnedBlocks, () => activeGenerations--));
            }

            if (restartDelay < 0f)
            {
                yield break;
            }

            while (activeGenerations > 0)
            {
                yield return null;
            }

            while (allSpawnedBlocks.Count > 0)
            {
                allSpawnedBlocks.RemoveAll(b => b == null);
                if (allSpawnedBlocks.Count == 0) break;
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(restartDelay);
        }
    }

    private IEnumerator GenerateVineRoutine(Alpha_EnemyAI ai, VineB_SpawnConfig config, List<GameObject> allSpawnedBlocks, System.Action onComplete)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        Vector3 startWorldPos = Vector3.zero;

        if (config.spawnFromEnemy)
        {
            startWorldPos = ai.transform.position;
            startWorldPos.z = 0;
        }
        else
        {
            Vector2 startViewport = new Vector2((config.startScreenPos.x + 1f) / 2f, (config.startScreenPos.y + 1f) / 2f);
            startWorldPos = mainCam.ViewportToWorldPoint(new Vector3(startViewport.x, startViewport.y, Mathf.Abs(mainCam.transform.position.z)));
            startWorldPos.z = 0;
        }

        Vector3 direction = config.generateToRight ? Vector3.right : Vector3.left;

        Vector3 screenBottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(mainCam.transform.position.z)));
        Vector3 screenTopRight = mainCam.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCam.transform.position.z)));
        float screenDiagonal = Vector3.Distance(screenBottomLeft, screenTopRight);
        float totalDistance = screenDiagonal * 2f;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Vector3 currentPos = startWorldPos;
        float distanceCovered = 0f;
        int blockCount = 0;

        while (distanceCovered <= totalDistance)
        {
            GameObject block = Instantiate(vineBlockPrefab, currentPos, rotation);
            blockCount++;
            allSpawnedBlocks.Add(block);

            Alpha_WisteriaVineBlock vineScript = block.GetComponent<Alpha_WisteriaVineBlock>();
            if (vineScript != null)
            {
                vineScript.Grow(generationDelay > 0f ? generationDelay : 0.05f);
            }
            
            if (ai.PhaseSpawnedObjects != null)
            {
                ai.PhaseSpawnedObjects.Add(block);
            }

            if (turretBPrefab != null && turretSpawnInterval > 0 && blockCount % turretSpawnInterval == 0)
            {
                GameObject turret = Instantiate(turretBPrefab, currentPos, Quaternion.identity);
                turret.transform.SetParent(block.transform);
            }

            currentPos += direction * blockSpacing;
            distanceCovered += blockSpacing;

            if (generationDelay > 0f)
            {
                yield return new WaitForSeconds(generationDelay);
            }
        }

        onComplete?.Invoke();
    }
}
