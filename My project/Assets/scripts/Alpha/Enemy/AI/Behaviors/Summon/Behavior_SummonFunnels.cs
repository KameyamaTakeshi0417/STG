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

    [Header("Phase Transition")]
    [Tooltip("すべてのファンネルが破壊された時に切り替える攻撃行動（発狂モード）。未設定なら何もしません。")]
    public EnemyBehaviorData_Base enrageAttackBehavior;
    [Tooltip("発狂モード移行時にボスのスプライト色を変更するか")]
    public bool changeColorOnEnrage = true;
    [Tooltip("発狂時のボスの色")]
    public Color enrageColor = new Color(1f, 0.3f, 0.3f, 1f); // 少し赤くする

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

                // AIの管理下に追加（全滅チェックや死亡時の破棄用）
                ai.PhaseSpawnedObjects.Add(obj);
            }
        }

        // 召喚が終わったら、ファンネルの全滅を監視する
        while (true)
        {
            // Nullまたは非アクティブなもの（破壊されたもの）をリストから除外
            ai.PhaseSpawnedObjects.RemoveAll(x => x == null || !x.activeInHierarchy);

            // ファンネルが全滅した場合
            if (ai.PhaseSpawnedObjects.Count == 0)
            {
                if (enrageAttackBehavior != null)
                {
                    Debug.Log("[Behavior_SummonFunnels] All funnels destroyed! Triggering Enrage Attack Behavior.");
                    // Attackスロットのビヘイビアを発狂モードに置換する
                    ai.StartBehavior(Alpha_EnemyAI.BehaviorSlot.Attack, enrageAttackBehavior);

                    // スプライトを赤くする
                    if (changeColorOnEnrage)
                    {
                        // 子オブジェクトも含めて全てのスプライトレンダラーを取得
                        SpriteRenderer[] renderers = ai.GetComponentsInChildren<SpriteRenderer>();
                        foreach (var sr in renderers)
                        {
                            sr.color = enrageColor;
                        }
                    }
                }
                
                // 監視ループを抜けて、この召喚ビヘイビアを完了する
                break;
            }

            yield return null;
        }
    }
}
