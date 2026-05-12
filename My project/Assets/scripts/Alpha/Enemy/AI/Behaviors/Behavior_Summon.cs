using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Summon Behavior", menuName = "EnemyAI/Behaviors/Summon")]
public class Behavior_Summon : EnemyBehaviorData_Base
{
    public string summonActionName = "Summon Minions";
    
    [Header("Summon Parameters")]
    public GameObject summonPrefab;
    public float summonInterval = 5f;
    public int summonCount = 2;
    public Vector2 spawnOffset = new Vector2(1f, 1f);

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (ai is Alpha_EliteEnemyAI eliteAi)
        {
            eliteAi.TriggerAttackEvent(summonActionName);
        }

        while (true)
        {
            yield return new WaitForSeconds(summonInterval);

            for (int i = 0; i < summonCount; i++)
            {
                // オフセットを少し散らす
                Vector3 offset = new Vector3(
                    Random.Range(-spawnOffset.x, spawnOffset.x),
                    Random.Range(-spawnOffset.y, spawnOffset.y),
                    0
                );

                Vector3 spawnPos = ai.transform.position + offset;

                if (Alpha_ObjectPoolManager.Instance != null && summonPrefab != null)
                {
                    Alpha_ObjectPoolManager.Instance.Rent(summonPrefab, spawnPos, Quaternion.identity);
                }
                else if (summonPrefab != null)
                {
                    Instantiate(summonPrefab, spawnPos, Quaternion.identity);
                }
            }
        }
    }
}
