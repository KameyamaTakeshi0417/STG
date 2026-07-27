using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Enemy.Weapons;

[System.Serializable]
public class FunnelGroupConfig
{
    [Header("Basic")]
    public GameObject funnelPrefab;
    public int count = 6;
    
    [Header("Orbit")]
    public float orbitRadius = 7f;
    public float orbitSpeed = 0f; // 0なら固定
    public float initialAngleOffset = 0f; // グループ全体の角度ズレ
    public Alpha_FunnelController.AimMode aimMode = Alpha_FunnelController.AimMode.Player;

    [Header("Laser Setup")]
    public int wayCount = 2;
    public float spreadAngle = 15f;
    public float laserLength = 30f;
    public float laserThickness = 1.5f;
    public float laserExpandTime = 0.3f;
    public float laserDamage = 1f;

    [Header("Fire Timing")]
    public float fireInterval = 1f;
    public float fireDuration = 0.5f;
}

[CreateAssetMenu(fileName = "Behavior_SummonFunnels", menuName = "Alpha/Enemy AI/Behaviors/Summon/Summon Funnels")]
public class Behavior_SummonFunnels : EnemyBehaviorData_Base
{
    public List<FunnelGroupConfig> funnelGroups;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        if (funnelGroups == null || funnelGroups.Count == 0) yield break;

        foreach (var group in funnelGroups)
        {
            if (group.funnelPrefab == null) continue;

            for (int i = 0; i < group.count; i++)
            {
                GameObject obj = Instantiate(group.funnelPrefab, ai.transform.position, Quaternion.identity);
                Alpha_FunnelController fc = obj.GetComponent<Alpha_FunnelController>();
                
                if (fc != null)
                {
                    fc.centerTarget = ai.transform;
                    fc.orbitRadius = group.orbitRadius;
                    fc.orbitSpeed = group.orbitSpeed;
                    
                    // 等間隔に配置
                    float offset = (360f / group.count) * i;
                    fc.currentAngle = offset + group.initialAngleOffset;
                    fc.aimMode = group.aimMode;

                    fc.wayCount = group.wayCount;
                    fc.spreadAngle = group.spreadAngle;
                    fc.laserLength = group.laserLength;
                    fc.laserThickness = group.laserThickness;
                    fc.laserExpandTime = group.laserExpandTime;
                    fc.laserDamage = group.laserDamage;

                    fc.fireInterval = group.fireInterval;
                    fc.fireDuration = group.fireDuration;
                }

                // AIの管理下に追加（ボス死亡時に一緒に消えるようにする等）
                ai.PhaseSpawnedObjects.Add(obj);
            }
        }

        // 召喚が終わったら、この行動としては「完了（または維持）」
        // 召喚ビヘイビアが抜け落ちないよう無限待機
        while (true)
        {
            yield return null;
        }
    }
}
