using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New MoveToPoint Behavior", menuName = "EnemyAI/Behaviors/Move To Point")]
public class Behavior_MoveToPoint : EnemyBehaviorData_Base
{
    [Tooltip("目的地")]
    public Vector2 targetPosition;
    
    [Tooltip("移動速度")]
    public float moveSpeed = 5f;
    
    [Tooltip("この距離以内に入ったら到着とみなす")]
    public float stopDistance = 0.1f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        while (true)
        {
            Vector2 currentPos = ai.transform.position;
            float distance = Vector2.Distance(currentPos, targetPosition);

            if (distance > stopDistance)
            {
                // 目的地へ向かって移動
                Vector2 direction = (targetPosition - currentPos).normalized;
                ai.Rb.velocity = direction * moveSpeed;
            }
            else
            {
                // 到着したら停止
                ai.Rb.velocity = Vector2.zero;
            }

            yield return new WaitForFixedUpdate();
        }
    }
}
