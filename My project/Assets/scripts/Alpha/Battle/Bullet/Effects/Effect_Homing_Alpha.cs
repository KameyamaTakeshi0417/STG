using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Homing_Alpha : Alpha_Effect_Base
{
    private Transform currentTarget;
    private float homingStrength = 10f; // 誘導の強さ（旋回速度）
    
    public int homingLevel = 0; // 追尾レベル
    private float trackingDamageTimer = 0f; // 追尾中のダメージ増加タイマー

    public Effect_Homing_Alpha(int pos, int rarity = 1) : base(pos, rarity) 
    {
        // 航行中の処理を毎フレーム（deltaTimeベース）で行うため 0 以下に設定
        flightEffectInterval = 0f;
    }

    private Transform lockedTargetOnFire; // 発射時にロックオンしていたターゲット

    public override void Setup(Bullet_Base bullet, playerStatusManager_Alpha playerStatus)
    {
        base.Setup(bullet, playerStatus);

        // --- 追尾レベルのスタック計算 ---
        // 雷管 (0): 追尾レベル + 1、弾速 - 5%
        if (equipPosition == 0 || canUseAllEffects)
        {
            homingLevel += 1;
            bullet.Speed *= 0.95f; // 5%減速
        }

        // 薬莢 (1): 追尾レベル + (2 + rarity)
        if (equipPosition == 1 || canUseAllEffects)
        {
            homingLevel += (2 + rarity);
        }

        // 弾頭 (2): 追尾レベル + 1
        if (equipPosition == 2 || canUseAllEffects)
        {
            homingLevel += 1;
        }

        // ターゲットを絞り、旋回速度を前回の約1.6倍（回転半径を60%コンパクトに）上げる
        homingStrength = 16f + (stackCount * 3f); 
        
        // 発射した瞬間(セットアップ時)のみロックオン対象を取得し、記憶する
        Alpha.PointerLineSystem pointerSystem = Object.FindAnyObjectByType<Alpha.PointerLineSystem>();
        if (pointerSystem != null && pointerSystem.CurrentTarget != null)
        {
            lockedTargetOnFire = pointerSystem.CurrentTarget;
            currentTarget = lockedTargetOnFire;
        }
    }

    // Alpha_Effect_Baseの「航行中エフェクトは装備スロット1(薬莢)でないと呼ばれない」という
    // 制限を回避するため、OnFlight 本体を override します。
    public override void OnFlight(Bullet_Base bullet, float deltaTime)
    {
        // 装備位置の制限をバイパスして、毎フレーム必ず追尾処理を行う
        DoFlightEffect(bullet, deltaTime);
    }

    // 引数を追加したカスタム用のDoFlightEffect
    private void DoFlightEffect(Bullet_Base bullet, float deltaTime)
    {
        // 親であるサーキュラー(ドローン)自体は追尾しない。サブバレットのみ追尾する。
        if (bullet.GetComponent<CircularObject>() != null) return;

        // 追尾レベルが0以下の場合は何もしない（通常は最低1はあるはず）
        if (homingLevel <= 0) return;

        // 最初からロックオンしていなかった場合は真っ直ぐ飛ぶ
        if (lockedTargetOnFire == null) return;

        // 【貫通時の再誘導用】もし貫通して一度リセット(ターゲット解除)されていても、
        // 「発射時にロックした奴がまだ生きている」なら再ロックする
        if (currentTarget == null && lockedTargetOnFire.gameObject.activeInHierarchy)
        {
            currentTarget = lockedTargetOnFire;
        }

        // ロックした相手が死んで消えたり非アクティブになったら、もう誘導しない（まっすぐ飛ぶ）
        if (currentTarget != null && !currentTarget.gameObject.activeInHierarchy)
        {
            currentTarget = null;
        }

        bool isTracking = false;

        // ターゲットがいればそちらへ旋回する
        if (currentTarget != null)
        {
            Vector2 directionToTarget = (Vector2)currentTarget.position - (Vector2)bullet.transform.position;
            directionToTarget.Normalize();

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 現在の進行方向（velocityが0になる瞬間に備え、論理的な進行方向を利用する）
                Vector2 currentVelocityDir = bullet.rotate.normalized;
                if (currentVelocityDir == Vector2.zero) currentVelocityDir = Vector2.up; // フェイルセーフ

                // 貫通弾（貫通回数を持つ弾）の場合は旋回半径を少し広げる（旋回力を下げる）
                float currentHomingStrength = homingStrength;
                if (bullet.piercingCount > 0)
                {
                    currentHomingStrength *= 0.9f; // 旋回力を90%にする = 旋回半径が広がる
                }

                // ターゲット方向へ少しずつベクトルを向ける（Lerpで旋回）
                Vector2 newDirection = Vector3.Slerp(currentVelocityDir, directionToTarget, currentHomingStrength * deltaTime).normalized;

                // 物理エンジンの影響で減速しないよう、常に発射時の初期スピードで上書きする
                // => 以前のAddForce相当の速度にするため、0.02fを掛ける
                rb.velocity = newDirection * (bullet.Speed * 0.02f);

                // 弾の見た目の向き (Z軸回転) も合わせる
                float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
                bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
                bullet.rotate = newDirection; // Bullet_Base管理の方向も更新
                
                // 旋回中かどうかのフラグを立てる
                isTracking = true;
            }
        }

        // 追尾レベルが2以上で、実際に追尾中(ターゲットへ旋回中)であれば、ダメージ上昇処理を行う
        if (homingLevel >= 2 && isTracking)
        {
            trackingDamageTimer += deltaTime;
            
            // 10/60秒（約0.166秒）ごとにダメージ増加
            float interval = 10f / 60f;
            if (trackingDamageTimer >= interval)
            {
                // ダメージ増加率 = +(1 + (2 * (n - 1)))%
                float increasePercentage = 1f + (2f * (homingLevel - 1f));
                float multiplier = 1.0f + (increasePercentage / 100f);
                
                // 蓄積された回数分ループして（フレーム落ちなどで複数回分またいだ場合）
                while (trackingDamageTimer >= interval)
                {
                    bullet.dmg *= multiplier;
                    trackingDamageTimer -= interval;
                }
            }
        }
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        // 【貫通弾とのシナジー用】
        // 当たった場合は一旦ターゲットへの旋回を解除。
        // （次フレームのDoFlightEffectで「発射時のターゲットがまだ生きているか」をチェックし、
        // 生きていれば再度そこへ向かうコンパクトな円軌道を描き始めます）
        // === 修正 ===
        // 貫通中に何度もHitイベントが発生してターゲットが何度もリセットされることで、
        // その場で高速回転してしまうバグが発生するためコメントアウトします。
        // currentTarget = null;
    }
}
