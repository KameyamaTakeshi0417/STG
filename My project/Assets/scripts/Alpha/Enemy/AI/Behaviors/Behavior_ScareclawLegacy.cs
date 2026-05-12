using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scareclaw Legacy Behavior", menuName = "EnemyAI/Behaviors/Scareclaw Legacy")]
public class Behavior_ScareclawLegacy : EnemyBehaviorData_Base
{
    public enum MoveType { Horizontal, Orbital }
    public MoveType moveType = MoveType.Horizontal;

    public float moveRadius = 3f;
    public float moveSpeed = 2f;
    public float returnSpeed = 5f; // 開始位置へ戻る速度

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        // 開始想定位置の計算
        Vector3 targetPos = ai.InitialPosition;
        if (moveType == MoveType.Orbital)
        {
            targetPos = ai.InitialPosition + new Vector3(moveRadius, 0, 0);
        }

        // フェーズ1: 開始位置への復帰 (Return)
        while (Vector3.Distance(ai.transform.position, targetPos) >= 0.05f)
        {
            Vector3 nextPos = Vector3.Lerp(ai.transform.position, targetPos, returnSpeed * Time.fixedDeltaTime);
            // MovePositionを利用して物理と同期
            ai.Rb.MovePosition(nextPos);
            yield return new WaitForFixedUpdate();
        }
        ai.Rb.MovePosition(targetPos);

        // フェーズ2: 本動作 (Moving)
        float moveProgress = 0f;
        while (true)
        {
            moveProgress += moveSpeed * Time.fixedDeltaTime;
            Vector3 nextPos;

            if (moveType == MoveType.Horizontal)
            {
                float offset = Mathf.Sin(moveProgress) * moveRadius;
                nextPos = ai.InitialPosition + new Vector3(offset, 0, 0);
            }
            else
            {
                float x = Mathf.Cos(moveProgress) * moveRadius;
                float y = Mathf.Sin(moveProgress) * moveRadius;
                nextPos = ai.InitialPosition + new Vector3(x, y, 0);
            }

            ai.Rb.MovePosition(Vector3.Lerp(ai.transform.position, nextPos, returnSpeed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }
    }
}
