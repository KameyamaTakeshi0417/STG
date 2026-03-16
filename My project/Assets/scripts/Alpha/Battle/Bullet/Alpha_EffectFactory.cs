using UnityEngine;

public static class Alpha_EffectFactory
{
    // 各装備データから指定された効果クラスのインスタンスを生成する
    public static Alpha_Effect_Base CreateEffect(BASE_WeaponData_Alpha data, int position)
    {
        if (data == null) return null;

        switch (data.effectType)
        {
            case Alpha_EffectType.None:
                return null;
            case Alpha_EffectType.SampleEffect:
                return new Sample_Effect_Alpha(data, position);
            case Alpha_EffectType.Explosion:
                return new Effect_Explosion_Alpha(data, position);
            case Alpha_EffectType.Homing:
                return new Effect_Homing_Alpha(data, position);
            case Alpha_EffectType.Volt:
                return new Effect_Volt_Alpha(data, position);
            // 今後追加する効果（例: 爆発、ダメージアップ等）はここにケースを追加していく
            default:
                return null;
        }
    }
}
