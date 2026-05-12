using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dash Behavior", menuName = "EnemyAI/Behaviors/Dash")]
public class Behavior_Dash : EnemyBehaviorData_Base
{
    public float chargeTime = 1.0f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;
    public float recoverTime = 1.0f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        while (true)
        {
            // 1. 予備動作（停止して狙いを定める）
            ai.Rb.velocity = Vector2.zero;
            Vector2 dashDirection = Vector2.zero;
            
            float timer = 0f;
            while (timer < chargeTime)
            {
                // チャージ中は常にプレイヤーの方を向く
                if (ai.HasTarget())
                {
                    dashDirection = (ai.TargetTransform.position - ai.transform.position).normalized;
                }
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            // ターゲットがいなければ右向きなどにデフォルト化
            if (dashDirection == Vector2.zero) dashDirection = Vector2.right;

            // 2. 突進（高速移動）
            timer = 0f;
            while (timer < dashDuration)
            {
                ai.Rb.velocity = dashDirection * dashSpeed;
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            // 3. 硬直（リカバリー）
            ai.Rb.velocity = Vector2.zero;
            timer = 0f;
            while (timer < recoverTime)
            {
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
