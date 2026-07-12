using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Enemy.Wisteria;

[System.Serializable]
public class VineA_SpawnConfig
{
    [Tooltip("trueにすると、画面端ではなくこのエネミー自身の座標を起点としてツタを生成します。")]
    public bool spawnFromEnemy = true;

    [Tooltip("画面端を基準とした生成開始位置。(-1, 1)が左上、(1, -1)が右下。(spawnFromEnemyがfalseの時のみ有効)")]
    public Vector2 startScreenPos = new Vector2(-1f, 1f);

    [Tooltip("trueにすると生成開始時にプレイヤーの方向を狙います。falseにすると下記の fixedAngle の方向に真っ直ぐ伸びます。")]
    public bool targetPlayer = true;

    [Tooltip("targetPlayer が false の場合の進行角度（0=右、90=上、180=左、270=下）")]
    public float fixedAngle = 270f;
}

public enum TurretActivationMode
{
    Synchronized, // ツタが伸びきった後、一斉に発射開始（現在の仕様）
    Sequential    // タレットが生成された順に即座に発射開始
}

[CreateAssetMenu(fileName = "New WisteriaVine A", menuName = "EnemyAI/Behaviors/WisteriaVine A")]
public class Behavior_WisteriaVine_A : EnemyBehaviorData_Base
{
    [Header("Wisteria Vine Settings")]
    [Tooltip("タレットの射撃開始タイミング（Synchronized=一斉射撃, Sequential=生成順に順次射撃）")]
    public TurretActivationMode activationMode = TurretActivationMode.Synchronized;
    [Header("Wisteria Vine Settings")]
    [Tooltip("ツタブロックのプレハブ")]
    public GameObject vineBlockPrefab;
    [Tooltip("砲台タイプAのプレハブ")]
    public GameObject turretAPrefab;

    [Header("Spawn Configurations")]
    [Tooltip("同時に生成するツタの設定リスト（要素数を増やすと複数本同時に生成されます）")]
    public List<VineA_SpawnConfig> spawnConfigs = new List<VineA_SpawnConfig> { new VineA_SpawnConfig() };
    
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

        // ボスは動かない（あるいは現在の速度をゼロにする）
        if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;

        while (true)
        {
            List<GameObject> allSpawnedBlocks = new List<GameObject>();
            int activeGenerations = spawnConfigs.Count;

            // リストに登録された設定数だけ同時に生成コルーチンを走らせる
            foreach (var config in spawnConfigs)
            {
                ai.StartCoroutine(GenerateVineRoutine(ai, config, allSpawnedBlocks, () => activeGenerations--));
            }

            // リピートしない設定なら呼び出し元のコルーチンはここで終了（生成自体は走る）
            if (restartDelay < 0f)
            {
                yield break;
            }

            // すべての生成処理（伸びる演出など）が完了するのを待機
            while (activeGenerations > 0)
            {
                yield return null;
            }

            // ツタがすべて破壊されるのを待機する
            while (allSpawnedBlocks.Count > 0)
            {
                allSpawnedBlocks.RemoveAll(b => b == null);
                if (allSpawnedBlocks.Count == 0) break;
                yield return new WaitForSeconds(0.5f);
            }

            // ツタ全滅後、指定時間待ってから次の攻撃を開始
            yield return new WaitForSeconds(restartDelay);
        }
    }

    private IEnumerator GenerateVineRoutine(Alpha_EnemyAI ai, VineA_SpawnConfig config, List<GameObject> allSpawnedBlocks, System.Action onComplete)
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
            Vector2 viewportPos = new Vector2((config.startScreenPos.x + 1f) / 2f, (config.startScreenPos.y + 1f) / 2f);
            startWorldPos = mainCam.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, Mathf.Abs(mainCam.transform.position.z)));
            startWorldPos.z = 0;
        }

        Vector3 direction = Vector3.zero;

        if (config.targetPlayer)
        {
            Vector3 playerPos = Vector3.zero;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerPos = player.transform.position;
            }

            direction = (playerPos - startWorldPos).normalized;
            if (direction == Vector3.zero) direction = Vector3.down;
        }
        else
        {
            float rad = config.fixedAngle * Mathf.Deg2Rad;
            direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        }

        Vector3 screenBottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(mainCam.transform.position.z)));
        Vector3 screenTopRight = mainCam.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCam.transform.position.z)));
        float screenDiagonal = Vector3.Distance(screenBottomLeft, screenTopRight);
        float maxDistance = screenDiagonal * 2f; 

        List<Alpha_TurretA_Controller> spawnedTurrets = new List<Alpha_TurretA_Controller>();

        Vector3 currentPos = startWorldPos;
        float distanceCovered = 0f;
        int blockCount = 0;

        while (distanceCovered < maxDistance)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

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

            if (turretAPrefab != null && turretSpawnInterval > 0 && blockCount % turretSpawnInterval == 0)
            {
                GameObject turret = Instantiate(turretAPrefab, currentPos, Quaternion.identity);
                turret.transform.SetParent(block.transform);
                
                Alpha_TurretA_Controller turretCtrl = turret.GetComponent<Alpha_TurretA_Controller>();
                if (turretCtrl != null)
                {
                    spawnedTurrets.Add(turretCtrl);

                    // Sequentialモードなら生成直後に起動
                    if (activationMode == TurretActivationMode.Sequential)
                    {
                        turretCtrl.ActivateTurret();
                    }
                }
            }

            currentPos += direction * blockSpacing;
            distanceCovered += blockSpacing;

            if (generationDelay > 0f)
            {
                yield return new WaitForSeconds(generationDelay);
            }
        }

        // Synchronizedモードなら最後に一斉起動
        if (activationMode == TurretActivationMode.Synchronized)
        {
            foreach (var turret in spawnedTurrets)
            {
                if (turret != null)
                {
                    turret.ActivateTurret();
                }
            }
        }

        onComplete?.Invoke();
    }
}
