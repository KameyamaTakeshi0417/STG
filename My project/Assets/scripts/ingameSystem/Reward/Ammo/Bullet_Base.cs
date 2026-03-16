using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet_Base : MonoBehaviour, IAlphaPoolable
{
    // 追加: 自分が生まれた元のプレハブの参照を保持する
    public GameObject sourcePrefab;

    public void OnRentFromPool()
    {
        // 再登場時のリセット処理
        piercingCount = 0; // Prefab側（PiercingBulletなど）で上書きされる前提だが念のため0クリア
        hitCountsPerEnemy.Clear();
        if (bulletCollider != null) bulletCollider.enabled = true;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        activeEffects.Clear();
    }

    public void OnReturnToPool()
    {
        // 非表示になる直前の処理（必要ならば）
        StopAllCoroutines();
    }
    public string Objname;
    protected Rigidbody2D rb;
    public float dmg; // 弾のダメージ量
    public float Speed; //弾の出る速度
    public float DestroyTime; //弾の存在する時間
    public float bullettype = 0; //弾のタイプ決定
    public Vector3 rotate; //弾の発射角

    public int rarelity; //オブジェクトの挙動が変わるもの
    public string bulletName;
    public float addDmg; //ダメージ倍率のかからない固定ダメージ
    public int piercingCount = 0;

    // 貫通処理用ステート
    protected float initialDmg; // 減衰計算ベースの初期ダメージ
    protected Dictionary<GameObject, int> hitCountsPerEnemy = new Dictionary<GameObject, int>(); // 敵ごとのヒット回数
    protected Collider2D bulletCollider; // 再判定用にコライダーを一時的に制御

    // Start is called before the first frame update
    void Start() { }

    public string getBulletName()
    {
        return bulletName;
    }

    // Update is called once per frame
    void Update() { }

    public void setDmg(float damage)
    {
        dmg = damage;
    }

    //弾の撃つ角度の正規化
    public void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(
            0,
            0,
            MathF.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90
        );
        rotate = rot.normalized;
    }

    //弾の速度決定
    public void setBulletSpeed(float mag) { }

    //弾の特性決定

    //弾丸の貫通回数設定


    public bool canUseAllEffects = false;
    public List<Alpha_Effect_Base> activeEffects = new List<Alpha_Effect_Base>();

    public void setStatus(Vector3 Prot, float pSpeed, float pDmg)
    {
        rotate = Prot;
        Speed = pSpeed;
        dmg = pDmg;
    }

    // 武器の効果データを弾に割り当てる（ファクトリを通じてC#クラスのインスタンス化）
    public void SetWeaponEffects(BASE_WeaponData_Alpha w1, BASE_WeaponData_Alpha w2, BASE_WeaponData_Alpha w3, bool allEffects)
    {
        canUseAllEffects = allEffects;
        activeEffects.Clear();

        AddEffectFromWeapon(w1, 0); // 0: 生成
        AddEffectFromWeapon(w2, 1); // 1: 航行
        AddEffectFromWeapon(w3, 2); // 2: 着弾
    }

    private void AddEffectFromWeapon(BASE_WeaponData_Alpha weaponData, int position)
    {
        if (weaponData == null) return;

        // ファクトリから効果クラスのインスタンスを生成
        Alpha_Effect_Base newEffect = Alpha_EffectFactory.CreateEffect(weaponData, position);
        if (newEffect != null)
        {
            newEffect.canUseAllEffects = canUseAllEffects; // 全効果発動可能フラグを効果インスタンスにも渡す

            // 同じ効果がすでにある場合はスタック数を増やす（重複対応）
            var existingEffect = activeEffects.Find(e => e.GetType() == newEffect.GetType());
            if (existingEffect != null)
            {
                existingEffect.stackCount++;
            }
            else
            {
                activeEffects.Add(newEffect);
            }
        }
    }

    protected float initialSpeed; // 発射時の威力を基準スピードとして記憶（追加部分）

    public void shoot()
    {
        initialDmg = dmg; // 発射時の威力を基準ダメージとして記憶
        initialSpeed = Speed; // 発射時のスピードを基準として記憶
        bulletCollider = GetComponent<Collider2D>(); // コライダー取得

        // プレイヤーのステータスを取得（弾に個別インターバルなどを反映するため）
        playerStatusManager_Alpha pStatus = null;
        GameObject manager = GameObject.Find("manager");
        if (manager != null)
        {
            pStatus = manager.GetComponent<playerStatusManager_Alpha>();
        }

        // 弾を撃ち出す前に初期化処理 -> 生成時効果を発動
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

    //弾を撃ち出す
    protected virtual IEnumerator move()
    {
        int count = 0;

        //弾の発射
        rb = gameObject.GetComponent<Rigidbody2D>();
        Vector2 force = new Vector2(rotate.x, rotate.y) * Speed;
        rb.AddForce(force);

        while (count <= DestroyTime)
        {
            // 弾の位置を更新する
            count++;

            // 航行時効果を発動 (ループ自体は0.01秒周期)
            foreach (var effect in activeEffects)
            {
                effect.OnFlight(this, 0.01f);
            }

            yield return new WaitForSeconds(0.01f);
        }

        // 寿命で消滅時も着弾扱い（対象はnull）にする
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
            Destroy(this.gameObject);
        }
        yield break;
    }

    public void callHitEffect()
    {
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
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        bool hitSomething = false;

        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            // HPを持つコンポーネントを取得
            _Health_Base health = collision.GetComponent<_Health_Base>();
            if (health != null)
            {
                GameObject targetObj = collision.gameObject;
                if (!hitCountsPerEnemy.ContainsKey(targetObj))
                {
                    hitCountsPerEnemy[targetObj] = 0;
                }

                int prevHitCount = hitCountsPerEnemy[targetObj];
                // 既にこの敵の最大ヒット数に達している場合は何もしない
                if (prevHitCount >= health.PierceVolume) return;

                // 今回の衝突で与えるべきヒット回数（弾の残り貫通回数+1 と、敵の残り許容ヒット数の少ない方）
                // ※ piercingCount が残っている回数 = あと「通り抜けられる」回数
                //   つまりヒットできる回数は piercingCount + 1 回
                int allowableHits = health.PierceVolume - prevHitCount;
                int actualHits = Mathf.Min(piercingCount + 1, allowableHits);

                // 減衰率を取得
                float reductionRate = 0.25f;
                GameObject manager = GameObject.Find("manager");
                if (manager != null)
                {
                    var pStatus = manager.GetComponent<playerStatusManager_Alpha>();
                    if (pStatus != null) reductionRate = pStatus.pierceDamageReductionRate;
                }

                // actualHitsの回数分ループ
                for (int i = 0; i < actualHits; i++)
                {
                    // HPを減らす（1回目は今のdmg、2回目以降はさっき減衰されたdmgを使う）
                    health.TakeDamage(dmg);
                    hitCountsPerEnemy[targetObj]++;

                    // 貫通枠を消費する（最後の1ヒット＝もう貫通できない時は消費しない、もしくは0未満になる）
                    piercingCount--;

                    // 次のヒット（同じ敵の連続ヒット、もしくは次の敵へのヒット）のために威力と速度を減衰させる
                    dmg -= initialDmg * reductionRate;
                    if (dmg <= initialDmg * 0.1f) dmg = initialDmg * 0.1f;
                    
                    Speed -= initialSpeed * reductionRate;
                    if (Speed <= initialSpeed * 0.1f) Speed = initialSpeed * 0.1f;
                }

                // 速度が変更されたので、実際のRigidbodyの速度にも反映する
                if (rb != null)
                {
                    if (rb.velocity.sqrMagnitude > 0f)
                    {
                        // 既にループ内で減衰済みの「いまのSpeed」と「初期のSpeed」の比率を出す
                        float ratio = Speed / initialSpeed;

                        // 現在飛んでいる実際の向きと速度の長さを取得
                        Vector2 currentDir = rb.velocity.normalized;
                        float originalVelocityMag = rb.velocity.magnitude;

                        // AddForceで飛んだ当初の速度が最初から変化していないと仮定し、
                        // 「初期速度×今の比率」を新しい物理速度として上書きする
                        // （※ AddForce時の最終速度を originalMag に近いとみなす）
                        float newMag = originalVelocityMag * ratio;

                        rb.velocity = currentDir * newMag;
                    }
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
            // 着弾効果を発動
            foreach (var effect in activeEffects)
            {
                effect.OnHit(this, collision);
            }

            // 残りの貫通回数が0未満（＝もう貫通枠がない）もしくは壁に当たった場合は消滅
            if (piercingCount < 0 || collision.CompareTag("wall"))
            {
                if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
                {
                    Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }
            // 貫通枠が残っている場合は1フレーム無効化してすり抜ける
            else
            {
                StartCoroutine(TemporaryDisableCollider());
            }
        }
    }

    protected IEnumerator TemporaryDisableCollider()
    {
        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
            yield return null; // 1フレーム待機
            if (this != null && this.gameObject != null)
            {
                bulletCollider.enabled = true;
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
}
