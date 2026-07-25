using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Enemy.Weapons;

[CreateAssetMenu(fileName = "Behavior_FunnelCircleAndOrbit", menuName = "Alpha/Enemy AI/Behaviors/Boss3/Funnel Circle And Orbit")]
public class Behavior_FunnelCircleAndOrbit : EnemyBehaviorData_Base
{
    [Header("Boss Movement")]
    public Vector2 centerPos = new Vector2(0, 3f);
    public float bossMoveSpeed = 5f;
    
    [Header("Funnels Setup")]
    public GameObject funnelPrefab; // Alpha_FunnelControllerがアタッチされたプレハブ
    public float circleRadius = 7f; // 外側の6機の半径
    public float orbitRadius = 3f;  // 内側の2機の軌道半径
    public float orbitSpeed = 45f;  // 内側の回転速度

    [Header("Laser Setup")]
    public float laserLength = 30f;
    public float laserThickness = 1.5f;
    public float laserExpandTime = 0.3f;
    public float laserDamage = 1f;

    [Header("Boss Barrage Setup")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public float fireRate = 0.1f;
    public int bulletCountPerShot = 3;

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
        List<Alpha_FunnelController> circleFunnels = new List<Alpha_FunnelController>();
        List<Alpha_FunnelController> orbitFunnels = new List<Alpha_FunnelController>();

        // 円形配置（6機）
        for (int i = 0; i < 6; i++)
        {
            GameObject obj = Instantiate(funnelPrefab, ai.transform.position, Quaternion.identity);
            Alpha_FunnelController fc = obj.GetComponent<Alpha_FunnelController>();
            
            float angle = (360f / 6f) * i;
            Vector2 pos = centerPos + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * circleRadius;
            fc.SetTargetPosition(pos);
            circleFunnels.Add(fc);
        }

        // 周回配置（2機）
        for (int i = 0; i < 2; i++)
        {
            GameObject obj = Instantiate(funnelPrefab, ai.transform.position, Quaternion.identity);
            Alpha_FunnelController fc = obj.GetComponent<Alpha_FunnelController>();
            orbitFunnels.Add(fc);
        }

        // 3. 各種コルーチンの起動
        ai.StartCoroutine(CircleFunnelsRoutine(circleFunnels));
        ai.StartCoroutine(OrbitFunnelsRoutine(orbitFunnels, ai.transform));
        ai.StartCoroutine(BossBarrageRoutine(ai));

        // フェーズが終了するまで無限ループして待機
        while (true)
        {
            yield return null;
        }
    }

    private IEnumerator CircleFunnelsRoutine(List<Alpha_FunnelController> funnels)
    {
        while (true)
        {
            // クールダウン中（照準合わせ）
            float timer = 0f;
            while (timer < 1f)
            {
                Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
                if (player != null)
                {
                    foreach (var f in funnels)
                    {
                        if (f != null) f.LookAtTarget(player.position);
                    }
                }
                timer += Time.deltaTime;
                yield return null;
            }

            // 発射
            foreach (var f in funnels)
            {
                // ここでは2WAYプレハブをFunnel側にセットしている想定なので、そのまま発射を呼ぶ
                if (f != null) f.FireLasers(laserLength, laserThickness, laserExpandTime, laserDamage);
            }

            // 発射持続時間は0.5秒程度と仮定（ビームの生存時間はFunnelController側やプレハブ側で管理、もしくは一定時間後に消す）
            yield return new WaitForSeconds(0.5f);
            foreach (var f in funnels)
            {
                if (f != null) f.ClearLasers();
            }
        }
    }

    private IEnumerator OrbitFunnelsRoutine(List<Alpha_FunnelController> funnels, Transform center)
    {
        float currentAngle = 0f;
        float fireTimer = 0f;

        while (true)
        {
            currentAngle += orbitSpeed * Time.deltaTime;
            fireTimer += Time.deltaTime;

            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

            for (int i = 0; i < funnels.Count; i++)
            {
                var f = funnels[i];
                if (f == null) continue;

                float offset = (360f / funnels.Count) * i;
                float angleRad = (currentAngle + offset) * Mathf.Deg2Rad;
                Vector2 pos = (Vector2)center.position + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * orbitRadius;
                f.SetTargetPosition(pos);

                // 発射していない間はプレイヤーを向く
                if (!f.HasActiveLasers() && player != null)
                {
                    f.LookAtTarget(player.position);
                }
            }

            // 1.5秒ごとに発射
            if (fireTimer >= 1.5f)
            {
                fireTimer = 0f;
                foreach (var f in funnels)
                {
                    if (f != null) f.FireLasers(laserLength, laserThickness, laserExpandTime, laserDamage);
                }
                
                // 0.5秒後に消す処理を非同期で走らせる
                funnels[0].StartCoroutine(ClearLaserAfterDelay(funnels, 0.5f));
            }

            yield return null;
        }
    }

    private IEnumerator ClearLaserAfterDelay(List<Alpha_FunnelController> funnels, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var f in funnels)
        {
            if (f != null) f.ClearLasers();
        }
    }

    private IEnumerator BossBarrageRoutine(Alpha_EnemyAI ai)
    {
        while (true)
        {
            if (bulletPrefab != null)
            {
                for (int i = 0; i < bulletCountPerShot; i++)
                {
                    float angle = Random.Range(0f, 360f);
                    GameObject bulletObj = Instantiate(bulletPrefab, ai.transform.position, Quaternion.Euler(0, 0, angle));
                    Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.velocity = bulletObj.transform.up * bulletSpeed;
                    }
                }
            }
            yield return new WaitForSeconds(fireRate);
        }
    }
}
