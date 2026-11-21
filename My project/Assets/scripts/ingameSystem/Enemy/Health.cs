using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Health : _Health_Base
{
    public delegate void HPChangedHandler();
    protected GameObject canvasInstance;
    protected Transform canvasTransform; // エネミーのCanvasのTransform
    public GameObject damageTextPrefab; // ダメージ表示用のプレハブ
    public float DamageUIMagnitude = 0.1f;

    public enum MobKillHitStopSize
    {
        Auto = 0,
        Small = 1,
        Medium = 2,
        Large = 3
    }

    [Header("Kill HitStop Settings")]
    [Tooltip("撃破時のヒットストップサイズ（Auto時は mob1/TutorialFirstEnemy/SwarmMob→小、mob4-6→大、その他→中。インスペクターで個別変更可能）")]
    public MobKillHitStopSize killHitStopSize = MobKillHitStopSize.Auto;

    [Header("Defeat Audio Settings")]
    [Tooltip("雑魚敵撃破時のSE（未設定時は重め・大きめデフォルトの Resources/SE/SE_Mob_Defeat_Heavy を使用）")]
    public AudioClip defeatSEClip;
    [Range(0f, 1.5f)]
    public float defeatSEVolume = 1.0f;

    private static float lastGlobalDefeatSoundTime = -1f;
    private const float MinGlobalDefeatSoundInterval = 0.035f;
    private static AudioClip cachedMobDefeatSEClip = null;

    [Header("Flee Settings")]
    public bool canFlee = true;
    public float fleeTimeLimit = 10f;
    [Tooltip("最高速度に到達するまでの加速時間（秒）")]
    public float fleeAccelerationDuration = 3.0f;
    [Tooltip("逃亡時の初速")]
    public float fleeInitialSpeed = 0f;
    [Tooltip("逃亡時の最高速度")]
    public float fleeMaxSpeed = 30f;
    protected float currentFleeTime = 0f;
    protected float fleeElapsedTime = 0f;
    protected bool isFleeing = false;
    protected Slider fleeSlider;
    protected Vector2 fleeDir = Vector2.left;
    protected Color originalHPColor = Color.red;

    private static float lastGlobalEscapeSoundTime = -1f;
    private const float MinGlobalEscapeSoundInterval = 0.08f;
    private static AudioClip cachedEscapeSEClip = null;

    protected virtual void Start()
    {
        currentHP = HP;
        m_handler = gameObject.GetComponent<HPBar_Base>();
        setSlideHPBar();
    }

    public virtual void setSlideHPBar()
    {
        canvasInstance = Instantiate(
            Resources.Load<GameObject>("UI/EnemyHPCanvas"),
            gameObject.transform.position,
            Quaternion.identity
        );
        canvasTransform = canvasInstance.transform;
        canvasInstance.GetComponent<HPBarFollower>().setTargetTransform(gameObject.transform);
        //canvasInstance.transform.SetParent(transform);
        canvasInstance.transform.localPosition = new Vector3(0, 2, 0); // 必要に応じてオフセットを調整
        // HPバー(Slider)を取得
        hpSlider = canvasInstance.transform.Find("HPBar").GetComponent<Slider>();

        if (hpSlider != null)
        {
            if (hpSlider != null)
            {
                // HPバーの初期設定
                hpSlider.maxValue = HP;
                hpSlider.value = (float)currentHP; // HPバーの最初の値を現在のHPに設定

                Image hpFill = hpSlider.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
                if (hpFill != null)
                {
                    originalHPColor = hpFill.color;
                }

                if (canFlee && !isBoss && !isMidBoss)
                {
                    // 逃亡ゲージの生成
                    GameObject fleeBarObj = Instantiate(hpSlider.gameObject, hpSlider.transform.parent);
                    fleeBarObj.name = "FleeBar";
                    // 指定のスケールに変更
                    fleeBarObj.transform.localScale = new Vector3(3f, 1.5f, 1f);
                    
                    RectTransform fleeRect = fleeBarObj.GetComponent<RectTransform>();
                    if (fleeRect != null)
                    {
                        // 指定のYPosに変更（Xは元のHPバーを維持）
                        fleeRect.anchoredPosition = new Vector2(fleeRect.anchoredPosition.x, 50f);
                    }
                    else
                    {
                        fleeBarObj.transform.localPosition = new Vector3(fleeBarObj.transform.localPosition.x, 50f, fleeBarObj.transform.localPosition.z);
                    }

                    // 描画順を最前面（一番下）にする
                    fleeBarObj.transform.SetAsLastSibling();

                    fleeSlider = fleeBarObj.GetComponent<Slider>();
                    fleeSlider.maxValue = fleeTimeLimit;
                    fleeSlider.value = 0f;
                    
                    // 色をグレー等に変更（可能であれば）
                    Image fillImage = fleeBarObj.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        fillImage.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Canvas or HPBar not found in the enemy object.");
        }
    }

    protected override void Awake() { base.Awake(); }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (isFleeing)
        {
            isFleeing = false;
            currentFleeTime = 0f;
            fleeElapsedTime = 0f;
            if (fleeSlider != null) fleeSlider.value = 0f;

            // 1. アルファ値の復旧
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }

            // 2. 壁との衝突無視の解除
            Collider2D myCol = GetComponent<Collider2D>();
            if (myCol != null)
            {
                GameObject[] walls = GameObject.FindGameObjectsWithTag("wall");
                foreach (var wall in walls)
                {
                    Collider2D wallCol = wall.GetComponent<Collider2D>();
                    if (wallCol != null)
                    {
                        Physics2D.IgnoreCollision(myCol, wallCol, false);
                    }
                }
            }

            // 3. 移動スクリプトの再稼働
            var movement = GetComponent<Alpha_Enemy_Movement>();
            if (movement != null)
            {
                movement.enabled = true;
            }

            // 4. キャンバスの再表示と色の復旧
            if (canvasInstance != null)
            {
                canvasInstance.SetActive(true);
            }
            
            if (hpSlider != null)
            {
                Image hpFill = hpSlider.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
                if (hpFill != null)
                {
                    hpFill.color = originalHPColor;
                }
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isFleeing)
        {
            fleeElapsedTime += Time.deltaTime;

            // easeInQuad による加速: t^2
            float t = (fleeAccelerationDuration > 0f)
                ? Mathf.Clamp01(fleeElapsedTime / fleeAccelerationDuration)
                : 1f;
            float easeInQuadFactor = t * t;
            float currentSpeed = Mathf.Lerp(fleeInitialSpeed, fleeMaxSpeed, easeInQuadFactor);

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = fleeDir * currentSpeed;
            }

            // 画面外に出たら削除
            if (Camera.main != null)
            {
                Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
                if (viewportPos.x < -0.2f || viewportPos.x > 1.2f || viewportPos.y < -0.2f || viewportPos.y > 1.2f)
                {
                    if (canvasInstance != null) Destroy(canvasInstance);
                    Destroy(gameObject);
                }
            }
            return;
        }

        if (canFlee && !isBoss && !isMidBoss && !isDead && !isFleeing)
        {
            currentFleeTime += Time.deltaTime;
            if (fleeSlider != null)
            {
                fleeSlider.value = currentFleeTime;
            }

            if (currentFleeTime >= fleeTimeLimit)
            {
                StartFlee();
            }
        }
    }

    protected void PlayFleeSound()
    {
        if (Time.unscaledTime - lastGlobalEscapeSoundTime < MinGlobalEscapeSoundInterval) return;
        lastGlobalEscapeSoundTime = Time.unscaledTime;

        if (cachedEscapeSEClip == null)
        {
            cachedEscapeSEClip = Resources.Load<AudioClip>("SE/SE_Enemy_Escape_Insect_01_Kasa");
        }

        if (cachedEscapeSEClip != null && Alpha.Audio.SoundManager_Alpha.Instance != null)
        {
            float pitch = Random.Range(0.95f, 1.05f);
            Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(cachedEscapeSEClip, 0.75f, pitch);
        }
    }

    protected virtual void StartFlee()
    {
        isFleeing = true;
        fleeElapsedTime = 0f;
        PlayFleeSound();
        
        // 1. 半透明化
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.5f;
            sr.color = c;
        }

        // 2. 壁との接触判定のみ無効化（弾は当たる）
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol != null)
        {
            GameObject[] walls = GameObject.FindGameObjectsWithTag("wall");
            foreach (var wall in walls)
            {
                Collider2D wallCol = wall.GetComponent<Collider2D>();
                if (wallCol != null)
                {
                    Physics2D.IgnoreCollision(myCol, wallCol, true);
                }
            }
        }

        // 3. 既存の移動を停止し、プレイヤーと逆方向へ走らせる
        var movement = GetComponent<Alpha_Enemy_Movement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // 行動系AI（Alpha_EnemyAI）も強制停止
        var enemyAI = GetComponent<Alpha_EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.StopAllBehaviors(clearSpawnedObjectsOnStop: false);
            enemyAI.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            fleeDir = Vector2.left; // プレイヤーが見つからない場合のデフォルト
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 diff = transform.position - player.transform.position;
                fleeDir = diff.sqrMagnitude > 0.001f ? diff.normalized : Vector2.left;
            }
            rb.velocity = fleeDir * fleeInitialSpeed; // 初速からスタート（easeInQuadで加速）
        }

        // 4. HPバーの色を青に変更
        if (hpSlider != null)
        {
            Image hpFill = hpSlider.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
            if (hpFill != null)
            {
                hpFill.color = new Color(0.1f, 0.6f, 1f, 1f); // 鮮やかな青色
            }
        }
    }

    // ダメージを受け取るメソッド
    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;
        if (hpSlider != null)
        {
            SliderUpdate();
            ShowDamage(damage);
        }
        // Debug.Log(gameObject.name + " took " + damage + " damage. Remaining HP: " + currentHP);
        if (gameObject.tag == "Enemy" && currentHP <= 0)
        {
            isDead = true;
            Die();
        }
    }

    // CreateExpPos は不要になりますが、念のため残すか削除します
    private Vector3 CreateExpPos()
    {
        Vector3 ret = new Vector3(0, 0, 0);
        float randomPos;
        randomPos = Random.Range(-2f, 2f);
        ret.x = randomPos;
        randomPos = Random.Range(-2f, 2f);
        ret.y = randomPos;
        ret += transform.localPosition;
        return ret;
    }

    public void setMoneyCount(int count)
    {
        moneyCount = count;
    }

    /// <summary>
    /// タイムライン短縮時に、まだ逃亡していない雑魚敵の残留（逃亡）ゲージを巻き戻して延長する
    /// </summary>
    public void ExtendFleeTimer(float extraSeconds)
    {
        if (!canFlee || isBoss || isMidBoss || isDead || isFleeing) return;
        if (extraSeconds <= 0f) return;

        currentFleeTime = Mathf.Max(0f, currentFleeTime - extraSeconds);
        if (fleeSlider != null)
        {
            fleeSlider.value = currentFleeTime;
        }
    }

    /// <summary>
    /// 雑魚撃破時のヒットストップサイズ（1=小, 2=中, 3=大）を決定する
    /// </summary>
    protected int ResolveKillHitStopLevel()
    {
        if (killHitStopSize != MobKillHitStopSize.Auto)
        {
            return (int)killHitStopSize;
        }

        string lowerName = gameObject.name.ToLowerInvariant();

        // 小: mob1, TutorialFirstEnemy, swarmMob系
        if (lowerName.Contains("mob1") || lowerName.Contains("tutorialfirstenemy") || lowerName.Contains("swarmmob"))
        {
            return (int)MobKillHitStopSize.Small;
        }

        // 大: mob4〜mob6
        if (lowerName.Contains("mob4") || lowerName.Contains("mob5") || lowerName.Contains("mob6"))
        {
            return (int)MobKillHitStopSize.Large;
        }

        // エネミーラッシュ中の雑魚は基本 小
        if (Alpha.Flow.StageManager_Alpha.Instance != null &&
            Alpha.Flow.StageManager_Alpha.Instance.spawnManager != null &&
            Alpha.Flow.StageManager_Alpha.Instance.spawnManager.IsRushActive())
        {
            return (int)MobKillHitStopSize.Small;
        }

        // その他（mob2, mob3 等）は 中
        return (int)MobKillHitStopSize.Medium;
    }

    protected void PlayMobDefeatSound(int hitStopLevel)
    {
        if (Time.unscaledTime - lastGlobalDefeatSoundTime < MinGlobalDefeatSoundInterval) return;
        lastGlobalDefeatSoundTime = Time.unscaledTime;

        AudioClip clip = defeatSEClip;
        if (clip == null)
        {
            if (cachedMobDefeatSEClip == null)
            {
                cachedMobDefeatSEClip = Resources.Load<AudioClip>("SE/SE_Mob_Defeat_Heavy");
            }
            clip = cachedMobDefeatSEClip;
        }

        if (clip != null && Alpha.Audio.SoundManager_Alpha.Instance != null)
        {
            float pitch;
            float volMult;
            switch (hitStopLevel)
            {
                case 1: // Small (mob1, TutorialFirstEnemy, SwarmMob)
                    pitch = Random.Range(1.02f, 1.12f);
                    volMult = 0.88f;
                    break;
                case 3: // Large (mob4-6 強敵)
                    pitch = Random.Range(0.76f, 0.85f);
                    volMult = 1.18f;
                    break;
                default: // Medium (mob2, mob3 標準の重め＆大きめ)
                    pitch = Random.Range(0.92f, 1.02f);
                    volMult = 1.0f;
                    break;
            }
            Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(clip, defeatSEVolume * volMult, pitch);
        }
    }

    // HPが0になった時の処理
    protected virtual void Die()
    {
        // 共通のドロップ処理（経験値、オーブ、花弁）を呼び出す
        DropEnemyRewards();

        // 雑魚敵撃破時の重めSE＆ヒットストップを発動（ボス・中ボスは専用の撃破シネマティックを使用）
        if (!isBoss && !isMidBoss)
        {
            int hitStopLevel = ResolveKillHitStopLevel();
            PlayMobDefeatSound(hitStopLevel);

            if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
            {
                Alpha.Core.ProceduralJuiceManager_Alpha.Instance.TriggerMobKillHitStop(hitStopLevel);
            }
        }

        // 追従している体力バーCanvasを即座に破棄
        if (canvasInstance != null)
        {
            Destroy(canvasInstance);
            canvasInstance = null;
        }

        // 爆発エフェクトを1回だけ再生する（プール使用）
        GameObject explosionPrefab = Resources.Load<GameObject>("Objects/Effect/Effect_AetherExplosion");
        if (explosionPrefab != null)
        {
            GameObject effect = null;
            if (global::Alpha_ObjectPoolManager.Instance != null)
            {
                effect = global::Alpha_ObjectPoolManager.Instance.Rent(explosionPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                effect = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            var explosionScript = effect.GetComponent<Alpha.Enemy.Effect.AetherExplosionEffect_Alpha>();
            if (explosionScript != null)
            {
                explosionScript.sourcePrefab = explosionPrefab;
            }
        }

        // ボス・中ボスの特別なドロップ処理 (ボス用の特別なアイテムなどがあれば継続)
        if (Alpha.Flow.RewardManager_Alpha.Instance != null)
        {
            if (isBoss)
            {
                Alpha.Flow.RewardManager_Alpha.Instance.DropBossReward(transform.position, bossId);
                if (Alpha.Flow.StageManager_Alpha.Instance != null)
                {
                    Alpha.Flow.StageManager_Alpha.Instance.OnBossDefeated();
                }
            }
            else if (isMidBoss)
            {
                Alpha.Flow.RewardManager_Alpha.Instance.DropMidBossReward(transform.position);
                if (Alpha.Flow.StageManager_Alpha.Instance != null)
                {
                    Alpha.Flow.StageManager_Alpha.Instance.OnBossDefeated();
                }
            }
            else
            {
                int pointsToGive = rewardPoints;
                var escapeComp = GetComponent<Alpha.Enemy.EnemyEscape_Alpha>();
                if (escapeComp != null && escapeComp.IsNearEscape)
                {
                    pointsToGive = Mathf.RoundToInt(rewardPoints * 1.5f);
                    Debug.Log($"[Escape Bonus] Just kill before escape! Points: {rewardPoints} -> {pointsToGive} (1.5x)");
                    if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
                    {
                        Alpha.Core.ProceduralJuiceManager_Alpha.Instance.SpawnHitSparks(transform.position, new Color(1f, 0.85f, 0.2f), 8);
                    }
                }
                Alpha.Flow.RewardManager_Alpha.Instance.AddPoints(pointsToGive);
                
                if (Alpha.Result.RunStatsTracker_Alpha.Instance != null)
                {
                    bool isNear = escapeComp != null && escapeComp.IsNearEscape;
                    Alpha.Result.RunStatsTracker_Alpha.Instance.RecordMobKill(isNear);
                }
            }
        }

        Debug.Log(gameObject.name + " died.");
        // ここに死亡時の処理を書く
        Destroy(gameObject);
    }

    public void ShowDamage(float damage)
    {
        if (damageTextPrefab == null) return;

        // ダメージテキストの生成
        GameObject damageTextInstance = Instantiate(damageTextPrefab, canvasTransform);
        if (damageTextInstance == null) return;

        damageTextInstance.GetComponent<RectTransform>().localPosition = Vector3.zero;

        // テキストのスケールを調整
        damageTextInstance.GetComponent<RectTransform>().localScale =
            Vector3.one * DamageUIMagnitude;

        // テキスト内容を設定
        var damageUI = damageTextInstance.GetComponent<DamageUI3D>();
        if (damageUI != null)
        {
            damageUI.damage = damage;
        }
    }

    protected virtual void OnDestroy()
    {
        // 敵破棄時に体力バーCanvasも確実に破棄
        if (canvasInstance != null)
        {
            Destroy(canvasInstance);
            canvasInstance = null;
        }
    }
}
