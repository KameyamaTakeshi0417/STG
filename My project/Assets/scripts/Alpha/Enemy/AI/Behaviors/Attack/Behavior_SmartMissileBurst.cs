using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Enemy.Weapons;

[CreateAssetMenu(fileName = "Behavior_SmartMissileBurst", menuName = "Alpha/Enemy AI/Behaviors/Attack/Smart Missile Burst")]
public class Behavior_SmartMissileBurst : EnemyBehaviorData_Base
{
    public enum FirePattern
    {
        Random,
        RadialCW,
        RadialCCW
    }

    [Header("Smart Missile Burst")]
    public GameObject smartMissilePrefab;
    public int missileCount = 12;
    public float missileBurstInterval = 0.1f; // 発射間隔
    public float missileCooldown = 10f;       // 発射後のクールタイム
    public Vector2 spawnOffset = Vector2.zero;

    [Header("Pattern Settings")]
    public FirePattern firePattern = FirePattern.RadialCW;
    public float spreadAngle = 360f;
    [Tooltip("放射状発射の開始角度オフセット")]
    public float startAngleOffset = 0f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        while (true)
        {
            if (smartMissilePrefab != null)
            {
                // ミサイルタイプのリストを作成（最低1つずつ含む）
                List<Alpha_SmartMissile.MissileType> missileTypes = new List<Alpha_SmartMissile.MissileType>();
                missileTypes.Add(Alpha_SmartMissile.MissileType.SmallHoming);
                missileTypes.Add(Alpha_SmartMissile.MissileType.EliteHoming);
                missileTypes.Add(Alpha_SmartMissile.MissileType.Baka);
                
                // 残りをランダムに追加
                for (int i = 3; i < missileCount; i++)
                {
                    missileTypes.Add((Alpha_SmartMissile.MissileType)Random.Range(1, 4)); // 1:SmallHoming, 2:EliteHoming, 3:Baka
                }

                // シャッフル
                for (int i = 0; i < missileTypes.Count; i++)
                {
                    Alpha_SmartMissile.MissileType temp = missileTypes[i];
                    int randomIndex = Random.Range(i, missileTypes.Count);
                    missileTypes[i] = missileTypes[randomIndex];
                    missileTypes[randomIndex] = temp;
                }

                // バースト発射
                Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

                float angleStep = spreadAngle / Mathf.Max(1, missileCount);

                for (int i = 0; i < missileCount; i++)
                {
                    float finalAngle = 0f;

                    if (firePattern == FirePattern.Random)
                    {
                        Vector2 fireDir = Vector2.down;
                        if (player != null)
                        {
                            fireDir = (player.position - ai.transform.position).normalized;
                            float randomOffset = Random.Range(-spreadAngle/2f, spreadAngle/2f);
                            fireDir = Quaternion.Euler(0, 0, randomOffset) * fireDir;
                        }
                        finalAngle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg - 90f;
                    }
                    else
                    {
                        // 放射状発射（360度を等分）
                        float directionMultiplier = (firePattern == FirePattern.RadialCW) ? -1f : 1f;
                        finalAngle = startAngleOffset + (angleStep * i * directionMultiplier);
                    }
                    
                    Vector3 spawnPos = ai.transform.position + (Vector3)spawnOffset;
                    
                    GameObject missileObj = Instantiate(smartMissilePrefab, spawnPos, Quaternion.Euler(0, 0, finalAngle));
                    Alpha_SmartMissile smartMissile = missileObj.GetComponent<Alpha_SmartMissile>();
                    
                    if (smartMissile != null)
                    {
                        smartMissile.type = missileTypes[i];
                    }

                    yield return new WaitForSeconds(missileBurstInterval);
                }
            }

            // 指定秒数のクールタイム
            yield return new WaitForSeconds(missileCooldown);
        }
    }
}
