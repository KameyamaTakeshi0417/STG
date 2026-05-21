using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Composite Behavior", menuName = "EnemyAI/Behaviors/Composite")]
public class Behavior_Composite : EnemyBehaviorData_Base
{
    public enum ExecutionMode
    {
        Parallel,    // 同時実行
        Sequential   // 順番実行
    }

    [Header("Composite Settings")]
    [Tooltip("実行モード。Parallel(同時実行)かSequential(順番実行)かを選択します。")]
    public ExecutionMode executionMode = ExecutionMode.Parallel;

    [Tooltip("Sequentialモード時、1つのBehaviorが終了してから次のBehaviorを実行するまでの待機時間（秒）")]
    public float delayBetweenBehaviors = 0f;

    [FormerlySerializedAs("behaviorsToRunInParallel")]
    [Tooltip("実行したいBehaviorをここに複数登録します。上から順に実行（または同時実行）されます。")]
    public List<EnemyBehaviorData_Base> behaviors = new List<EnemyBehaviorData_Base>();

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (executionMode == ExecutionMode.Parallel)
        {
            // 並行実行
            foreach (var behavior in behaviors)
            {
                if (behavior != null)
                {
                    ai.StartCoroutine(behavior.RunBehavior(ai));
                }
            }

            // Parallelの場合は（これまでの互換性を維持し）フェーズが終わるまで生存し続ける
            while (true)
            {
                yield return null;
            }
        }
        else if (executionMode == ExecutionMode.Sequential)
        {
            // 順番実行
            for (int i = 0; i < behaviors.Count; i++)
            {
                var behavior = behaviors[i];
                if (behavior != null)
                {
                    // 完了するまで待機
                    yield return ai.StartCoroutine(behavior.RunBehavior(ai));
                }

                // 最後のBehavior以外はインターバルを挟む
                if (i < behaviors.Count - 1 && delayBetweenBehaviors > 0f)
                {
                    yield return new WaitForSeconds(delayBetweenBehaviors);
                }
            }

            // Sequentialの場合はすべて終了したらコルーチンを抜ける（これによりLoopコンポーネント等で再利用しやすくなります）
        }
    }
}
