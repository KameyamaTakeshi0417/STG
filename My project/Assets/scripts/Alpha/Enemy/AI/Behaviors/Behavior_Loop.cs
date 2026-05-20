using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Loop Behavior", menuName = "EnemyAI/Behaviors/Loop")]
public class Behavior_Loop : EnemyBehaviorData_Base
{
    [Header("Target Behavior")]
    [Tooltip("繰り返し実行したいBehaviorをアサインします。")]
    public EnemyBehaviorData_Base targetBehavior;

    [Header("Loop Settings")]
    [Tooltip("繰り返し回数。0以下の場合は無限にループします。")]
    public int repeatCount = 0;
    
    [Tooltip("ターゲットのBehaviorが完全に終了してから、次に再実行するまでの待機時間（秒）")]
    public float intervalDelay = 1f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (targetBehavior == null)
        {
            Debug.LogWarning("Behavior_Loop: targetBehavior is not assigned!");
            yield break;
        }

        int currentCount = 0;

        while (true)
        {
            // ターゲットとなるBehaviorを実行し、その終了を待機する
            yield return ai.StartCoroutine(targetBehavior.RunBehavior(ai));

            currentCount++;

            // 指定回数に達したらループを抜ける（0以下の場合は無限ループ）
            if (repeatCount > 0 && currentCount >= repeatCount)
            {
                break;
            }

            // 次の実行までの待機時間
            if (intervalDelay > 0f)
            {
                yield return new WaitForSeconds(intervalDelay);
            }
        }
    }
}
