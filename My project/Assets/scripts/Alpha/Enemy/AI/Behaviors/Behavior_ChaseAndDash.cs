using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chase And Dash Behavior", menuName = "EnemyAI/Behaviors/Chase And Dash")]
public class Behavior_ChaseAndDash : EnemyBehaviorData_Base
{
    [Header("Chase Settings")]
    public float chaseSpeed = 3f;
    [Tooltip("この距離以内に入ったら突進の予備動作を開始する")]
    public float dashTriggerDistance = 5f;

    [Header("Dash Settings")]
    [Tooltip("突進前の予備動作（停止して狙いを定める時間）")]
    public float telegraphTime = 1.0f;
    [Tooltip("突進スピード")]
    public float dashSpeed = 15f;
    [Tooltip("突進を継続する時間")]
    public float dashDuration = 0.3f;
    [Tooltip("突進後の隙（クールダウン）")]
    public float cooldownTime = 1.0f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        while (true)
        {
            // ターゲットがいない場合は停止して待機
            if (!ai.HasTarget())
            {
                ai.Rb.velocity = Vector2.zero;
                yield return new WaitForFixedUpdate();
                continue;
            }

            Vector2 toTarget = ai.TargetTransform.position - ai.transform.position;
            float distance = toTarget.magnitude;

            // 1. チェイス状態
            if (distance > dashTriggerDistance)
            {
                ai.Rb.velocity = toTarget.normalized * chaseSpeed;
                yield return new WaitForFixedUpdate();
            }
            else
            {
                // 2. 突進圏内に入った：予備動作（Telegraph）
                ai.Rb.velocity = Vector2.zero;
                Vector2 dashDirection = Vector2.zero;
                float timer = 0f;

                while (timer < telegraphTime)
                {
                    // 予備動作中も常にプレイヤーの方を向く（狙いを更新し続ける場合）
                    if (ai.HasTarget())
                    {
                        dashDirection = (ai.TargetTransform.position - ai.transform.position).normalized;
                    }
                    timer += Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }

                if (dashDirection == Vector2.zero) dashDirection = Vector2.right;

                // 3. 突進（Dash）
                timer = 0f;
                while (timer < dashDuration)
                {
                    ai.Rb.velocity = dashDirection * dashSpeed;
                    timer += Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }

                // 4. 硬直（Cooldown）
                ai.Rb.velocity = Vector2.zero;
                timer = 0f;
                while (timer < cooldownTime)
                {
                    timer += Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }
            }
        }
    }
}
