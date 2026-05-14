using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Composite Behavior", menuName = "EnemyAI/Behaviors/Composite")]
public class Behavior_Composite : EnemyBehaviorData_Base
{
    [Tooltip("同時に実行したいBehaviorをここに複数登録します。")]
    public List<EnemyBehaviorData_Base> behaviorsToRunInParallel = new List<EnemyBehaviorData_Base>();

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        // 登録されているすべてのBehaviorに対してコルーチンを走らせる
        foreach (var behavior in behaviorsToRunInParallel)
        {
            if (behavior != null)
            {
                ai.StartCoroutine(behavior.RunBehavior(ai));
            }
        }

        // Composite自身はフェーズが終わる（StopAllBehaviorsが呼ばれる）まで生存し続ける
        while (true)
        {
            yield return null;
        }
    }
}
