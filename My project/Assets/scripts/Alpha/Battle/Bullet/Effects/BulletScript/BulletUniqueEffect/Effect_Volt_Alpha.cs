using System.Collections.Generic;
using UnityEngine;

public class Effect_Volt_Alpha : Alpha_Effect_Base
{
    private GameObject voltAreaPrefab;

    public Effect_Volt_Alpha(int pos, int rarity = 1) : base(pos, rarity) 
    {
        // 航行中の展開間隔。ユーザーの設定値をそのまま使用する
        flightEffectInterval = 0.2f; // デフォルト値。必要に応じて変更してください

        // リソース等から帯電領域プレハブをロード
        voltAreaPrefab = Resources.Load<GameObject>("Objects/Effect_VoltArea_Alpha") 
                      ?? Resources.Load<GameObject>("Objects/Effect_Volt");
    }

    protected virtual float CalculateVoltDamage(Bullet_Base bullet)
    {
        float ratio = 0.30f * rarity;
        
        // ベスト部位が一致していればさらに+0.03f
        if (sourceSeries != null)
        {
            if (equipPosition == 0 && sourceSeries.bestSlot == Alpha.Data.WeaponPartType_Alpha.Primer) ratio += 0.03f;
            else if (equipPosition == 1 && sourceSeries.bestSlot == Alpha.Data.WeaponPartType_Alpha.Casing) ratio += 0.03f;
            else if (equipPosition == 2 && sourceSeries.bestSlot == Alpha.Data.WeaponPartType_Alpha.Bullet) ratio += 0.03f;
        }
        
        return bullet.dmg * ratio * bullet.secondaryDamageMultiplier;
    }

    protected override void DoFireEffect(Bullet_Base bullet)
    {
        // 弾頭がサーキュラーであり、自分がサブバレットである場合は雷管エフェクト(生成時)をスキップ
        // （親であるサーキュラー生成時のみプレイヤー中心に展開する）
        // ※念のため、確実に親ドローンには発動させるため CircularObject の判定を入れる
        if (isSubBullet && bullet.GetComponent<CircularObject>() == null) return;

        Debug.Log($"[Effect_Volt_Alpha] DoFireEffect Triggered! equipPosition: {equipPosition}, canUseAllEffects: {canUseAllEffects}, bullet: {bullet.name}");
        // 雷管装備時(0): 弾を発射した瞬間、プレイヤーの位置に生成
        Vector3 spawnPos = bullet.transform.position;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            spawnPos = player.transform.position;
        }
        else if (playerStatusManager_Alpha.Instance != null && playerStatusManager_Alpha.Instance.transform.parent != null)
        {
            // Playerタグが見つからない場合のフォールバック
            spawnPos = playerStatusManager_Alpha.Instance.transform.position;
        }
        
        SpawnVoltArea(spawnPos, CalculateVoltDamage(bullet), rarity, 1.4f, bullet);
    }

    protected override void DoFlightEffect(Bullet_Base bullet)
    {
        // 弾頭がサーキュラーであり、自分がサブバレットである場合は薬莢エフェクト(航行時)をスキップ
        // （親であるサーキュラー航行中のみ帯電を展開する）
        // ※確実に親ドローンには発動させるため CircularObject の判定を入れる
        if (isSubBullet && bullet.GetComponent<CircularObject>() == null) return;

        // 薬莢装備時(1): 航行中にポロポロと弾の位置に帯電領域を展開
        SpawnVoltArea(bullet.transform.position, CalculateVoltDamage(bullet), rarity, 1.0f, bullet);
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        // 弾頭がサーキュラーであり、自分が親（サーキュラー本体）である場合は弾頭エフェクト(着弾時)をスキップ
        // （サーキュラー本体が敵に当たった時は発動せず、そこから発射されたサブバレット着弾時のみ発動する）
        if (!isSubBullet && bullet.GetComponent<CircularObject>() != null) return;

        // 弾頭装備時(2): 着弾時に帯電領域を展開
        Vector3 spawnPos = bullet.transform.position;
        if (target != null && (target.CompareTag("Enemy") || target.CompareTag("Player")))
        {
            // 敵などの場合はその位置を中心に展開
            spawnPos = target.transform.position;
        }
        SpawnVoltArea(spawnPos, CalculateVoltDamage(bullet), rarity, 1.0f, bullet);
    }

    private void SpawnVoltArea(Vector3 position, float dmg, int rarity, float scaleMultiplier = 1.0f, Bullet_Base bullet = null)
    {
        // xは弾のレアリティに等しい(chainVolt)
        int voltLevelX = rarity;
        
        // 最低は1
        if (voltLevelX <= 0) voltLevelX = 1;

        if (voltAreaPrefab != null)
        {
            GameObject obj = null;
            if (Alpha_ObjectPoolManager.Instance != null)
            {
                obj = Alpha_ObjectPoolManager.Instance.Rent(voltAreaPrefab, position, Quaternion.identity);
            }
            else
            {
                obj = GameObject.Instantiate(voltAreaPrefab, position, Quaternion.identity);
            }
            
            // スケールの適用（プールからの再利用時にもリセット・適用されるように必ず設定）
            if (obj != null)
            {
                obj.transform.localScale = voltAreaPrefab.transform.localScale * scaleMultiplier;
            }

            // 帯電領域側にダメージとVoltレベルを渡す
            Alpha_VoltArea areaScript = obj.GetComponent<Alpha_VoltArea>();
            if (areaScript != null)
            {
                areaScript.sourcePrefab = voltAreaPrefab; // プール用
                int trc = bullet != null ? bullet.voltTickReduceCount : 0;
                areaScript.ActivateVoltArea(dmg, voltLevelX, trc);
            }
        }
        else
        {
            Debug.LogWarning("Alpha_VoltArea のプレハブが見つかりません。Resources/Objects/Effect_VoltArea_Alpha を作成・確認してください。");
        }
    }
}
