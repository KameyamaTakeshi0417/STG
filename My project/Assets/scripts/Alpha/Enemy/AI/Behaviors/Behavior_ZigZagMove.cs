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

    [Tooltip("移動可能領域のサイズの最低値")]
    public Vector2 areaMinimumSize =new Vector2(0f,0f);
    [Tooltip("trueの場合、エネミーが生成された位置（InitialPosition）を基準に移動領域を計算します")]
    public bool useRelativeArea = true;

    [Header("ZigZag Settings")]
    [Tooltip("移動速度")]
    public float speed = 5f;
    [Tooltip("移動前の待機時間（この間、ディレクションラインが表示されます）")]
    public float waitTimeAtTarget = 0.5f;

    [Header("Visual Settings")]
    [Tooltip("移動前にディレクションライン（予兆）を表示するかどうか")]
    public bool showDirectionLine = true;
    public float lineWidth = 0.05f;
    public Color lineColor = new Color(1f, 0.5f, 0f, 0.5f);

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

        Vector2 currentTarget = GetRandomPositionInBounds(minBounds+areaMinimumSize, maxBounds);
        bool isWaiting = true;
        float timer = waitTimeAtTarget;

        LineRenderer lr = null;

        try
        {
            while (true)
            {
                if (showDirectionLine && lr == null && ai != null && ai.gameObject != null)
                {
                    // ライン専用のオブジェクトを子として生成
                    GameObject lineObj = new GameObject("ZigZag_DirectionLine");
                    lineObj.transform.SetParent(ai.transform);
                    
                    lr = lineObj.AddComponent<LineRenderer>();
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    lr.startWidth = lineWidth;
                    lr.endWidth = lineWidth;
                    lr.startColor = lineColor;
                    lr.endColor = lineColor;
                    lr.positionCount = 2;
                    lr.useWorldSpace = true;
                    lr.sortingOrder = -10;
                    lr.enabled = false;

                    // フェーズ終了時等に安全に破棄されるようリストに追加
                    if (ai is Alpha_EliteEnemyAI bossAi)
                    {
                        bossAi.PhaseSpawnedObjects.Add(lineObj);
                    }
                }

                Vector2 currentPos = ai.transform.position;
                Vector2 toTarget = currentTarget - currentPos;

                if (isWaiting)
                {

                    if (ai.Rb != null)
                    {
                        ai.Rb.velocity = Vector2.zero;
                        ai.Rb.angularVelocity = 0f; // 回転も止めたいなら
                    }
                    timer -= Time.fixedDeltaTime;

                    if (showDirectionLine && lr != null)
                    {
                        lr.enabled = true;
                        lr.SetPosition(0, currentPos);
                        lr.SetPosition(1, currentTarget);
                    }

                    if (timer <= 0f)
                    {
                        isWaiting = false;
                        if (lr != null) lr.enabled = false; // 移動直前にラインを消す
                    }
                }
                else
                {
                    // 目標までの距離が十分近い場合、待機してから次の目的地へ
                    if (toTarget.sqrMagnitude < 0.01f)
                    {
                        if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;
                        currentTarget = GetRandomPositionInBounds(minBounds, maxBounds);
                        isWaiting = true;
                        timer = waitTimeAtTarget;
                    }
                    else
                    {
                        // 目標に向かってスムーズに移動
                        ai.transform.position = Vector3.MoveTowards(currentPos, currentTarget, speed * Time.fixedDeltaTime);
                        if (ai.Rb != null) ai.Rb.velocity = Vector2.zero;
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }
        finally
        {
            // コルーチンが正常終了した場合のクリーンアップ（フェーズ移行のStopCoroutine時は呼ばれないことがあるため、PhaseSpawnedObjectsでの管理がメイン）
            if (lr != null && lr.gameObject != null)
            {
                Destroy(lr.gameObject);
            }
        }
    }

    private Vector2 GetRandomPositionInBounds(Vector2 min, Vector2 max)
    {
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        return new Vector2(x, y);
    }
}
