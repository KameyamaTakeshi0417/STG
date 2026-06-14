using System.Collections.Generic;
using UnityEngine;

public class Effect_VoltPassive_Alpha : Effect_Volt_Alpha
{
    public Effect_VoltPassive_Alpha(int pos, int rarity, float interval) : base(pos, rarity)
    {
        // パッシブ効果専用のインターバル（WeaponEffectSO_AlphaのqualityValuesから渡される）
        // ※ユーザー設定値は「フレーム数（Bullet_Baseの0.01秒ループ換算）」であるため、秒数に変換する
        this.flightEffectInterval = interval * 0.01f;
    }

    protected override float CalculateVoltDamage(Bullet_Base bullet)
    {
        // パッシブとしての雷は基礎倍率を0.2fとする
        float ratio = 0.20f * rarity;
        
        // ベスト部位のボーナスはそのまま乗せる
        if (sourceSeries != null)
        {
            if (equipPosition == 0 && sourceSeries.bestSlot == Alpha.Data.WeaponPartType_Alpha.Primer) ratio += 0.03f;
            else if (equipPosition == 1 && sourceSeries.bestSlot == Alpha.Data.WeaponPartType_Alpha.Casing) ratio += 0.03f;
            else if (equipPosition == 2 && sourceSeries.bestSlot == Alpha.Data.WeaponPartType_Alpha.Bullet) ratio += 0.03f;
        }
        
        return bullet.dmg * ratio;
    }
}
