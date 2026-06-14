using UnityEngine;

public class Effect_Pierce_Alpha : Alpha_Effect_Base
{
    public Effect_Pierce_Alpha(int pos, int rarity = 1) : base(pos, rarity) 
    {
        // 航行中の処理は最初の一回だけステータス反映すればよいため、基本はDoFlightEffectは使用しない
        flightEffectInterval = 0f;
    }

    public override void Setup(Bullet_Base bullet, playerStatusManager_Alpha playerStatus)
    {
        base.Setup(bullet, playerStatus);

        // --- 薬莢 (1): 航行時効果（ステータス反映として Setup 時に弾の能力を底上げする） ---
        // 弾のスピードとダメージを +(1 * rarity) 上昇させる
        if (equipPosition == 1 || canUseAllEffects)
        {
            float addition = 1f * rarity;
            
            bullet.Speed += addition;
            bullet.dmg += addition; 
            
            // Debug.Log($"[Pierce] 薬莢効果: Speed += {addition}, Dmg += {addition} (Rarity: {rarity})");
        }

        // --- 弾頭 (2): 着弾時効果（ステータス反映として Setup 時にセットする） ---
        // 貫通時ダメージ減衰率を 10% (0.10f) に上書き
        // 貫通回数を +(2 + n) 追加
        if (equipPosition == 2 || canUseAllEffects)
        {
            bullet.localPierceDamageReductionRate = 0.10f; // 10%減衰に上書き
            bullet.piercingCount += (2 + rarity);
            
            // Debug.Log($"[Pierce] 弾頭効果: 貫通回数 += {2 + rarity}, 減衰率を0.10に変更 (Rarity: {rarity})");
        }
    }

    protected override void DoFireEffect(Bullet_Base bullet)
    {
        // --- 雷管 (0): 発射時効果 ---
        // 貫通回数を +1 する (固定値)
        bullet.piercingCount += 1;
        // Debug.Log($"[Pierce] 雷管効果: 貫通回数 += 1 (Rarity: {rarity})");
    }

    protected override void DoFlightEffect(Bullet_Base bullet)
    {
        // 航行中毎フレームやることは特にない
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        // 着弾や貫通のたびにパッシブに行う処理は特にない
        // （ダメージ減衰や貫通回数消費のロジックは Bullet_Base 側で処理されるため）
    }
}
