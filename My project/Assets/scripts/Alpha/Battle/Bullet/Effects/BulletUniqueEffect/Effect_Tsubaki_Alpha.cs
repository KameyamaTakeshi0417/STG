using System.Collections;
using UnityEngine;

public class Effect_Tsubaki_Alpha : Alpha_Effect_Base
{
    public Effect_Tsubaki_Alpha(int pos, int rarity = 1) : base(pos, rarity) { }

    protected override void DoFireEffect(Bullet_Base bullet)
    {
        if (isSubBullet && bullet.GetComponent<CircularObject>() == null) return;

        // 雷管装備時(0): 滞留・ターゲットロック・拡大・発射のコントローラーを付与
        if (equipPosition == 0)
        {
            var primerCtrl = bullet.gameObject.AddComponent<TsubakiPrimerController>();
            primerCtrl.Initialize(bullet, rarity);
        }

        // 薬莢装備時(1): 3秒かけて攻撃力・スピードが上昇するコントローラーを付与
        if (equipPosition == 1)
        {
            var casingCtrl = bullet.gameObject.AddComponent<TsubakiCasingController>();
            casingCtrl.Initialize(bullet, rarity);
        }
    }

    protected override void DoFlightEffect(Bullet_Base bullet) { }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        if (!isSubBullet && bullet.GetComponent<CircularObject>() != null) return;

        // 弾頭装備時(2) かつ 全スロット椿シリーズ(Bouquet)の場合のみ処理
        if (equipPosition == 2 && canUseAllEffects)
        {
            if (target != null && target.CompareTag("Enemy"))
            {
                // Effect_Homing_Alphaが持っているターゲットを取得する
                Transform homingTarget = null;
                foreach (var eff in bullet.activeEffects)
                {
                    if (eff is Effect_Homing_Alpha homingEff)
                    {
                        homingTarget = homingEff.currentTarget; // publicプロパティに変更が必要かも？リフレクションは避けたい。
                        break;
                    }
                }

                // ターゲットが存在し、ヒットした敵がターゲットそのものである場合
                if (homingTarget != null)
                {
                    _Health_Base targetHealth = homingTarget.GetComponentInParent<_Health_Base>();
                    _Health_Base hitHealth = target.GetComponentInParent<_Health_Base>();

                    if (targetHealth != null && hitHealth != null && targetHealth == hitHealth)
                    {
                        // (0.5 * 品質)秒のスタン
                        hitHealth.ApplyStun(0.5f * rarity);

                        // 相手のピアスボリューム回数分のダメージを与える
                        int pVol = hitHealth.PierceVolume > 0 ? hitHealth.PierceVolume : 1;
                        // すでにBullet_Baseで1回ヒット判定が行われているため、残りの回数分(pVol - 1)の追加ダメージを与える
                        if (pVol - 1 > 0)
                        {
                            float totalDamage = bullet.dmg * (pVol - 1);
                            hitHealth.ApplyDamage(totalDamage);
                        }
                        
                        // 弾を確実に消滅させるため
                        bullet.piercingCount = -1;
                        bullet.preventAutoDestroy = false;
                    }
                }
            }
        }
    }
}

// ==========================================
// 椿シリーズ 雷管用の滞留・ターゲット・発射コントローラー
// ==========================================
public class TsubakiPrimerController : MonoBehaviour
{
    private Bullet_Base bullet;
    private int rarity;
    public Transform currentTarget { get; private set; }

    private float timer = 0f;
    private const float STAY_TIME = 3.0f;
    private Vector3 initialScale;
    private Collider2D bulletCollider;
    
    public float originalSpeed;
    public bool isWaiting { get; private set; }

    public void Initialize(Bullet_Base b, int r)
    {
        bullet = b;
        rarity = r;
        initialScale = transform.localScale;
        bulletCollider = GetComponent<Collider2D>();

        // 初期のスピードを保存し、滞留中は0にする
        originalSpeed = bullet.Speed;
        bullet.Speed = 0f;
        isWaiting = true;
        if (bullet.GetComponent<Rigidbody2D>() != null)
        {
            bullet.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        // 滞留中は当たり判定を無効化
        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (bullet == null) return;

        timer += Time.deltaTime;

        if (timer < STAY_TIME)
        {
            // サイズを最大1.5倍まで徐々に大きくする
            float progress = timer / STAY_TIME;
            float scaleMag = 1.0f + (0.5f * progress);
            transform.localScale = initialScale * scaleMag;

            // ターゲットを探し、向きを更新する
            UpdateTarget();
            if (currentTarget != null)
            {
                Vector2 dir = (currentTarget.position - transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
                bullet.rotate = dir;
            }
        }
        else if (isWaiting)
        {
            // 3秒経過した瞬間に発射処理
            isWaiting = false;
            if (bulletCollider != null)
            {
                bulletCollider.enabled = true;
            }

            // 直線上に発射
            bullet.Speed = originalSpeed;
            if (bullet.GetComponent<Rigidbody2D>() != null)
            {
                bullet.GetComponent<Rigidbody2D>().velocity = bullet.rotate.normalized * (bullet.Speed * 0.02f);
            }
        }
    }

    private void OnDisable()
    {
        // プール返却時にサイズを元に戻し、コンポーネントを破棄して次の弾に影響が出ないようにする
        if (initialScale != Vector3.zero)
        {
            transform.localScale = initialScale;
        }
        Destroy(this);
    }

    private void UpdateTarget()
    {
        // 既にターゲットがいて有効なら継続
        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            _Health_Base h = currentTarget.GetComponent<_Health_Base>();
            if (h != null && h.getCurrentHP() > 0) return;
        }

        // ロックオン対象が存在し有効ならそれを優先する
        if (bullet != null && bullet.lockedTarget != null && bullet.lockedTarget.gameObject.activeInHierarchy)
        {
            _Health_Base lockedH = bullet.lockedTarget.GetComponent<_Health_Base>();
            if (lockedH != null && lockedH.getCurrentHP() > 0)
            {
                currentTarget = bullet.lockedTarget;
                return;
            }
        }

        // 新しいターゲットを検索（一番近い敵）
        currentTarget = FindClosestEnemy();
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null;
        float closestDistSq = float.MaxValue;

        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            _Health_Base h = enemy.GetComponent<_Health_Base>();
            if (h == null || h.getCurrentHP() <= 0) continue;

            float distSq = (enemy.transform.position - currentPos).sqrMagnitude;
            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                bestTarget = enemy.transform;
            }
        }
        return bestTarget;
    }
}

// ==========================================
// 椿シリーズ 薬莢用のステータス上昇コントローラー
// ==========================================
public class TsubakiCasingController : MonoBehaviour
{
    private Bullet_Base bullet;
    private int rarity;
    private float timer = 0f;
    private const float DURATION = 3.0f;

    private float baseDamage;
    private float baseSpeed;

    public void Initialize(Bullet_Base b, int r)
    {
        bullet = b;
        rarity = r;
        baseDamage = bullet.dmg;
        baseSpeed = bullet.Speed;
        
        // もしPrimerが先にInitializeされていてbullet.Speedが0になっている場合、
        // originalSpeedをbaseSpeedとして取得し直す
        var primerCtrl = GetComponent<TsubakiPrimerController>();
        if (primerCtrl != null && primerCtrl.isWaiting)
        {
            baseSpeed = primerCtrl.originalSpeed;
        }
    }

    private void Update()
    {
        if (bullet == null) return;
        if (timer >= DURATION) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / DURATION);

        // 3秒かけて 基礎値 + (品質 * 基礎値) まで上昇
        float targetBonusDmg = rarity * baseDamage;
        float targetBonusSpd = rarity * baseSpeed;

        bullet.dmg = baseDamage + (targetBonusDmg * progress);
        float newSpeed = baseSpeed + (targetBonusSpd * progress);
        
        var primerCtrl = GetComponent<TsubakiPrimerController>();
        if (primerCtrl != null && primerCtrl.isWaiting)
        {
            // 待機中は発射用の速度だけを更新し、実際の速度(0)は書き換えない
            primerCtrl.originalSpeed = newSpeed;
        }
        else
        {
            // 発射済、またはPrimerが装備されていない場合は普通に加速
            bullet.Speed = newSpeed;
            if (bullet.GetComponent<Rigidbody2D>() != null)
            {
                bullet.GetComponent<Rigidbody2D>().velocity = bullet.rotate.normalized * (bullet.Speed * 0.02f);
            }
        }
    }

    private void OnDisable()
    {
        // プール返却時にコンポーネントを破棄して次の弾に影響が出ないようにする
        Destroy(this);
    }
}
