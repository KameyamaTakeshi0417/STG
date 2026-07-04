using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Enemy.Wisteria;

[CreateAssetMenu(fileName = "New WisteriaVine A", menuName = "EnemyAI/Behaviors/WisteriaVine A")]
public class Behavior_WisteriaVine_A : EnemyBehaviorData_Base
{
    [Header("Wisteria Vine Settings")]
    [Tooltip("ツタブロックのプレハブ")]
    public GameObject vineBlockPrefab;
    [Tooltip("砲台タイプAのプレハブ")]
    public GameObject turretAPrefab;
    
    [Tooltip("画面端を基準とした生成開始位置。(-1, 1)が左上、(1, -1)が右下。")]
    public Vector2 startScreenPos = new Vector2(-1f, 1f);
    
    [Tooltip("ツタブロックを生成する間隔（距離）")]
    public float blockSpacing = 1.0f;
    [Tooltip("次のブロックを生成するまでの時間（秒）。0なら一瞬で全て生成。")]
    public float generationDelay = 0.05f;
    
    [Tooltip("砲台を生成する間隔（ツタブロック何個ごとに生成するか）。0以下の場合は生成しない。")]
    public int turretSpawnInterval = 5;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (vineBlockPrefab == null) yield break;

        // ボスは動かない（あるいは現在の速度をゼロにする）
        if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;

        // カメラからワールド座標を計算
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        // startScreenPos (-1 to 1) を Viewport (0 to 1) に変換
        Vector2 viewportPos = new Vector2((startScreenPos.x + 1f) / 2f, (startScreenPos.y + 1f) / 2f);
        Vector3 startWorldPos = mainCam.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, Mathf.Abs(mainCam.transform.position.z)));
        startWorldPos.z = 0;

        // プレイヤーの位置を取得
        Vector3 playerPos = Vector3.zero;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerPos = player.transform.position;
        }

        // 進行方向
        Vector3 direction = (playerPos - startWorldPos).normalized;
        if (direction == Vector3.zero) direction = Vector3.down;

        // 動的な最大距離の計算（カメラの対角線の長さをベースに十分な長さを確保）
        Vector3 screenBottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(mainCam.transform.position.z)));
        Vector3 screenTopRight = mainCam.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCam.transform.position.z)));
        float screenDiagonal = Vector3.Distance(screenBottomLeft, screenTopRight);
        
        // 画面対角線の2倍あれば、どこからスタートしても必ず画面を突き抜ける
        float maxDistance = screenDiagonal * 2f; 

        List<Alpha_TurretA_Controller> spawnedTurrets = new List<Alpha_TurretA_Controller>();

        Vector3 currentPos = startWorldPos;
        float distanceCovered = 0f;
        int blockCount = 0;

        while (distanceCovered < maxDistance)
        {
            // 進行方向への回転を計算
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // ブロック生成
            GameObject block = Instantiate(vineBlockPrefab, currentPos, rotation);
            blockCount++;
            
            // ツタの伸びるアニメーションを開始
            Alpha_WisteriaVineBlock vineScript = block.GetComponent<Alpha_WisteriaVineBlock>();
            if (vineScript != null)
            {
                vineScript.Grow(generationDelay > 0f ? generationDelay : 0.05f);
            }
            
            // ブロックをフェーズ終了時に消すリストに追加
            if (ai.PhaseSpawnedObjects != null)
            {
                ai.PhaseSpawnedObjects.Add(block);
            }

            // 指定間隔ごとに砲台を生成
            if (turretAPrefab != null && turretSpawnInterval > 0 && blockCount % turretSpawnInterval == 0)
            {
                GameObject turret = Instantiate(turretAPrefab, currentPos, Quaternion.identity);
                // 砲台をブロックの子オブジェクトにする（ブロック破壊時に道連れ）
                turret.transform.SetParent(block.transform);
                
                Alpha_TurretA_Controller turretCtrl = turret.GetComponent<Alpha_TurretA_Controller>();
                if (turretCtrl != null)
                {
                    spawnedTurrets.Add(turretCtrl);
                }
            }

            // 次の位置へ
            currentPos += direction * blockSpacing;
            distanceCovered += blockSpacing;

            if (generationDelay > 0f)
            {
                yield return new WaitForSeconds(generationDelay);
            }
        }

        // 全ツタの生成完了後、砲台を一斉起動
        foreach (var turret in spawnedTurrets)
        {
            if (turret != null)
            {
                turret.ActivateTurret();
            }
        }
    }
}
