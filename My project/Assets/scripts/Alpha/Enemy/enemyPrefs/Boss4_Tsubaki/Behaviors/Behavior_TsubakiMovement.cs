using System.Collections;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// ツバキ専用の移動ビヘイビア
    /// 緩やかな浮遊移動でプレイヤーと適正距離を維持しつつ、
    /// Phase 3 や特定状況ではシャープな残像ダッシュを行う。
    /// </summary>
    [CreateAssetMenu(fileName = "Behavior_TsubakiMovement", menuName = "EnemyAI/Behaviors/Tsubaki Movement")]
    public class Behavior_TsubakiMovement : EnemyBehaviorData_Base
    {
        [Header("Movement Range")]
        [SerializeField] private Vector2 xRange = new Vector2(-5.5f, 5.5f);
        [SerializeField] private Vector2 yRange = new Vector2(1f, 3.8f);
        [SerializeField] private float moveSpeed = 3.8f;
        [SerializeField] private float waitTimeAtPoint = 1.0f;
        [SerializeField] private bool enableAfterimageDash = false;
        [SerializeField] private float dashSpeed = 12f;

        public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
        {
            if (ai == null) yield break;

            while (true)
            {
                // 目標地点を決定
                Vector2 targetPos = new Vector2(
                    Random.Range(xRange.x, xRange.y),
                    Random.Range(yRange.x, yRange.y)
                );

                bool doDash = enableAfterimageDash && (Random.value < 0.4f);

                if (doDash)
                {
                    // 残像ダッシュ
                    yield return ai.StartCoroutine(DashRoutine(ai, targetPos));
                }
                else
                {
                    // 通常の滑らかな移動
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

                    if (ai != null && ai.Rb != null)
                    {
                        ai.Rb.velocity = Vector2.zero;
                    }
                }

                yield return new WaitForSeconds(waitTimeAtPoint);
            }
        }

        private IEnumerator DashRoutine(Alpha_EnemyAI ai, Vector2 targetPos)
        {
            float elapsed = 0f;
            float duration = Vector2.Distance(ai.transform.position, targetPos) / dashSpeed;
            Vector2 startPos = ai.transform.position;

            while (elapsed < duration && ai != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // シャープなイージング
                float ease = Mathf.SmoothStep(0f, 1f, t);

                if (ai.Rb != null)
                {
                    ai.Rb.MovePosition(Vector2.Lerp(startPos, targetPos, ease));
                }
                else
                {
                    ai.transform.position = Vector2.Lerp(startPos, targetPos, ease);
                }
                yield return null;
            }

            if (ai != null && ai.Rb != null) ai.Rb.velocity = Vector2.zero;
        }
    }
}
