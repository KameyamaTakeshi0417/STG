using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Summon Circulator Behavior", menuName = "EnemyAI/Behaviors/SummonCirculator")]
public class Behavior_SummonCirculator : EnemyBehaviorData_Base
{
    public string summonActionName = "Summon Circulator";

    [Header("Summon Settings")]
    public GameObject circulatorPrefab;
    [Tooltip("召喚の間隔（秒）")]
    public float summonInterval = 5f;
    [Tooltip("同時に存在できる最大数")]
    public int maxConcurrentSummons = 4;

    [Tooltip("召喚する位置のオフセット（エネミー中心からの相対位置）。配列の数だけ同時に召喚を試みます。")]
    public Vector2[] spawnOffsets = new Vector2[]
    {
        new Vector2(2f, 2f),
        new Vector2(-2f, 2f)
    };

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (ai is Alpha_EliteEnemyAI eliteAi)
        {
            eliteAi.TriggerAttackEvent(summonActionName);
        }

        List<GameObject> activeSummons = new List<GameObject>();

        while (true)
        {
            // 無効になった（破壊された）召喚物をリストから除去
            activeSummons.RemoveAll(obj => obj == null || !obj.activeInHierarchy);

            // 上限に達していなければ召喚
            if (activeSummons.Count < maxConcurrentSummons && circulatorPrefab != null)
            {
                foreach (Vector2 offset in spawnOffsets)
                {
                    if (activeSummons.Count >= maxConcurrentSummons)
                        break;

                    Vector3 spawnPos = ai.transform.position + (Vector3)offset;

                    GameObject newCirculator = null;

                    if (Alpha_ObjectPoolManager.Instance != null)
                    {
                        newCirculator = Alpha_ObjectPoolManager.Instance.Rent(circulatorPrefab, spawnPos, Quaternion.identity);
                    }
                    else
                    {
                        newCirculator = Instantiate(circulatorPrefab, spawnPos, Quaternion.identity);
                    }

                    if (newCirculator != null)
                    {
                        activeSummons.Add(newCirculator);
                    }
                }
            }

            yield return new WaitForSeconds(summonInterval);
        }
    }
}
