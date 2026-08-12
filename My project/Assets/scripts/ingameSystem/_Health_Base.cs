using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Alpha.Core.Utils;

public class _Health_Base : MonoBehaviour
{
    public float HP = 100f; // 初期HP

    public float currentHP;
    public int Exp;
    protected int moneyCount;
    protected Slider hpSlider; //HPバー（スライダー）
    protected HPBar_Base m_handler;
    public float VulnerableTime = 0f;
    public bool VulnerableFlg = false;
    public bool isDead = false;

    [Header("Pierce Settings")]
    [Tooltip("このエネミーに対して許容される貫通の最大回数")]
    public int PierceVolume = 1;

    [Header("Reward Status")]
    public float DropBounusExp = 0; // ドロップする基本の経験値
    public float DropRateExp = 0;   // 経験値ドロップ率

    [Header("Contact Damage")]
    public float contactDamage = 10f; // 接触ダメージ（0ならダメージなし）
    private bool isTouchingPlayer = false;
    private PlayerHealth currentPlayerHealth = null;

    [Header("UI Status")]
    public int LifeCount = 1;       // エリートなどが持つ複数ゲージの数
    public bool isNormalMob = true; // ザコ敵かどうか（演出などに使用）

    [Header("Status Effects")]
    [Tooltip("感電の蓄積値。0より大きい場合帯電状態")]
    public int VoltCount = 0;

    [Header("Stun Settings")]
    [Tooltip("現在のスタン耐性値（この値以上のスタン秒数でないとスタンしない）")]
    public float StunResistance = 0f;
    [Tooltip("スタンを受けた際に増加するスタン耐性値")]
    public float BaseStunResistance = 0.5f;
    
    [HideInInspector] public bool isStunned = false;
    protected float currentStunTime = 0f;

    [Header("Barrier Settings")]
    public bool isBarrierActive = false;
    public float barrierEndurableDamage = 0f;
    public float barrierBaseRespawnTime = 0f;
    [HideInInspector] public float currentBarrierRespawnTime = 0f;
    [HideInInspector] public float barrierRespawnTimer = 0f;
    public GameObject barrierVisualObject = null;


    [Header("Reward Settings")]
    [Tooltip("この敵を倒したときに得られる報酬ポイント")]
    public int rewardPoints = 10;
    [Tooltip("オーブドロップ確率 (0.0 〜 1.0)")]
    public float orbDropChance = 0.05f;
    [Tooltip("確定でオーブをドロップするかどうか")]
    public bool forceDropOrb = false;
    [Tooltip("中ボスかどうか")]
    public bool isMidBoss = false;
    [Tooltip("ボスかどうか")]
    public bool isBoss = false;
    [Tooltip("ボスのID（ボスの場合のみ）")]
    public string bossId = "";

    /// <summary>
    /// 全エネミー共通のドロップ処理（経験値、オーブ、花弁など）
    /// </summary>
    public void DropEnemyRewards()
    {
        if (Alpha.Flow.RewardManager_Alpha.Instance == null) return;
        var manager = Alpha.Flow.RewardManager_Alpha.Instance;

        // 1. 経験値のドロップ
        if (Exp > 0)
        {
            // 設定された経験値に対して0～3のランダムなブレを追加
            int remainingExp = Exp + Random.Range(0, 4);
            
            int count100 = remainingExp / 100;
            remainingExp %= 100;
            
            int count10 = remainingExp / 10;
            remainingExp %= 10;
            
            int count1 = remainingExp;
            
            manager.SpawnExp(transform.position, 100, 1.7f, count100);
            manager.SpawnExp(transform.position, 10, 1.3f, count10);
            manager.SpawnExp(transform.position, 1, 1.0f, count1);
            
            Debug.Log($"[{gameObject.name}] Dropped EXP: 100x{count100}, 10x{count10}, 1x{count1}");
        }

        // 2. オーブのドロップ判定
        if (forceDropOrb || Random.value <= orbDropChance)
        {
            int rarity = manager.mobDropTable.GetRandomRarity();
            manager.SpawnOrb(transform.position, rarity, Alpha.Data.OrbSource_Alpha.Mob);
            Debug.Log($"[{gameObject.name}] Dropped an Orb via RewardManager! Rarity: {rarity}");
        }

        // 3. 花弁のドロップ判定 (エリートかボスの場合)
        bool isElite = GetComponent<Alpha_EliteHealth>() != null || GetComponent<CirculatorEnemy>() != null;
        if (isMidBoss || isBoss || isElite)
        {
            int currentStage = Alpha.Flow.StageManager_Alpha.Instance != null ? Alpha.Flow.StageManager_Alpha.Instance.currentStageIndex : 1;
            manager.SpawnPetal(transform.position, currentStage);
            Debug.Log($"[{gameObject.name}] Dropped {currentStage} Petals!");
        }
    }

    protected virtual void Awake()
    {
        // 物理演算の衝突による意図しない回転（くるくる回る現象）を防ぐ
        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null) rb2d.freezeRotation = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (VulnerableFlg)
        {
            VulnerableTime -= Time.deltaTime; // 0.1f から Time.deltaTime に修正
            if (VulnerableTime <= 0f)
            {
                VulnerableFlg = false;
            }
        }

        if (isStunned)
        {
            currentStunTime -= Time.deltaTime;
            if (currentStunTime <= 0f)
            {
                isStunned = false;
            }
        }

        if (!isBarrierActive && barrierBaseRespawnTime > 0f)
        {
            barrierRespawnTimer -= Time.deltaTime;
            if (barrierRespawnTimer <= 0f)
            {
                isBarrierActive = true;
                if (barrierVisualObject != null)
                {
                    barrierVisualObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// スタンを付与する。耐性値によって軽減され、0以下になれば無効化される。
    /// 一度スタンを受けると、耐性値が加算される。
    /// </summary>
    public virtual void ApplyStun(float stunDuration)
    {
        float effectiveStun = stunDuration - StunResistance;
        
        if (effectiveStun > 0f)
        {
            isStunned = true;
            // すでにスタン中で、より長いスタンを受けた場合は上書き
            if (currentStunTime < effectiveStun)
            {
                currentStunTime = effectiveStun;
            }
            
            // スタン耐性を上昇させる
            StunResistance += BaseStunResistance;
            Debug.Log($"[{gameObject.name}] Stunned for {effectiveStun}s. Next Resistance: {StunResistance}");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Resisted Stun! (Resistance: {StunResistance} >= Duration: {stunDuration})");
        }
    }

    public void SliderUpdate()
    {
        hpSlider.value = currentHP; //スライダは０〜1.0で表現するため最大HPで割って少数点数字に変換
    }

    public void ApplyDamage(float damage, Bullet_Base sourceBullet = null)
    {
        if (isBarrierActive)
        {
            bool hasPierce = sourceBullet != null && sourceBullet.piercingCount > 0;
            if (hasPierce)
            {
                // ピアス攻撃の場合、バリア破壊＆ピアスの残機を0にしてダメージは受けない
                BreakBarrier();
                sourceBullet.piercingCount = 0;
                return;
            }
            else
            {
                if (damage > barrierEndurableDamage)
                {
                    // 耐久値以上のダメージを受けた場合、差分ダメージを受けてバリア破壊
                    BreakBarrier();
                    damage -= barrierEndurableDamage;
                }
                else
                {
                    // 耐久値以下の場合は無効化
                    return;
                }
            }
        }

        // --- Juice Additions ---
        var juice = GetComponent<Alpha.Core.EntityJuice_Alpha>();
        if (juice == null) 
        {
            juice = gameObject.AddComponent<Alpha.Core.EntityJuice_Alpha>();
        }

        bool isNormalMob = !isMidBoss && !isBoss && GetComponent<Alpha_EliteHealth>() == null && GetComponent("CirculatorEnemy") == null;

        if (juice != null)
        {
            // Flash white for enemies
            juice.FlashColor(Color.white, 0.05f);

            if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
            {
                Alpha.Core.ProceduralJuiceManager_Alpha.Instance.SpawnHitSparks(transform.position, Color.white, 3);
            }

            // Squash if normal mob
            if (isNormalMob)
            {
                juice.SquashAndStretch(new Vector3(1.2f, 0.8f, 1f), 0.1f);
            }
        }
        // --- End Juice Additions ---

        TakeDamage(damage);

        // HitStop on death for normal mobs
        if (currentHP <= 0f && isNormalMob)
        {
            if (Alpha.Core.JuiceManager_Alpha.Instance != null)
            {
                Alpha.Core.JuiceManager_Alpha.Instance.HitStop(0.05f);
            }
        }
    }

    protected void BreakBarrier()
    {
        isBarrierActive = false;
        if (barrierVisualObject != null)
        {
            barrierVisualObject.SetActive(false);
        }

        // 次回の復活時間を設定し、次回用の復活時間を延長する
        barrierRespawnTimer = currentBarrierRespawnTime;
        currentBarrierRespawnTime += barrierBaseRespawnTime;
    }

    public virtual void TakeDamage(float damage) { }

    public void setExp(int exp)
    {
        Exp = exp;
    }

    public float getHP()
    {
        return HP;
    }

    public void setHP(float hp)
    {
        HP = hp;
        return;
    }

    public float getCurrentHP()
    {
        return currentHP;
    }

    public void setCurrentHP(float set)
    {
        currentHP = set;
    }

    public void addHP(float hp)
    {
        HP += hp;
    }

    protected virtual void OnEnable()
    {
        if (Alpha_TickManager.Instance != null)
        {
            Alpha_TickManager.Instance.OnTick += HandleTick;
        }
    }

    protected virtual void OnDisable()
    {
        if (Alpha_TickManager.Instance != null)
        {
            Alpha_TickManager.Instance.OnTick -= HandleTick;
        }
        isTouchingPlayer = false;
        currentPlayerHealth = null;
    }

    private void HandleTick()
    {
        if (contactDamage <= 0f) return;
        if (isDead) return;
        
        if (isTouchingPlayer && currentPlayerHealth != null)
        {
            currentPlayerHealth.TakeDamage(contactDamage);
            // プレイヤーを無敵にはしない（Tickごとの連続ダメージを許容）
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealInitialContactDamage(collision.collider);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        TryDealInitialContactDamage(collider);
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        RemoveContact(collision.collider);
    }

    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        RemoveContact(collider);
    }

    protected void TryDealInitialContactDamage(Collider2D col)
    {
        if (contactDamage <= 0f) return;
        if (isDead) return;

        if (col.CompareTag("Player"))
        {
            var pHealth = col.GetComponentInParent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(contactDamage); // 接触時に即ダメージ
                isTouchingPlayer = true;
                currentPlayerHealth = pHealth;
            }
        }
    }

    protected void RemoveContact(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            isTouchingPlayer = false;
            currentPlayerHealth = null;
        }
    }

    public void AddCurrentHP(float set)
    {
        float ret = set;
        if (currentHP + set > HP)
            set = HP;
        currentHP += set;
    }
}
