using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New ZigZag Move Behavior", menuName = "EnemyAI/Behaviors/ZigZagMove")]
public class Behavior_ZigZagMove : EnemyBehaviorData_Base
{
    public string movementName = "ZigZag Move";

    [Header("Movement Area Settings")]
    [Tooltip("移動可能領域の中心座標（ワールド座標または初期位置からの相対座標として扱うかは useRelativeBounds で設定）")]
    public Vector2 areaCenter = Vector2.zero;
    [Tooltip("移動可能領域のサイズ")]
    public Vector2 areaSize = new Vector2(10f, 10f);
    [Tooltip("trueの場合、エネミーが生成された位置（InitialPosition）を基準に移動領域を計算します")]
    public bool useRelativeArea = true;

    [Header("ZigZag Settings")]
    [Tooltip("移動速度")]
    public float speed = 5f;
    [Tooltip("目標地点に到達した後の待機時間（秒）")]
    public float waitTimeAtTarget = 0.5f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (ai is Alpha_EliteEnemyAI eliteAi)
        {
            // 移動開始のイベントがあれば呼ぶ（今回は特に無くてもOK）
            // eliteAi.TriggerAttackEvent(movementName);
        }

        Vector2 basePosition = useRelativeArea ? (Vector2)ai.InitialPosition : Vector2.zero;
        Vector2 minBounds = basePosition + areaCenter - (areaSize / 2f);
        Vector2 maxBounds = basePosition + areaCenter + (areaSize / 2f);

        Vector2 currentTarget = GetRandomPositionInBounds(minBounds, maxBounds);
        float timer = 0f;

        while (true)
        {
            Vector2 currentPos = ai.transform.position;
            Vector2 toTarget = currentTarget - currentPos;
            
            // 目標までの距離が十分近い場合、待機してから次の目的地へ
            if (toTarget.sqrMagnitude < 0.01f)
            {
                if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;

                timer -= Time.fixedDeltaTime;
                if (timer <= 0f)
                {
                    currentTarget = GetRandomPositionInBounds(minBounds, maxBounds);
                    timer = waitTimeAtTarget;
                }
            }
            else
            {
                // 目標に向かってスムーズに移動（瞬間移動や物理演算のバグを防ぐためMoveTowardsを使用）
                ai.transform.position = Vector3.MoveTowards(currentPos, currentTarget, speed * Time.fixedDeltaTime);
                if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private Vector2 GetRandomPositionInBounds(Vector2 min, Vector2 max)
    {
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        return new Vector2(x, y);
    }
}
