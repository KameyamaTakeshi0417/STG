using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Pierce Settings")]
    [Tooltip("このエネミーに対して許容される貫通の最大回数")]
    public int PierceVolume = 1;

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
            int remainingExp = Exp;
            
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

        TakeDamage(damage);
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
        return;
    }

    public void AddCurrentHP(float set)
    {
        float ret = set;
        if (currentHP + set > HP)
            set = HP;
        currentHP += set;
    }
}
