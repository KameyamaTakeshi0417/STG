using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Make Barrier Behavior", menuName = "EnemyAI/Behaviors/MakeBarrier")]
public class Behavior_MakeBarrier : EnemyBehaviorData_Base
{
    [Tooltip("バリアの耐久値（EndurableDamage）")]
    public float barrierEndurableDamage = 20f;
    
    [Tooltip("バリアが破壊された後に復活するまでの時間")]
    public float barrierRespawnTime = 10f;

    [Tooltip("バリア展開時の硬直時間（展開アクションの長さ）")]
    public float actionDuration = 1.0f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        // 動きを止める
        ai.Rb.velocity = Vector2.zero;

        // Healthコンポーネントを取得
        _Health_Base health = ai.GetComponent<_Health_Base>();
        
        if (health != null)
        {
            health.isBarrierActive = true;
            health.barrierEndurableDamage = barrierEndurableDamage;
            health.barrierBaseRespawnTime = barrierRespawnTime;
            // バリアがアクティブになったので、見た目も更新（Update等で処理されるか、即座に有効化するか）
            // このスクリプトは行動中1回だけ呼ばれる想定
        }

        float timer = 0f;
        while (timer < actionDuration)
        {
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
}
