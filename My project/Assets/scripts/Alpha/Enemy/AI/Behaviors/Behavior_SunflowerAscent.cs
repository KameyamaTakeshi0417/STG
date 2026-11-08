using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Enemy.Weapons;

[CreateAssetMenu(fileName = "New Sunflower Ascent Behavior", menuName = "EnemyAI/Behaviors/SunflowerAscent")]
public class Behavior_SunflowerAscent : EnemyBehaviorData_Base
{
    public string attackName = "Sunflower Bloom";

    [Header("Sunflower Unit Setup")]
    [Tooltip("生成する向日葵ユニットのプレハブ（SunflowerBarrageUnit_Alpha が付いたもの）")]
    public GameObject sunflowerPrefab;

    [Header("Behavior Timing")]
    [Tooltip("この攻撃行動全体の継続時間（秒）")]
    public float behaviorDuration = 12.0f;

    [Tooltip("向日葵の生成間隔（最小・秒）")]
    [Range(0.5f, 5.0f)]
    public float spawnIntervalMin = 1.0f;

    [Tooltip("向日葵の生成間隔（最大・秒）")]
    [Range(0.5f, 5.0f)]
    public float spawnIntervalMax = 2.5f;

    [Tooltip("行動終了後のクールダウン待機時間（秒）")]
    public float cooldownAfterBehavior = 1.5f;

    [Header("Spawn Position (Screen Bottom)")]
    [Tooltip("画面下での出現X座標の最小値")]
    public float spawnXMin = -7.0f;

    [Tooltip("画面下での出現X座標の最大値")]
    public float spawnXMax = 7.0f;

    [Tooltip("画面下での出現Y座標")]
    public float spawnY = -10.0f;

    [Tooltip("画面上端での消滅Y座標")]
    public float destroyY = 12.0f;

    [Header("Optional Overrides for Sunflower")]
    [Tooltip("向日葵の上昇速度（0より大きければプレハブの設定を上書き）")]
    public float overrideRiseSpeed = 0f;

    [Tooltip("向日葵の自転速度（0より大きければプレハブの設定を上書き）")]
    public float overrideRotationSpeed = 0f;

    [Tooltip("円形弾のプレハブ（指定があればプレハブの設定を上書き）")]
    public GameObject overrideCircleBulletPrefab;

    [Tooltip("ばらまき弾のプレハブ（指定があればプレハブの設定を上書き）")]
    public GameObject overrideScatterBulletPrefab;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (ai is Alpha_EliteEnemyAI eliteAi)
        {
            eliteAi.TriggerAttackEvent(attackName);
        }

        if (sunflowerPrefab == null)
        {
            Debug.LogWarning("[Behavior_SunflowerAscent] sunflowerPrefab が未設定です。");
            yield return new WaitForSeconds(behaviorDuration);
            yield break;
        }

        float startTime = Time.time;

        while (Time.time - startTime < behaviorDuration)
        {
            // 画面下のランダムなX座標を決定
            float spawnX = Random.Range(spawnXMin, spawnXMax);
            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            // 向日葵オブジェクトを生成
            GameObject flowerObj = Instantiate(sunflowerPrefab, spawnPos, Quaternion.identity);

            // パラメータの上書き適用
            SunflowerBarrageUnit_Alpha unit = flowerObj.GetComponent<SunflowerBarrageUnit_Alpha>();
            if (unit != null)
            {
                unit.destroyY = destroyY;

                if (overrideRiseSpeed > 0f) unit.riseSpeed = overrideRiseSpeed;
                if (overrideRotationSpeed > 0f) unit.rotationSpeed = overrideRotationSpeed;
                if (overrideCircleBulletPrefab != null) unit.circleBulletPrefab = overrideCircleBulletPrefab;
                if (overrideScatterBulletPrefab != null) unit.scatterBulletPrefab = overrideScatterBulletPrefab;
            }

            // 次の生成までのインターバル（1-5秒の設定範囲でランダムまたは固定）
            float nextInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(nextInterval);
        }

        if (cooldownAfterBehavior > 0f)
        {
            yield return new WaitForSeconds(cooldownAfterBehavior);
        }
    }
}
