using UnityEngine;

public class Effect_Explosion_Alpha : Alpha_Effect_Base
{
    private GameObject explosionPrefab;

    public Effect_Explosion_Alpha(int pos, int rarity = 1) : base(pos, rarity) 
    {
        // 航行中の呼び出し間隔を0.5秒に設定（のちにバフ・デバフで短縮・延長します）
        flightEffectInterval = 0.5f;

        // リソース等から爆発プレハブをロード
        explosionPrefab = Resources.Load<GameObject>("Objects/Effect_Explosion");
    }

    // ステータス反映（バフ・デバフによる短縮・延長）
    public override void Setup(Bullet_Base bullet, playerStatusManager_Alpha playerStatus)
    {
        if (playerStatus != null)
        {
            // 例: プレイヤーのバフ（BulletSpanMag）によって間隔を短縮・延長する
            // （BulletSpanMag < 1.0 なら間隔が短くなり、頻繁に爆発するようになるイメージ）
            flightEffectInterval = 0.5f * playerStatus.BulletSpanMag;
        }
    }

    protected override void DoFireEffect(Bullet_Base bullet)
    {
        // 生成時に爆発エフェクトを生成
        SpawnExplosion(bullet.transform.position, bullet.dmg);
    }

    protected override void DoFlightEffect(Bullet_Base bullet)
    {
        // 航行中に0.5秒ごとに爆発エフェクトを生成
        SpawnExplosion(bullet.transform.position, bullet.dmg);
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        // 着弾時に爆発エフェクトを生成
        // targetが敵であるか壁であるかによらず、対象地点で爆発を起こす想定
        SpawnExplosion(bullet.transform.position, bullet.dmg);
    }

    private void SpawnExplosion(Vector3 position, float dmg)
    {
        if (explosionPrefab != null)
        {
            GameObject obj = null;
            if (Alpha_ObjectPoolManager.Instance != null)
            {
                obj = Alpha_ObjectPoolManager.Instance.Rent(explosionPrefab, position, Quaternion.identity);
            }
            else
            {
                obj = GameObject.Instantiate(explosionPrefab, position, Quaternion.identity);
            }
            
            // Effect_Explosionコンポーネントを取得し、消滅までのフレーム数(例: 10)とダメージを渡してキックする
            // (ingameSystem/AttackEffect/Effect_Explosion.cs側のコンポーネント)
            Effect_Explosion effectScript = obj.GetComponent<Effect_Explosion>();
            if (effectScript != null)
            {
                effectScript.sourcePrefab = explosionPrefab; // プール用
                effectScript.startExplosion(dmg, 10);
            }
        }
        else
        {
            Debug.LogWarning("Effect_Explosion のプレハブが見つかりません。Resources/Objects/Effect_Explosion を確認してください。");
        }
    }
}
