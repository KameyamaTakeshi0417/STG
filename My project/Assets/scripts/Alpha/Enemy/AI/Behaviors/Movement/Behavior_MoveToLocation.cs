using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Behavior_MoveToLocation", menuName = "Alpha/Enemy AI/Behaviors/Movement/Move To Location")]
public class Behavior_MoveToLocation : EnemyBehaviorData_Base
{
    [Tooltip("移動先の座標")]
    public Vector2 targetLocation = new Vector2(0, 3f);
    
    [Tooltip("TargetLocationをボスのスポーン初期位置からの相対座標として扱うか")]
    public bool isRelative = false;
    
    [Tooltip("移動速度")]
    public float moveSpeed = 5f;

    [Tooltip("到着後も物理演算を無効化してその場に留まり続けるか")]
    public bool lockPositionAfterArrival = true;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        // 到着判定のしきい値
        float threshold = 0.1f;
        
        // 実際の目的地を計算
        Vector2 actualTarget = targetLocation;
        if (isRelative)
        {
            actualTarget = (Vector2)ai.InitialPosition + targetLocation;
        }

        while (Vector2.Distance(ai.transform.position, actualTarget) > threshold)
        {
            // Rigidbodyがある場合は速度で移動（最も安定する）
            if (ai.Rb != null)
            {
                Vector2 dir = (actualTarget - (Vector2)ai.transform.position).normalized;
                // 到着寸前のオーバーシュートを防ぐため、距離に応じて速度を落とす等の処理も可能だが、
                // 一旦一定速度で向かわせる
                ai.Rb.velocity = dir * moveSpeed;

                // 距離が threshold 以下なら停止（ループを抜ける）
                if (Vector2.Distance(ai.transform.position, actualTarget) <= moveSpeed * Time.fixedDeltaTime)
                {
                    break;
                }
                yield return new WaitForFixedUpdate(); // 物理挙動と同期
            }
            else
            {
                // Rigidbodyがない場合はTransformを直接操作
                ai.transform.position = Vector3.MoveTowards(ai.transform.position, actualTarget, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // 到着後、速度をリセットして座標をピッタリ合わせる
        if (ai.Rb != null)
        {
            ai.Rb.velocity = Vector2.zero;
        }
        ai.transform.position = actualTarget;

        // 行動を終了せずにその場に留まり続ける
        while (true)
        {
            if (lockPositionAfterArrival)
            {
                if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;
                ai.transform.position = actualTarget;
            }
            yield return null;
        }
    }
}
