using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SummonPointData
{
    [Tooltip("敵からプレイヤーへ向かう方向の距離（＋なら前、－なら後ろ）。0なら真横になります。")]
    public float parallelDistance = 0f;
    
    [Tooltip("上記ベクトルに対して垂直な方向の距離（＋なら右、－なら左）")]
    public float orthogonalOffset = 2f;
    
    [Tooltip("サーキュレーターの回転方向（1 = 右回り, -1 = 左回り）など、用途に応じて使用")]
    public float rotationDirection = 1f;

    [Tooltip("召喚されたサーキュレーターの移動方向を、エネミー→プレイヤー間のベクトルに平行にするか")]
    public bool moveParallelToPlayerVector = true;

    [Tooltip("サーキュレーターの移動距離(PingPongの幅)を、エネミーからプレイヤーまでの距離と同じにするか")]
    public bool matchPlayerDistance = true;

    [Header("Screen Side Vertical Patrol")]
    [Tooltip("ONにすると画面の左端または右端に沿って上下に動き続けます")]
    public bool useScreenSideVerticalPatrol = false;
    [Tooltip("配置する画面端（-1: 左端, +1: 右端）")]
    public int screenSide = -1;
    [Tooltip("左右の画面端からの内側マージン（Viewport比率）")]
    public float screenEdgeMarginX = 0.08f;
    [Tooltip("上下の画面端からの内側マージン（Viewport比率）")]
    public float screenEdgeMarginY = 0.15f;
    [Tooltip("上下往復時の移動速度（0以下でプレハブのmoveSpeedを使用）")]
    public float screenPatrolSpeed = 8f;
    [Tooltip("初期Y進行度（0=下端, 0.5=中央, 1=上端）")]
    [Range(0f, 1f)] public float initialVerticalProgress = 0.5f;
    [Tooltip("初期の移動方向（true=上方向へ, false=下方向へ）")]
    public bool startMovingUp = true;
}

[CreateAssetMenu(fileName = "New Summon Behavior", menuName = "EnemyAI/Behaviors/Summon")]
public class Behavior_Summon : EnemyBehaviorData_Base
{
    public string summonActionName = "Summon Minions";
    
    [Header("Summon Parameters")]
    public GameObject summonPrefab;
    public float summonInterval = 5f;
    
    [Tooltip("一度だけ召喚して終了するかどうか（チェックを外すとインターバルごとに無限に召喚し続けます）")]
    public bool spawnOnlyOnce = true;

    [Tooltip("召喚する位置と設定のリスト。このリストの数だけ同時に召喚されます。")]
    public List<SummonPointData> summonPoints = new List<SummonPointData>();

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (ai is Alpha_EliteEnemyAI eliteAi)
        {
            eliteAi.TriggerAttackEvent(summonActionName);
        }

        while (true)
        {
            yield return new WaitForSeconds(summonInterval);

            if (summonPrefab == null || summonPoints.Count == 0) continue;

            // プレイヤーの方向を取得
            Vector3 playerPos = ai.TargetTransform != null ? ai.TargetTransform.position : ai.transform.position + Vector3.down;
            Vector3 toPlayer = (playerPos - ai.transform.position).normalized;
            
            // プレイヤーと重なっているなどベクトルが取れない場合のフォールバック
            if (toPlayer == Vector3.zero) toPlayer = Vector3.down;

            // 進行方向に対して垂直なベクトル（右方向）
            Vector3 orthogonal = new Vector3(-toPlayer.y, toPlayer.x, 0);

            // リストに登録された各ポイントに対して召喚を実行
            foreach (var point in summonPoints)
            {
                // エネミーの現在地 ＋ 平行移動 ＋ 垂直移動
                Vector3 spawnPos = ai.transform.position 
                                 + (toPlayer * point.parallelDistance) 
                                 + (orthogonal * point.orthogonalOffset);

                GameObject spawnedObj = null;
                if (Alpha_ObjectPoolManager.Instance != null)
                {
                    spawnedObj = Alpha_ObjectPoolManager.Instance.Rent(summonPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    spawnedObj = Instantiate(summonPrefab, spawnPos, Quaternion.identity);
                }

                if (spawnedObj != null)
                {
                    // フェーズ終了時に消せるようにAI側に記録しておく
                    ai.PhaseSpawnedObjects.Add(spawnedObj);

                    CirculatorEnemy circulator = spawnedObj.GetComponent<CirculatorEnemy>();
                    if (circulator != null)
                    {
                        // 常に絶対値に符号を掛けることで確実に向きを設定
                        circulator.angularSpeed = Mathf.Abs(circulator.angularSpeed) * Mathf.Sign(point.rotationDirection == 0 ? 1 : point.rotationDirection);

                        if (point.useScreenSideVerticalPatrol)
                        {
                            // 画面の左端または右端に沿って上下に動き続けるモードを設定
                            circulator.ConfigureScreenSidePatrol(
                                point.screenSide,
                                point.screenEdgeMarginX,
                                point.screenEdgeMarginY,
                                point.screenPatrolSpeed,
                                point.initialVerticalProgress,
                                point.startMovingUp
                            );
                        }
                        else
                        {
                            circulator.useScreenSideVerticalPatrol = false;

                            // 移動方向をプレイヤーとのベクトルに平行にする
                            if (point.moveParallelToPlayerVector)
                            {
                                circulator.moveDirection = toPlayer;
                                
                                // プレイヤーまでの距離を取得して移動距離を上書きする
                                if (point.matchPlayerDistance)
                                {
                                    circulator.moveDistance = Vector3.Distance(ai.transform.position, playerPos);
                                }

                                circulator.InitializePosition(); // 方向や距離を変えたので再計算させる
                            }
                        }
                    }
                }
            }

            // 1度きりの生成ならループを抜ける
            if (spawnOnlyOnce)
            {
                yield break;
            }
        }
    }
}
