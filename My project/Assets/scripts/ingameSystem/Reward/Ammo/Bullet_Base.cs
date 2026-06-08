using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Alpha.Core.Utils;

public class Bullet_Base : MonoBehaviour, IAlphaPoolable, IBombDestructible
{
    // 追加: 自刁E��生まれた允E�Eプレハブの参�Eを保持する
    public GameObject sourcePrefab;

    [Header("Alignment Settings")]
    [Tooltip("Tooltip")]
    public bool isEnemyBullet = false;
    [Tooltip("Tooltip")]
    public bool canHitBoth = false;
 
    public Vector3 originalAimDirection; // Reverseパターンで允E�E方向に戻るため記�E用
    public float reverseTimeRemaining = 0f; // Reverseパターンの後退残り時間
    public Transform lockedTarget; // ロチE��オン対象を保持

    public virtual void OnRentFromPool()
    {
        // 再登場時にリセチE��処琁E
        piercingCount = 0; // 継承先！EiercingBulletなど�E�で忁E��に応じて override して再設定する�Eースとして0クリア
        extraShotCount = 0; // 追加発封E��のリセチE��
        hitCountsPerEnemy.Clear();
        ignoredColliders.Clear();
        reverseTimeRemaining = 0f;
        lockedTarget = null;
        
        if (bulletCollider != null) bulletCollider.enabled = true;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        activeEffects.Clear();
    }

    public virtual void OnReturnToPool()
    {
        // 非表示になる直前�E処琁E��忁E��なら�E�E�E
        StopAllCoroutines();
    }
    public string Objname;
    protected Rigidbody2D rb;
    public float dmg; // 弾のダメージ釁E
    public float Speed; //弾の出る速度
    public float DestroyTime; //弾の存在する時間
    public float bullettype = 0; //弾のタイプ決宁E
    public Vector3 rotate; //弾の発封E��E

    public int rarelity; //オブジェクト�E挙動が変わるもの
    public string bulletName;
    public float addDmg; //ダメージ倍率のかからなぁE��定ダメージ
    public int piercingCount = 0;
    public int extraShotCount = 0; // サーキュラー等�EサブバレチE��増加用

    // 貫通�E琁E��スチE�EチE
    protected float initialDmg; // 減衰計算�Eースの初期ダメージ
    protected Dictionary<GameObject, int> hitCountsPerEnemy = new Dictionary<GameObject, int>(); // 敵ごとのヒット回数
    protected Collider2D bulletCollider; // 再判定用にコライダーを一時的に制御

    // Bomb destruction flag
    public bool canDestructByBomb { get; set; } = true;

    // Start is called before the first frame update
    void Start() { }

    public string getBulletName()
    {
        return bulletName;
    }

    protected virtual void Update() 
    { 
        if (reverseTimeRemaining > 0)
        {
            reverseTimeRemaining -= Time.deltaTime;
            if (reverseTimeRemaining <= 0)
            {
                // リバ�Eス時間が終亁E��た瞬間、本来の進行方向！EriginalAimDirection�E�へ向き直めE
                if (rb != null)
                {
                    rb.velocity = originalAimDirection.normalized * (Speed * 0.02f);
                }
                
                // 向きも修正する
                float rotationAngle = Mathf.Atan2(originalAimDirection.y, originalAimDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));
                
                // 以降、�Eーミング等�Eエフェクトがあればそちらが自発皁E��効き始めめE
            }
        }
    }

    public void setDmg(float damage)
    {
        dmg = damage;
    }

    //弾の撁E��角度の正規化
    public void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(
            0,
            0,
            MathF.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90
        );
        rotate = rot.normalized;
    }

    //弾の速度決宁E
    public void setBulletSpeed(float mag) { }

    //弾の特性決宁E

    //弾丸の貫通回数設宁E


    [Tooltip("Tooltip")]
    public bool preventAutoDestroy = false;

    // 貫通弾用のローカルダメージ減衰玁E��E1の場合�Eグローバル設定を使用�E�E
    public float localPierceDamageReductionRate = -1f;

    public bool canUseAllEffects = false;
    public List<Alpha_Effect_Base> activeEffects = new List<Alpha_Effect_Base>();

    public void setStatus(Vector3 Prot, float pSpeed, float pDmg)
    {
        rotate = Prot;
        Speed = pSpeed;
        dmg = pDmg;
    }

    // 武器の効果データを弾に割り当てめE
    public void SetWeaponEffects(List<Alpha_Effect_Base> effects, bool allEffects)
    {
        canUseAllEffects = allEffects;
        activeEffects.Clear();

        if (effects == null) return;

        foreach (var newEffect in effects)
        {
            if (newEffect == null) continue;
            
            var clonedEffect = newEffect.Clone();
            clonedEffect.canUseAllEffects = canUseAllEffects; // 全効果発動可能フラグを渡ぁE

            var existingEffect = activeEffects.Find(e => e.GetType() == clonedEffect.GetType() && e.equipPosition == clonedEffect.equipPosition);
            if (existingEffect != null)
            {
                existingEffect.stackCount++;
            }
            else
            {
                activeEffects.Add(clonedEffect);
            }
        }
    }

    protected float initialSpeed; // 発封E��の威力を基準スピ�Eドとして記�E�E�追加部刁E��E

    public void shoot()
    {
        initialDmg = dmg; // 発封E��の威力を基準ダメージとして記�E
        initialSpeed = Speed; // 発封E��のスピ�Eドを基準として記�E
        bulletCollider = GetComponent<Collider2D>(); // コライダー取征E

        // プレイヤーのスチE�Eタスを取得（弾に個別インターバルなどを反映するため�E�E
        playerStatusManager_Alpha pStatus = null;
        GameObject manager = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
        if (manager != null)
        {
            pStatus = manager.GetComponent<playerStatusManager_Alpha>();
        }

        // 弾を撃ち出す前に初期化�E琁E-> 生�E時効果を発勁E
        foreach (var effect in activeEffects)
        {
            effect.Setup(this, pStatus);
            effect.OnFire(this);
        }

        StartCoroutine(move());
    }

    public void fire()
    {
        gameObject.GetComponent<Case_Base>().setStatus(rotate, Speed, dmg);
        gameObject.GetComponent<Case_Base>().ApplyCaseEffect(this.gameObject);
        
    }

    //弾を撃ち出ぁE
    protected virtual IEnumerator move()
    {
        int count = 0;

        // 弾の発封E
        rb = gameObject.GetComponent<Rigidbody2D>();

        // 物琁E��ンジンの慣性めE��突による減速を防ぐため、�E速を強制する
        if (rb != null)
        {
            rb.velocity = rotate.normalized * (Speed * 0.02f);
        }

        while (count <= DestroyTime || preventAutoDestroy)
        {
            // 弾の位置を更新する�E�保護されてぁE��間�E寿命カウントを進めなぁE��E
            if (!preventAutoDestroy)
            {
                count++;
            }

            // 毎フレーム、現在の進行方吁Erotate)とスピ�EチESpeed)で速度を上書きし続けめE
            // �E��Eレイヤー移動系の改修と同様、物琁E��算による意図しなぁE��速を完�Eに防ぐ！E
            if (rb != null)
            {
                rb.velocity = rotate.normalized * (Speed * 0.02f);
            }

            // 画面外（バウンダリ�E�チェチE��
            if (Alpha.Core.ScreenBoundaryManager_Alpha.Instance != null)
            {
                if (Alpha.Core.ScreenBoundaryManager_Alpha.Instance.IsOutOfBounds(transform.position))
                {
                    break; // ループを抜けて消滁E�E琁E��
                }
            }

            // 航行時効果を発勁E(ループ�E体�E0.01秒周朁E
            foreach (var effect in activeEffects)
            {
                effect.OnFlight(this, 0.01f);
            }

            yield return new WaitForSeconds(0.01f);
        }

        // 寿命で消滁E��も着弾扱ぁE��対象はnull�E�にする
        foreach (var effect in activeEffects)
        {
            effect.OnHit(this, null);
        }

        if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
        {
            Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
        }
        else
        {
           DestroyAction();
        }
        yield break;
    }

    public void callHitEffect()
    {
        if (!gameObject.activeInHierarchy) return;

        DrainEffect targetScript;
        targetScript = GetComponent<DrainEffect>();
        if (targetScript != null)
        {
            targetScript.MakeEffect();
        }
        StartCoroutine(hitEffect());
    }

    protected IEnumerator hitEffect()
    {
        yield return null;
    }

    protected void DestroyCheck()
    {
        piercingCount--;

        if (piercingCount <= 0)
        {
            if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
            {
                Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
            }
            else
            {
               DestroyAction();
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // 貫通中�E�EgnoreCollision状態）�Eコライダーからのイベント�E無視すめE
        if (ignoredColliders.Contains(collision)) return;

        bool hitSomething = false;

        // 衝突したオブジェクト�EタグをチェチE��
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            // 敵味方の判宁E
            if (!canHitBoth)
            {
                if (isEnemyBullet && collision.CompareTag("Enemy")) return;
                if (!isEnemyBullet && collision.CompareTag("Player")) return;
            }

            // HPを持つコンポ�Eネントを取得（親オブジェクトに付いてぁE��場合も老E�Eして GetComponentInParent を使用�E�E
            _Health_Base health = collision.GetComponentInParent<_Health_Base>();
            if (health != null)
            {
                GameObject targetObj = health.gameObject; // ダメージを受けた本体をターゲチE��として記録
                if (!hitCountsPerEnemy.ContainsKey(targetObj))
                {
                    hitCountsPerEnemy[targetObj] = 0;
                }

                int prevHitCount = hitCountsPerEnemy[targetObj];
                // PierceVolumeぁE以下（未設定など�E��E場合�E最佁E回として扱ぁE
                int pVol = health.PierceVolume > 0 ? health.PierceVolume : 1;

                // 既にこ�E敵の最大ヒット数に達してぁE��場合�E何もしなぁE
                if (prevHitCount >= pVol) return;

                // 今回の衝突で与えるべきヒチE��回数�E�弾の残り貫通回数+1 と、敵の残り許容ヒット数の少なぁE���E�E
                // ※ piercingCount が残ってぁE��回数 = あと「通り抜けられる」回数
                //   つまりヒチE��できる回数は piercingCount + 1 囁E
                int allowableHits = pVol - prevHitCount;
                int actualHits = Mathf.Min(piercingCount + 1, allowableHits);

                // 減衰玁E��取征E
                float reductionRate = 0.25f;
                
                // まず�E弾自身�E�裁E��効果等）�E減衰玁E��定があればそれを優先すめE
                if (localPierceDamageReductionRate >= 0f)
                {
                    reductionRate = localPierceDamageReductionRate;
                }
                else
                {
                    // なければプレイヤースチE�Eタス�E��Eネ�Eジャー�E��E設定値を取征E
                    GameObject manager = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
                    if (manager != null)
                    {
                        var pStatus = manager.GetComponent<playerStatusManager_Alpha>();
                        if (pStatus != null) reductionRate = pStatus.pierceDamageReductionRate;
                    }
                }

                // actualHitsの回数刁E��ーチE
                for (int i = 0; i < actualHits; i++)
                {
                    // HPを減らす、E回目は今�Edmg、E回目以降�Eさっき減衰されたdmgを使ぁE��E
                    health.ApplyDamage(dmg, this);
                    hitCountsPerEnemy[targetObj]++;

                    // 貫通枠を消費する。（最後�E1ヒット＝もぁE��通できなぁE��は消費しなぁE��もしくは0未満になる！E
                    piercingCount--;

                    // 次のヒット（同じ敵の連続ヒチE��、もしくは次の敵へのヒット）�Eために威力を減衰させめE
                    dmg -= initialDmg * reductionRate;
                    if (dmg <= initialDmg * 0.1f) dmg = initialDmg * 0.1f;
                }
            }
            hitSomething = true;
        }
        else if (collision.CompareTag("wall"))
        {
            hitSomething = true;
        }

        if (hitSomething)
        {
            // 着弾効果を発勁E
            foreach (var effect in activeEffects)
            {
                effect.OnHit(this, collision);
            }

            // 残りの貫通回数ぁE未満�E�＝もぁE��通枠がなぁE��もしくは壁に当たった場合�E消滁E
            if (piercingCount < 0 || collision.CompareTag("wall"))
            {
                // ドローンのようなCircularObjectかつpreventAutoDestroyの場合�Eみ消滁E��免れめE
                if (preventAutoDestroy && GetComponent<CircularObject>() != null)
                {
                    // 保護されてぁE��場合�E消滁E��ず、すり抜けさせる
                    StartCoroutine(TemporaryDisableCollider(collision));
                }
                else
                {
                    if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
                    {
                        Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
                    }
                    else
                    {
                       DestroyAction();
                    }
                }
            }
            // 貫通枠が残ってぁE��場合�E1フレーム無効化してすり抜けめE
            else
            {
                StartCoroutine(TemporaryDisableCollider(collision));
            }
        }
    }

    // 貫通した対象との物琁E��な衝突判定を無視するリスチE
    private HashSet<Collider2D> ignoredColliders = new HashSet<Collider2D>();

    protected IEnumerator TemporaryDisableCollider(Collider2D targetCollider)
    {
        if (bulletCollider != null && targetCollider != null)
        {
            // 同じ敵に何度も当たらなぁE��ぁE��、かつ物琁E��に引っかかって減速しなぁE��ぁE��コリジョンを無視すめE
            Physics2D.IgnoreCollision(bulletCollider, targetCollider, true);
            ignoredColliders.Add(targetCollider);
            
            // 貫通後に再�E同じ敵に当たることを許可するかどぁE��はゲームの仕様によるが、E
            // 今回は「通り抜ける間」だけ無視し、一定時間（例えば0.5秒）後に無視を解除する
            yield return new WaitForSeconds(0.5f);
            
            if (this != null && this.gameObject != null && bulletCollider != null && targetCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, targetCollider, false);
                ignoredColliders.Remove(targetCollider);
            }
        }
    }

    protected float damageCaluculator(float pow, float mag)
    {
        float ret = 0f;
        ret = addDmg + (pow + dmg) * mag;

        return ret;
    }

    public float getDmg()
    {
        return dmg;
    }

    public float getSpeed()
    {
        return Speed;
    }

    public int getRarelity()
    {
        return rarelity;
    }
    public virtual void DestroyAction() {
        
        Destroy(this.gameObject);
    }
    public void GenerateAnotherChildBullet() { }

    public void OnBombDestruct()
    {
        if (canDestructByBomb)
        {
            if (isEnemyBullet)
            {
                int currentMNE = PlayerPrefs.GetInt("MoneyAndExp", 0);
                PlayerPrefs.SetInt("MoneyAndExp", currentMNE + 1);
            }
            if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
            {
                Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
            }
            else
            {
                DestroyAction();
            }
        }
    }
}

