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

            Vector2 currentPos = ai.transform.position;
            Vector2 playerPos = ai.TargetTransform.position;
            
            // プレイヤーからエネミーへの方向
            Vector2 playerToEnemy = currentPos - playerPos;
            
            // 距離が近すぎる・完全に重なっている場合のフェイルセーフ
            if (playerToEnemy.sqrMagnitude < 0.01f)
            {
                playerToEnemy = Vector2.up; 
            }

            // 理想の立ち位置（プレイヤーから targetDistance 離れた位置）
            Vector2 desiredPos = playerPos + playerToEnemy.normalized * targetDistance;

            // 画面外に出ないように目標位置をクランプ（壁に押し付けられるのを防ぐ）
            if (Alpha.Core.ScreenBoundaryManager_Alpha.Instance != null)
            {
                desiredPos = Alpha.Core.ScreenBoundaryManager_Alpha.Instance.ClampPositionToScreen(desiredPos);
            }

            // 現在地から目標位置へのベクトル
            Vector2 moveDir = desiredPos - currentPos;

            // ヒステリシス（遊び幅）を超えている場合のみ移動
            if (moveDir.magnitude > hysteresis)
            {
                ai.Rb.velocity = moveDir.normalized * moveSpeed;
            }
            else
            {
                // 目標位置に十分近いなら緩やかに停止
                ai.Rb.velocity = Vector2.MoveTowards(ai.Rb.velocity, Vector2.zero, moveSpeed * Time.fixedDeltaTime * 5f);
            }

            yield return new WaitForFixedUpdate();
        }
    }
}
