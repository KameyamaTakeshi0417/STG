using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Health : _Health_Base
{
    public delegate void HPChangedHandler();
    GameObject canvasInstance;
    Transform canvasTransform; // エネミーのCanvasのTransform
    public GameObject damageTextPrefab; // ダメージ表示用のプレハブ
    public float DamageUIMagnitude = 0.1f;

    [Header("Flee Settings")]
    public bool canFlee = true;
    public float fleeTimeLimit = 10f;
    protected float currentFleeTime = 0f;
    protected bool isFleeing = false;
    protected Slider fleeSlider;
    protected Vector2 fleeDir = Vector2.left;
    protected Color originalHPColor = Color.red;

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
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = fleeDir * 30f;
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

    protected virtual void StartFlee()
    {
        isFleeing = true;
        
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

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            fleeDir = Vector2.left; // プレイヤーが見つからない場合のデフォルト
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                fleeDir = (transform.position - player.transform.position).normalized;
            }
            rb.velocity = fleeDir * 15f; // 初期の15fに速度を戻す
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
            if (hpSlider != null)
            {
                Destroy(hpSlider);
            }

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

    // HPが0になった時の処理
    protected virtual void Die()
    {
        // 共通のドロップ処理（経験値、オーブ、花弁）を呼び出す
        DropEnemyRewards();

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
                Alpha.Flow.RewardManager_Alpha.Instance.AddPoints(rewardPoints);
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
}
