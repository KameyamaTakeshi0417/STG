using System.Collections;
using UnityEngine;

namespace Alpha.Enemy.AI.Behaviors
{
    [CreateAssetMenu(fileName = "New ChessMove Behavior", menuName = "EnemyAI/Behaviors/Chess Move")]
    public class Behavior_ChessMove_Alpha : EnemyBehaviorData_Base
    {
        public enum MoveType { Rook, Bishop, Knight }

        [Header("Chess Move Settings")]
        public MoveType moveType = MoveType.Rook;

        [Tooltip("移動距離（Knightの場合は現在地を中心とした円の半径）")]
        public float distance = 3f;

        [Tooltip("移動にかける時間")]
        public float moveDuration = 0.5f;

        [Tooltip("移動間の待機時間")]
        public float waitTime = 1.0f;

        [Header("Attack Settings")]
        [Tooltip("何回の移動ごとに攻撃を発動するか（0なら移動開始時に一度だけ発動し、常に並行して攻撃する）")]
        public int attackTriggerCount = 1;

        [Tooltip("発動する攻撃Behavior")]
        public EnemyBehaviorData_Base attackBehavior;

        public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
        {
            int moveCounter = 0;

            // n = 0 の場合、開始時に攻撃を並行して起動する
            if (attackTriggerCount == 0 && attackBehavior != null)
            {
                ai.StartBehavior(Alpha_EnemyAI.BehaviorSlot.Attack, attackBehavior);
            }

            while (true)
            {
                // 1. 次の目標位置を決定
                Vector3 currentPos = ai.transform.position;
                Vector3 targetPos = GetTargetPosition(currentPos, moveType);

                // 画面外に出ないように制限
                targetPos = Alpha.Core.ScreenBoundaryManager_Alpha.Instance.ClampPositionToScreen(targetPos);

                // 2. 移動 (ワープ or 移動)
                if (moveType == MoveType.Knight)
                {
                    // ワープ移動
                    ai.Rb.MovePosition(targetPos);
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    // 一定時間かけて移動
                    float timer = 0f;
                    while (timer < moveDuration)
                    {
                        timer += Time.fixedDeltaTime;
                        float t = Mathf.Clamp01(timer / moveDuration);
                        Vector3 nextPos = Vector3.Lerp(currentPos, targetPos, t);
                        ai.Rb.MovePosition(nextPos);
                        yield return new WaitForFixedUpdate();
                    }
                    ai.Rb.MovePosition(targetPos); // 最終位置を確定
                }

                // 3. 移動完了後の処理
                moveCounter++;

                // n >= 1 かつ 指定回数に達した場合、攻撃を発動して待機
                if (attackTriggerCount > 0 && moveCounter >= attackTriggerCount && attackBehavior != null)
                {
                    moveCounter = 0;
                    
                    // 攻撃を起動
                    ai.StartBehavior(Alpha_EnemyAI.BehaviorSlot.Attack, attackBehavior);

                    // 攻撃が終了するまで待機（完全な移動停止）
                    while (ai.CurrentAttackBehavior != null)
                    {
                        // ターゲット方向を向き続ける処理が必要な場合はここに追加
                        // 現在は単に待機
                        ai.Rb.velocity = Vector2.zero;
                        yield return new WaitForFixedUpdate();
                    }
                }

                // 4. 指定時間待機
                float waitTimer = 0f;
                while (waitTimer < waitTime)
                {
                    ai.Rb.velocity = Vector2.zero; // 完全に停止
                    waitTimer += Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        private Vector3 GetTargetPosition(Vector3 currentPos, MoveType type)
        {
            Vector3 direction = Vector3.zero;

            switch (type)
            {
                case MoveType.Rook:
                    // 上下左右の4方向からランダムに1つ選ぶ
                    int rookDir = Random.Range(0, 4);
                    if (rookDir == 0) direction = Vector3.up;
                    else if (rookDir == 1) direction = Vector3.down;
                    else if (rookDir == 2) direction = Vector3.left;
                    else direction = Vector3.right;
                    break;

                case MoveType.Bishop:
                    // 斜め4方向からランダムに1つ選ぶ
                    int bishopDir = Random.Range(0, 4);
                    if (bishopDir == 0) direction = new Vector3(1, 1, 0).normalized;
                    else if (bishopDir == 1) direction = new Vector3(1, -1, 0).normalized;
                    else if (bishopDir == 2) direction = new Vector3(-1, 1, 0).normalized;
                    else direction = new Vector3(-1, -1, 0).normalized;
                    break;

                case MoveType.Knight:
                    // 360度ランダムな方向
                    float angle = Random.Range(0f, 360f);
                    direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
                    break;
            }

            return currentPos + direction * distance;
        }
    }
}
