using UnityEngine;

public abstract class Alpha_Effect_Base
{
    public BASE_WeaponData_Alpha sourceData; // 元の武器・パーツデータ
    public int stackCount = 1; // 重複回数
    public int equipPosition; // 装備スロット位置（0:生成, 1:航行, 2:着弾）
    public bool canUseAllEffects = false; // 全効果発動可能フラグ
    public int rarity = 1; // レアリティ
    
    // 航行エフェクトの発動間隔（秒）。0以下なら毎フレーム（OnFlightの従来の挙動）
    protected float flightEffectInterval = 0f;
    private float flightEffectTimer = 0f;

    public Alpha_Effect_Base(BASE_WeaponData_Alpha data, int position, int rarity = 1)
    {
        this.sourceData = data;
        this.equipPosition = position;
        this.rarity = rarity;
    }

    // 弾にアタッチされた直後に初期化用として呼ばれる（ステータス反映などに使用）
    public virtual void Setup(Bullet_Base bullet, playerStatusManager_Alpha playerStatus) { }

    // 戦闘開始時にインベントリから呼ばれるパッシブ効果など
    public virtual void StartEffect(int rarity) { }

    // 弾が生成・発射される際に呼ばれる
    public void OnFire(Bullet_Base bullet)
    {
        // 装備か全効果フラグが許可していれば実行
        if (equipPosition == 0 || canUseAllEffects)
        {
            DoFireEffect(bullet);
        }
    }

    // サブクラスは生成時の効果をこちらに記述する
    protected virtual void DoFireEffect(Bullet_Base bullet) { }

    // 弾が航行中に毎フレーム呼ばれる。
    // インターバルを独自のタイミングで処理するため、インターバル経過時のみ実際のエフェクト処理 (DoFlightEffect) を呼ぶ
    public virtual void OnFlight(Bullet_Base bullet, float deltaTime)
    {
        // 装備か全効果フラグが許可していなければ実行しない
        if (equipPosition != 1 && !canUseAllEffects) return;

        if (flightEffectInterval <= 0f)
        {
            DoFlightEffect(bullet);
            return;
        }

        flightEffectTimer += deltaTime;
        if (flightEffectTimer >= flightEffectInterval)
        {
            DoFlightEffect(bullet);
            // 余剰時間を残すか0にリセットするか（ここでは0リセット）
            flightEffectTimer = 0f; 
        }
    }

    // サブクラスは航行中の効果をこちらに記述する
    protected virtual void DoFlightEffect(Bullet_Base bullet) { }

    // 着弾または寿命で消滅する際に呼ばれる
    public void OnHit(Bullet_Base bullet, Collider2D target)
    {
        // 装備か全効果フラグが許可していれば実行
        if (equipPosition == 2 || canUseAllEffects)
        {
            DoHitEffect(bullet, target);
        }
    }

    // サブクラスは着弾時の効果をこちらに記述する
    protected virtual void DoHitEffect(Bullet_Base bullet, Collider2D target) { }
}

// 効果のサンプル例（デバッグ等用）
public class Sample_Effect_Alpha : Alpha_Effect_Base
{
    public Sample_Effect_Alpha(BASE_WeaponData_Alpha data, int pos, int rarity = 1) : base(data, pos, rarity) { }

    protected override void DoFireEffect(Bullet_Base bullet)
    {
        Debug.Log($"[{sourceData.name}] DoFireEffect: 弾が発射されました (Stack: {stackCount}) - 装備位置: {equipPosition}");
    }

    protected override void DoFlightEffect(Bullet_Base bullet)
    {
        // 毎フレーム呼ばれるため大量にログが出ます。必要に応じてコメントアウト等
        // Debug.Log($"[{sourceData.name}] DoFlightEffect: 航行中...");
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        string targetName = target != null ? target.name : "寿命・壁";
        Debug.Log($"[{sourceData.name}] DoHitEffect: 着弾しました - 対象: {targetName} (Stack: {stackCount})");
    }
}
