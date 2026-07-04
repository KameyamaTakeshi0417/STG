using System.Collections;
using UnityEngine;
using Alpha.Enemy.Wisteria;

[CreateAssetMenu(fileName = "New WisteriaVine B", menuName = "EnemyAI/Behaviors/WisteriaVine B")]
public class Behavior_WisteriaVine_B : EnemyBehaviorData_Base
{
    [Header("Wisteria Vine Settings")]
    [Tooltip("ツタブロックのプレハブ")]
    public GameObject vineBlockPrefab;
    [Tooltip("砲台タイプBのプレハブ")]
    public GameObject turretBPrefab;
    
    [Tooltip("生成開始位置（デフォは右上）")]
    public Vector2 startScreenPos = new Vector2(1.2f, 0.8f);
    
    [Tooltip("生成方向（true=右へ向かって生成、false=左へ向かって生成）")]
    public bool generateToRight = false;
    
    [Tooltip("ツタブロックを生成する間隔（距離）")]
    public float blockSpacing = 1.0f;
    [Tooltip("次のブロックを生成するまでの時間（秒）。0なら一瞬で全て生成。")]
    public float generationDelay = 0.05f;
    
    [Tooltip("砲台を生成する間隔（ツタブロック何個ごとに生成するか）。0以下の場合は生成しない。")]
    public int turretSpawnInterval = 5;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (vineBlockPrefab == null) yield break;

        if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;

        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        // Viewport変換
        Vector2 startViewport = new Vector2((startScreenPos.x + 1f) / 2f, (startScreenPos.y + 1f) / 2f);
        Vector3 startWorldPos = mainCam.ViewportToWorldPoint(new Vector3(startViewport.x, startViewport.y, Mathf.Abs(mainCam.transform.position.z)));
        startWorldPos.z = 0;

        // 生成方向
        Vector3 direction = generateToRight ? Vector3.right : Vector3.left;

        // 動的な最大距離の計算
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
                // 砲台Bは出現直後に自身のStart()から勝手に下に向かって撃ち始める
            }

            currentPos += direction * blockSpacing;
            distanceCovered += blockSpacing;

            if (generationDelay > 0f)
            {
                yield return new WaitForSeconds(generationDelay);
            }
        }
    }
}
