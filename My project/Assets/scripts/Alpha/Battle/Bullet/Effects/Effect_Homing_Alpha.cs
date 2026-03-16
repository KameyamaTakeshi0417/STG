using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Homing_Alpha : Alpha_Effect_Base
{
    private Transform currentTarget;
    private float homingStrength = 10f; // 誘導の強さ（旋回速度）
    private float searchRadius = 15f; // 索敵範囲

    public Effect_Homing_Alpha(BASE_WeaponData_Alpha data, int pos) : base(data, pos) 
    {
        // 航行中の処理を毎フレーム（deltaTimeベース）で行うため 0 以下に設定
        flightEffectInterval = 0f;
    }

    private Transform lockedTargetOnFire; // 発射時にロックオンしていたターゲット

    public override void Setup(Bullet_Base bullet, playerStatusManager_Alpha playerStatus)
    {
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

    protected override void DoFlightEffect(Bullet_Base bullet)
    {
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

        // ターゲットがいればそちらへ旋回する
        if (currentTarget != null)
        {
            Vector2 directionToTarget = (Vector2)currentTarget.position - (Vector2)bullet.transform.position;
            directionToTarget.Normalize();

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null && rb.velocity.sqrMagnitude > 0.01f)
            {
                // 現在の進行方向
                Vector2 currentVelocityDir = rb.velocity.normalized;

                // ターゲット方向へ少しずつベクトルを向ける（Lerpで旋回）
                Vector2 newDirection = Vector3.Slerp(currentVelocityDir, directionToTarget, homingStrength * Time.deltaTime).normalized;

                // 弾の実際の速度(magnitude)は維持したまま、向きだけを新しい方向に上書きする
                rb.velocity = newDirection * rb.velocity.magnitude;

                // 弾の見た目の向き (Z軸回転) も合わせる
                float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
                bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
                bullet.rotate = newDirection; // Bullet_Base管理の方向も更新
            }
        }
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        // 【貫通弾とのシナジー用】
        // 当たった場合は一旦ターゲットへの旋回を解除。
        // （次フレームのDoFlightEffectで「発射時のターゲットがまだ生きているか」をチェックし、
        // 生きていれば再度そこへ向かうコンパクトな円軌道を描き始めます）
        currentTarget = null;
    }
}
