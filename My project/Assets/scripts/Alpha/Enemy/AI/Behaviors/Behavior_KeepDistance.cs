using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Keep Distance Behavior", menuName = "EnemyAI/Behaviors/Keep Distance")]
public class Behavior_KeepDistance : EnemyBehaviorData_Base
{
    public float targetDistance = 5f;
    public float hysteresis = 1f; // ガタつき防止用の遊び幅
    public float moveSpeed = 3f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        while (true)
        {
            if (!ai.HasTarget())
            {
                ai.Rb.velocity = Vector2.zero;
                yield return new WaitForFixedUpdate();
                continue;
            }

            Vector2 toTarget = ai.TargetTransform.position - ai.transform.position;
            float currentDistance = toTarget.magnitude;
            
            // 目標距離に対しての誤差
            float difference = currentDistance - targetDistance;

            if (Mathf.Abs(difference) > hysteresis)
            {
                // ヒステリシスの外に出た場合、目標距離に戻る方向に移動
                float direction = Mathf.Sign(difference); // 1: 近づく, -1: 離れる
                ai.Rb.velocity = toTarget.normalized * (moveSpeed * direction);
            }
            else
            {
                // ヒステリシス範囲内なら緩やかに停止（慣性を殺す）
                ai.Rb.velocity = Vector2.MoveTowards(ai.Rb.velocity, Vector2.zero, moveSpeed * Time.fixedDeltaTime * 5f);
            }

            yield return new WaitForFixedUpdate();
        }
    }
}
