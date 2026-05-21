using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Dash Behavior", menuName = "EnemyAI/Behaviors/Shield Dash")]
public class Behavior_ShieldDash : EnemyBehaviorData_Base
{
    [Header("Dash Settings")]
    [Tooltip("突進前の予備動作（停止して狙いを定める時間）")]
    public float telegraphTime = 1.0f;
    [Tooltip("突進スピード")]
    public float dashSpeed = 15f;
    [Tooltip("突進を継続する時間")]
    public float dashDuration = 0.5f;
    [Tooltip("突進後の隙（クールダウン）")]
    public float cooldownTime = 1.0f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        while (true)
        {
            // 1. 予備動作（停止して狙いを定める）
            ai.Rb.velocity = Vector2.zero;
            Vector2 dashDirection = Vector2.zero;
            
            float timer = 0f;
            while (timer < telegraphTime)
            {
                // チャージ中は常にプレイヤーの方を向く
                if (ai.HasTarget())
                {
                    dashDirection = (ai.TargetTransform.position - ai.transform.position).normalized;
                }
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            if (dashDirection == Vector2.zero) dashDirection = Vector2.right;

            // バリアをONにする
            var barrier = ai.GetComponentInChildren<Alpha.Enemy.Alpha_DamageBarrier>();
            if (barrier != null)
            {
                barrier.SetBarrierActive(true);
            }

            // 2. 突進（高速移動）
            timer = 0f;
            while (timer < dashDuration)
            {
                ai.Rb.velocity = dashDirection * dashSpeed;
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            // バリアをOFFにする
            if (barrier != null)
            {
                barrier.SetBarrierActive(false);
            }

            // 3. 硬直（リカバリー）
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
