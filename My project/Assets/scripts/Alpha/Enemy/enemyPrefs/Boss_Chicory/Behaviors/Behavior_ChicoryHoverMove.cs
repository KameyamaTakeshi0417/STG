using System.Collections;
using UnityEngine;

namespace Alpha.Enemy.Chicory
{
    /// <summary>
    /// チコリ専用の浮遊・巡航移動ビヘイビア
    /// フィールド上部〜中央エリア（画面X: -6〜6, Y: 1〜4など）をなめらかにフワフワ移動する
    /// </summary>
    [CreateAssetMenu(fileName = "Behavior_ChicoryHoverMove", menuName = "EnemyAI/Behaviors/Chicory Hover Move")]
    public class Behavior_ChicoryHoverMove : EnemyBehaviorData_Base
    {
        [Header("Movement Range")]
        [SerializeField] private Vector2 xRange = new Vector2(-5f, 5f);
        [SerializeField] private Vector2 yRange = new Vector2(1f, 4f);
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float waitTimeAtPoint = 1.2f;

        public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
        {
            if (ai == null) yield break;

            while (true)
            {
                // 次の目標地点をランダムに決定
                Vector2 targetPos = new Vector2(
                    Random.Range(xRange.x, xRange.y),
                    Random.Range(yRange.x, yRange.y)
                );

                // なめらかに移動
                while (ai != null && Vector2.Distance(ai.transform.position, targetPos) > 0.15f)
                {
                    Vector2 currentPos = ai.transform.position;
                    Vector2 dir = (targetPos - currentPos).normalized;
                    
                    if (ai.Rb != null)
                    {
                        ai.Rb.velocity = dir * moveSpeed;
                    }
                    else
                    {
                        ai.transform.position = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
                    }
                    yield return null;
                }

                if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;

                // 到着後の待機
                yield return new WaitForSeconds(waitTimeAtPoint);
            }
        }
    }
}
