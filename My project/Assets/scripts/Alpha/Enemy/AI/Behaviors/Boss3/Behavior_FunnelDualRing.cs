using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Enemy.Weapons;

[CreateAssetMenu(fileName = "Behavior_FunnelDualRing", menuName = "Alpha/Enemy AI/Behaviors/Boss3/Funnel Dual Ring")]
public class Behavior_FunnelDualRing : EnemyBehaviorData_Base
{
    [Header("Boss Movement")]
    public Vector2 centerPos = new Vector2(0, 3f);
    public float bossMoveSpeed = 5f;

    [Header("Funnels Setup")]
    public GameObject funnelPrefab; // Alpha_FunnelControllerがアタッチされたプレハブ
    public float innerRadius = 3f;
    public float outerRadius = 6f;
    public float innerOrbitSpeed = 60f;  // 時計回り (正)
    public float outerOrbitSpeed = -45f; // 反時計回り (負)

    [Header("Laser Setup")]
    public float laserLength = 30f;
    public float laserThickness = 1.5f;
    public float laserExpandTime = 0.3f;
    public float laserDamage = 1f;

    [Header("Laser Way Setup")]
    public int wayCount = 2;
    public float spreadAngle = 15f;

    [Header("Smart Missile Burst")]
    public GameObject smartMissilePrefab;
    public int missileCount = 12;
    public float missileBurstInterval = 0.1f; // 発射間隔
    public float missileCooldown = 10f;       // 発射後のクールタイム

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        // 1. ボスを中央に移動
        while (Vector2.Distance(ai.transform.position, centerPos) > 0.1f)
        {
            ai.transform.position = Vector3.MoveTowards(ai.transform.position, centerPos, bossMoveSpeed * Time.deltaTime);
            yield return null;
        }

        if (funnelPrefab == null) yield break;

        // 2. ファンネルの生成
        List<Alpha_FunnelController> innerFunnels = new List<Alpha_FunnelController>();
        List<Alpha_FunnelController> outerFunnels = new List<Alpha_FunnelController>();

        for (int i = 0; i < 2; i++)
        {
            GameObject objIn = Instantiate(funnelPrefab, ai.transform.position, Quaternion.identity);
            innerFunnels.Add(objIn.GetComponent<Alpha_FunnelController>());

            GameObject objOut = Instantiate(funnelPrefab, ai.transform.position, Quaternion.identity);
            outerFunnels.Add(objOut.GetComponent<Alpha_FunnelController>());
        }

        // 3. 各種コルーチンの起動
        ai.StartCoroutine(DualRingRoutine(innerFunnels, outerFunnels, ai.transform));
        ai.StartCoroutine(SmartMissileRoutine(ai));

        // フェーズ終了まで無限ループ
        while (true)
        {
            yield return null;
        }
    }

    private IEnumerator DualRingRoutine(List<Alpha_FunnelController> inner, List<Alpha_FunnelController> outer, Transform center)
    {
        float innerAngle = 0f;
        float outerAngle = 0f;

        // レーザーを発射しっぱなしにする（一度だけ呼ぶ）
        foreach (var f in inner)
        {
            if (f != null) f.FireLasers(wayCount, spreadAngle, laserLength, laserThickness, laserExpandTime, laserDamage);
        }
        foreach (var f in outer)
        {
            if (f != null) f.FireLasers(wayCount, spreadAngle, laserLength, laserThickness, laserExpandTime, laserDamage);
        }

        while (true)
        {
            innerAngle += innerOrbitSpeed * Time.deltaTime;
            outerAngle += outerOrbitSpeed * Time.deltaTime;

            // 内側（時計回り・外向き）
            for (int i = 0; i < inner.Count; i++)
            {
                var f = inner[i];
                if (f == null) continue;

                float offset = (360f / inner.Count) * i;
                float angleRad = (innerAngle + offset) * Mathf.Deg2Rad;
                Vector2 pos = (Vector2)center.position + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * innerRadius;
                f.SetTargetPosition(pos);

                // 外側を向く（中心から離れる方向）
                Vector2 outDir = (pos - (Vector2)center.position).normalized;
                float rotAngle = Mathf.Atan2(outDir.y, outDir.x) * Mathf.Rad2Deg - 90f;
                f.SetTargetRotation(Quaternion.AngleAxis(rotAngle, Vector3.forward));
            }

            // 外側（反時計回り・内向き）
            for (int i = 0; i < outer.Count; i++)
            {
                var f = outer[i];
                if (f == null) continue;

                float offset = (360f / outer.Count) * i;
                float angleRad = (outerAngle + offset) * Mathf.Deg2Rad;
                Vector2 pos = (Vector2)center.position + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * outerRadius;
                f.SetTargetPosition(pos);

                // 内側を向く（中心へ向かう方向）
                Vector2 inDir = ((Vector2)center.position - pos).normalized;
                float rotAngle = Mathf.Atan2(inDir.y, inDir.x) * Mathf.Rad2Deg - 90f;
                f.SetTargetRotation(Quaternion.AngleAxis(rotAngle, Vector3.forward));
            }

            yield return null;
        }
    }

    private IEnumerator SmartMissileRoutine(Alpha_EnemyAI ai)
    {
        while (true)
        {
            if (smartMissilePrefab != null)
            {
                // ミサイルタイプのリストを作成（最低1つずつ含む）
                List<Alpha_SmartMissile.MissileType> missileTypes = new List<Alpha_SmartMissile.MissileType>();
                missileTypes.Add(Alpha_SmartMissile.MissileType.SmallHoming);
                missileTypes.Add(Alpha_SmartMissile.MissileType.EliteHoming);
                missileTypes.Add(Alpha_SmartMissile.MissileType.Baka);
                
                // 残りをランダムに追加
                for (int i = 3; i < missileCount; i++)
                {
                    missileTypes.Add((Alpha_SmartMissile.MissileType)Random.Range(1, 4)); // 1:SmallHoming, 2:EliteHoming, 3:Baka
                }

                // シャッフル
                for (int i = 0; i < missileTypes.Count; i++)
                {
                    Alpha_SmartMissile.MissileType temp = missileTypes[i];
                    int randomIndex = Random.Range(i, missileTypes.Count);
                    missileTypes[i] = missileTypes[randomIndex];
                    missileTypes[randomIndex] = temp;
                }

                // バースト発射
                Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

                for (int i = 0; i < missileCount; i++)
                {
                    Vector2 fireDir = Vector2.down;
                    if (player != null)
                    {
                        // プレイヤーの方向に大まかに向けて発射
                        fireDir = (player.position - ai.transform.position).normalized;
                        // 少しブレさせる
                        float angleOffset = Random.Range(-15f, 15f);
                        fireDir = Quaternion.Euler(0, 0, angleOffset) * fireDir;
                    }
                    
                    float angle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg - 90f;
                    GameObject missileObj = Instantiate(smartMissilePrefab, ai.transform.position, Quaternion.Euler(0, 0, angle));
                    Alpha_SmartMissile smartMissile = missileObj.GetComponent<Alpha_SmartMissile>();
                    
                    if (smartMissile != null)
                    {
                        smartMissile.type = missileTypes[i];
                    }

                    yield return new WaitForSeconds(missileBurstInterval);
                }
            }

            // 指定秒数のクールタイム
            yield return new WaitForSeconds(missileCooldown);
        }
    }
}
