using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Alpha.Core.Utils;

public class Bullet_Base : MonoBehaviour, IAlphaPoolable, IBombDestructible
{
    // 霑ｽ蜉�: 閾ｪ蛻・′逕溘∪繧後◆蜈・・繝励Ξ繝上ヶ縺ｮ蜿ら・繧剃ｿ晄戟縺吶ｋ
    public GameObject sourcePrefab;

    [Header("Alignment Settings")]
    [Tooltip("Tooltip")]
    public bool isEnemyBullet = false;
    [Tooltip("Tooltip")]
    public bool canHitBoth = false;
 
    public Vector3 originalAimDirection; // Reverse繝代ち繝ｼ繝ｳ縺ｧ蜈・・譁ｹ蜷代↓謌ｻ繧九◆繧∬ｨ俶・逕ｨ
    public float reverseTimeRemaining = 0f; // Reverse繝代ち繝ｼ繝ｳ縺ｮ蠕碁€€谿九ｊ譎る俣
    public Transform lockedTarget; // 繝ｭ繝・け繧ｪ繝ｳ蟇ｾ雎｡繧剃ｿ晄戟

    public virtual void OnRentFromPool()
    {
        // 蜀咲匳蝣ｴ譎ゅ↓繝ｪ繧ｻ繝・ヨ蜃ｦ逅・
        piercingCount = 0; // 邯呎価蜈茨ｼ・iercingBullet縺ｪ縺ｩ・峨〒蠢・ｦ√↓蠢懊§縺ｦ override 縺励※蜀崎ｨｭ螳壹☆繧九・繝ｼ繧ｹ縺ｨ縺励※0繧ｯ繝ｪ繧｢
        extraShotCount = 0; // 追加発射数のリセット
        voltTickReduceCount = 0; // 毒絡弾のTick短縮カウントリセット
        secondaryDamageMultiplier = 1.0f; // 派生ダメージ倍率のリセット
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
        // 髱櫁｡ｨ遉ｺ縺ｫ縺ｪ繧狗峩蜑阪・蜃ｦ逅・ｼ亥ｿ・ｦ√↑繧峨・・・
        StopAllCoroutines();
    }
    public string Objname;
    protected Rigidbody2D rb;
    public float dmg; // 蠑ｾ縺ｮ繝€繝｡繝ｼ繧ｸ驥・
    public float Speed; //蠑ｾ縺ｮ蜃ｺ繧矩€溷ｺｦ
    public float DestroyTime; //蠑ｾ縺ｮ蟄伜惠縺吶ｋ譎る俣
    public float bullettype = 0; //蠑ｾ縺ｮ繧ｿ繧､繝玲ｱｺ螳・
    public Vector3 rotate; //蠑ｾ縺ｮ逋ｺ蟆・ｧ・

    public int rarelity; //繧ｪ繝悶ず繧ｧ繧ｯ繝医・謖吝虚縺悟､峨ｏ繧九ｂ縺ｮ
    public string bulletName;
    public float addDmg; //繝€繝｡繝ｼ繧ｸ蛟咲紫縺ｮ縺の°繧峨↑縺・崋螳壹ム繝｡繝ｼ繧ｸ
    public int piercingCount = 0;
    public int extraShotCount = 0; // サーキュラー等のサブバレット増加用
    public int voltTickReduceCount = 0; // 毒絡弾（Volt）のダメージTick短縮用
    public float secondaryDamageMultiplier = 1.0f; // 派生ダメージ（Volt, Explosion, 子弾等）の威力倍率

    // 雋ｫ騾壼・逅・畑繧ｹ繝・・繝・
    protected float initialDmg; // 貂幄｡ｰ險育ｮ励・繝ｼ繧ｹ縺ｮ蛻晄悄繝€繝｡繝ｼ繧ｸ
    protected Dictionary<GameObject, int> hitCountsPerEnemy = new Dictionary<GameObject, int>(); // 謨ｵ縺斐→縺ｮ繝偵ャ繝亥屓謨ｰ
    protected Collider2D bulletCollider; // 蜀榊愛螳夂畑縺ｫ繧ｳ繝ｩ繧､繝€繝ｼ繧剃ｸ€譎ら噪縺ｫ蛻ｶ蠕｡

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
                // 繝ｪ繝舌・繧ｹ譎る俣縺檎ｵゆｺ・＠縺溽椪髢薙∵悽譚･縺ｮ騾ｲ陦梧婿蜷托ｼ・riginalAimDirection・峨∈蜷代″逶ｴ繧・
                if (rb != null)
                {
                    rb.velocity = originalAimDirection.normalized * (Speed * 0.02f);
                }
                
                // 蜷代″繧ゆｿｮ豁｣縺吶ｋ
                float rotationAngle = Mathf.Atan2(originalAimDirection.y, originalAimDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));
                
                // 莉･髯阪√・繝ｼ繝溘Φ繧ｰ遲峨・繧ｨ繝輔ぉ繧ｯ繝医′縺ゅｌ縺ｰ縺昴■繧峨′閾ｪ逋ｺ逧・↓蜉ｹ縺榊ｧ九ａ繧・
            }
        }
    }

    public void setDmg(float damage)
    {
        dmg = damage;
    }

    //蠑ｾ縺ｮ謦・▽隗貞ｺｦ縺ｮ豁｣隕丞喧
    public void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(
            0,
            0,
            MathF.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90
        );
        rotate = rot.normalized;
    }

    //蠑ｾ縺ｮ騾溷ｺｦ豎ｺ螳・
    public void setBulletSpeed(float mag) { }

    //蠑ｾ縺ｮ迚ｹ諤ｧ豎ｺ螳・

    //蠑ｾ荳ｸ縺ｮ雋ｫ騾壼屓謨ｰ險ｭ螳・


    [Tooltip("Tooltip")]
    public bool preventAutoDestroy = false;

    // 雋ｫ騾壼ｼｾ逕ｨ縺ｮ繝ｭ繝ｼ繧ｫ繝ｫ繝繝｡繝ｼ繧ｸ貂幄｡ｰ邇・ｼ・1縺ｮ蝣ｴ蜷医・繧ｰ繝ｭ繝ｼ繝舌Ν險ｭ螳壹ｒ菴ｿ逕ｨ・・
    public float localPierceDamageReductionRate = -1f;

    public bool canUseAllEffects = false;
    public List<Alpha_Effect_Base> activeEffects = new List<Alpha_Effect_Base>();

    public void setStatus(Vector3 Prot, float pSpeed, float pDmg)
    {
        rotate = Prot;
        Speed = pSpeed;
        dmg = pDmg;
    }

    // 豁ｦ蝎ｨ縺ｮ蜉ｹ譫懊ョ繝ｼ繧ｿ繧貞ｼｾ縺ｫ蜑ｲ繧雁ｽ薙※繧・
    public void SetWeaponEffects(List<Alpha_Effect_Base> effects, bool allEffects)
    {
        canUseAllEffects = allEffects;
        activeEffects.Clear();

        if (effects == null) return;

        foreach (var newEffect in effects)
        {
            if (newEffect == null) continue;
            
            var clonedEffect = newEffect.Clone();
            clonedEffect.canUseAllEffects = canUseAllEffects; // 全効果発動可能フラグを渡す

            // canUseAllEffectsがtrueの場合は、位置に関わらず各エフェクト1つあれば全効果が発動するので、型のみで重複排除する
            var existingEffect = activeEffects.Find(e => 
                e.GetType() == clonedEffect.GetType() && 
                (canUseAllEffects || e.equipPosition == clonedEffect.equipPosition)
            );
            
            if (existingEffect != null)
            {
                existingEffect.stackCount++;
                // 全効果発動時は、最も高いレアリティを優先する
                if (canUseAllEffects && clonedEffect.rarity > existingEffect.rarity)
                {
                    existingEffect.rarity = clonedEffect.rarity;
                }
            }
            else
            {
                activeEffects.Add(clonedEffect);
            }
        }
    }

    protected float initialSpeed; // 逋ｺ蟆・凾縺ｮ螽∝鴨繧貞渕貅悶せ繝斐・繝峨→縺励※險俶・・郁ｿｽ蜉�驛ｨ蛻・ｼ・

    public void shoot()
    {
        initialDmg = dmg; // 逋ｺ蟆・凾縺ｮ螽∝鴨繧貞渕貅悶ム繝｡繝ｼ繧ｸ縺ｨ縺励※險俶・
        initialSpeed = Speed; // 逋ｺ蟆・凾縺ｮ繧ｹ繝斐・繝峨ｒ蝓ｺ貅悶→縺励※險俶・
        bulletCollider = GetComponent<Collider2D>(); // 繧ｳ繝ｩ繧､繝繝ｼ蜿門ｾ・

        // 繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繧ｿ繧ｹ繧貞叙蠕暦ｼ亥ｼｾ縺ｫ蛟句挨繧､繝ｳ繧ｿ繝ｼ繝舌Ν縺ｪ縺ｩ繧貞渚譏�縺吶ｋ縺溘ａ・・
        playerStatusManager_Alpha pStatus = null;
        GameObject manager = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
        if (manager != null)
        {
            pStatus = manager.GetComponent<playerStatusManager_Alpha>();
        }

        // 蠑ｾ繧呈茶縺｡蜃ｺ縺吝燕縺ｫ蛻晄悄蛹門・逅・-> 逕滓・譎ょ柑譫懊ｒ逋ｺ蜍・
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

    //蠑ｾ繧呈茶縺｡蜃ｺ縺・
    protected virtual IEnumerator move()
    {
        int count = 0;

        // 蠑ｾ縺ｮ逋ｺ蟆・
        rb = gameObject.GetComponent<Rigidbody2D>();

        // 迚ｩ逅・お繝ｳ繧ｸ繝ｳ縺ｮ諷｣諤ｧ繧・｡晉ｪ√↓繧医ｋ貂幃溘ｒ髦ｲ縺舌◆繧√∝・騾溘ｒ蠑ｷ蛻ｶ縺吶ｋ
        if (rb != null)
        {
            rb.velocity = rotate.normalized * (Speed * 0.02f);
        }

        while (count <= DestroyTime || preventAutoDestroy)
        {
            // 蠑ｾ縺ｮ菴咲ｽｮ繧呈峩譁ｰ縺吶ｋ・井ｿ晁ｭｷ縺輔ｌ縺ｦ縺・ｋ髢薙・蟇ｿ蜻ｽ繧ｫ繧ｦ繝ｳ繝医ｒ騾ｲ繧√↑縺・ｼ・
            if (!preventAutoDestroy)
            {
                count++;
            }

            // 豈弱ヵ繝ｬ繝ｼ繝�縲∫樟蝨ｨ縺ｮ騾ｲ陦梧婿蜷・rotate)縺ｨ繧ｹ繝斐・繝・Speed)縺ｧ騾溷ｺｦ繧剃ｸ頑嶌縺阪＠邯壹￠繧・
            // ・医・繝ｬ繧､繝､繝ｼ遘ｻ蜍慕ｳｻ縺ｮ謾ｹ菫ｮ縺ｨ蜷梧ｧ倥∫黄逅・ｼ皮ｮ励↓繧医ｋ諢丞峙縺励↑縺・ｸ幃溘ｒ螳悟・縺ｫ髦ｲ縺撰ｼ・
            if (rb != null)
            {
                rb.velocity = rotate.normalized * (Speed * 0.02f);
            }

            // 逕ｻ髱｢螟厄ｼ医ヰ繧ｦ繝ｳ繝繝ｪ・峨メ繧ｧ繝・け
            if (Alpha.Core.ScreenBoundaryManager_Alpha.Instance != null)
            {
                if (Alpha.Core.ScreenBoundaryManager_Alpha.Instance.IsOutOfBounds(transform.position))
                {
                    break; // 繝ｫ繝ｼ繝励ｒ謚懊￠縺ｦ豸域ｻ・・逅・∈
                }
            }

            // 闊ｪ陦梧凾蜉ｹ譫懊ｒ逋ｺ蜍・(繝ｫ繝ｼ繝苓・菴薙・0.01遘貞捉譛・
            foreach (var effect in activeEffects)
            {
                effect.OnFlight(this, 0.01f);
            }

            yield return new WaitForSeconds(0.01f);
        }

        // 蟇ｿ蜻ｽ縺ｧ豸域ｻ・凾繧ら捩蠑ｾ謇ｱ縺・ｼ亥ｯｾ雎｡縺ｯnull・峨↓縺吶ｋ
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
        // 雋ｫ騾壻ｸｭ・・gnoreCollision迥ｶ諷具ｼ峨・繧ｳ繝ｩ繧､繝繝ｼ縺九ｉ縺ｮ繧､繝吶Φ繝医・辟｡隕悶☆繧・
        if (ignoredColliders.Contains(collision)) return;

        bool hitSomething = false;

        // 陦晉ｪ√＠縺溘が繝悶ず繧ｧ繧ｯ繝医・繧ｿ繧ｰ繧偵メ繧ｧ繝・け
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            // 謨ｵ蜻ｳ譁ｹ縺ｮ蛻､螳・
            if (!canHitBoth)
            {
                if (isEnemyBullet && collision.CompareTag("Enemy")) return;
                if (!isEnemyBullet && collision.CompareTag("Player")) return;
            }

            // HP繧呈戟縺､繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ蜿門ｾ暦ｼ郁ｦｪ繧ｪ繝悶ず繧ｧ繧ｯ繝医↓莉倥＞縺ｦ縺・ｋ蝣ｴ蜷医ｂ閠・・縺励※ GetComponentInParent 繧剃ｽｿ逕ｨ・・
            _Health_Base health = collision.GetComponentInParent<_Health_Base>();
            if (health != null)
            {
                GameObject targetObj = health.gameObject; // 繝繝｡繝ｼ繧ｸ繧貞女縺代◆譛ｬ菴薙ｒ繧ｿ繝ｼ繧ｲ繝・ヨ縺ｨ縺励※險倬鹸
                if (!hitCountsPerEnemy.ContainsKey(targetObj))
                {
                    hitCountsPerEnemy[targetObj] = 0;
                }

                int prevHitCount = hitCountsPerEnemy[targetObj];
                // PierceVolume縺・莉･荳具ｼ域悴險ｭ螳壹↑縺ｩ・峨・蝣ｴ蜷医・譛菴・蝗槭→縺励※謇ｱ縺・
                int pVol = health.PierceVolume > 0 ? health.PierceVolume : 1;

                // 譌｢縺ｫ縺薙・謨ｵ縺ｮ譛螟ｧ繝偵ャ繝域焚縺ｫ驕斐＠縺ｦ縺・ｋ蝣ｴ蜷医・菴輔ｂ縺励↑縺・
                if (prevHitCount >= pVol) return;

                // 莉雁屓縺ｮ陦晉ｪ√〒荳弱∴繧九∋縺阪ヲ繝・ヨ蝗樊焚・亥ｼｾ縺ｮ谿九ｊ雋ｫ騾壼屓謨ｰ+1 縺ｨ縲∵雰縺ｮ谿九ｊ險ｱ螳ｹ繝偵ャ繝域焚縺ｮ蟆代↑縺・婿・・
                // 窶ｻ piercingCount 縺梧ｮ九▲縺ｦ縺・ｋ蝗樊焚 = 縺ゅ→縲碁壹ｊ謚懊￠繧峨ｌ繧九榊屓謨ｰ
                //   縺､縺ｾ繧翫ヲ繝・ヨ縺ｧ縺阪ｋ蝗樊焚縺ｯ piercingCount + 1 蝗・
                int allowableHits = pVol - prevHitCount;
                int actualHits = Mathf.Min(piercingCount + 1, allowableHits);

                // 貂幄｡ｰ邇・ｒ蜿門ｾ・
                float reductionRate = 0.25f;
                
                // 縺ｾ縺壹・蠑ｾ閾ｪ霄ｫ・郁｣・ｙ蜉ｹ譫懃ｭ会ｼ峨・貂幄｡ｰ邇・ｨｭ螳壹′縺ゅｌ縺ｰ縺昴ｌ繧貞━蜈医☆繧・
                if (localPierceDamageReductionRate >= 0f)
                {
                    reductionRate = localPierceDamageReductionRate;
                }
                else
                {
                    // 縺ｪ縺代ｌ縺ｰ繝励Ξ繧､繝､繝ｼ繧ｹ繝・・繧ｿ繧ｹ・医・繝阪・繧ｸ繝｣繝ｼ・峨・險ｭ螳壼､繧貞叙蠕・
                    GameObject manager = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
                    if (manager != null)
                    {
                        var pStatus = manager.GetComponent<playerStatusManager_Alpha>();
                        if (pStatus != null) reductionRate = pStatus.pierceDamageReductionRate;
                    }
                }

                // actualHits縺ｮ蝗樊焚蛻・Ν繝ｼ繝・
                for (int i = 0; i < actualHits; i++)
                {
                    // HP繧呈ｸ帙ｉ縺吶・蝗樒岼縺ｯ莉翫・dmg縲・蝗樒岼莉･髯阪・縺輔▲縺肴ｸ幄｡ｰ縺輔ｌ縺歸mg繧剃ｽｿ縺・・
                    health.ApplyDamage(dmg, this);
                    hitCountsPerEnemy[targetObj]++;

                    // 雋ｫ騾壽棧繧呈ｶ郁ｲｻ縺吶ｋ縲ゑｼ域怙蠕後・1繝偵ャ繝茨ｼ昴ｂ縺・ｲｫ騾壹〒縺阪↑縺・凾縺ｯ豸郁ｲｻ縺励↑縺・√ｂ縺励￥縺ｯ0譛ｪ貅縺ｫ縺ｪ繧具ｼ・
                    piercingCount--;

                    // 谺｡縺ｮ繝偵ャ繝茨ｼ亥酔縺俶雰縺ｮ騾｣邯壹ヲ繝・ヨ縲√ｂ縺励￥縺ｯ谺｡縺ｮ謨ｵ縺ｸ縺ｮ繝偵ャ繝茨ｼ峨・縺溘ａ縺ｫ螽∝鴨繧呈ｸ幄｡ｰ縺輔○繧・
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
            // 逹蠑ｾ蜉ｹ譫懊ｒ逋ｺ蜍・
            foreach (var effect in activeEffects)
            {
                effect.OnHit(this, collision);
            }

            // 谿九ｊ縺ｮ雋ｫ騾壼屓謨ｰ縺・譛ｪ貅・茨ｼ昴ｂ縺・ｲｫ騾壽棧縺後↑縺・ｼ峨ｂ縺励￥縺ｯ螢√↓蠖薙◆縺｣縺溷�ｴ蜷医・豸域ｻ・
            if (piercingCount < 0 || collision.CompareTag("wall"))
            {
                // 繝峨Ο繝ｼ繝ｳ縺ｮ繧医≧縺ｪCircularObject縺九▽preventAutoDestroy縺ｮ蝣ｴ蜷医・縺ｿ豸域ｻ・ｒ蜈阪ｌ繧・
                if (preventAutoDestroy && GetComponent<CircularObject>() != null)
                {
                    // 菫晁ｭｷ縺輔ｌ縺ｦ縺・ｋ蝣ｴ蜷医・豸域ｻ・○縺壹√☆繧頑栢縺代＆縺帙ｋ
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
            // 雋ｫ騾壽棧縺梧ｮ九▲縺ｦ縺・ｋ蝣ｴ蜷医・1繝輔Ξ繝ｼ繝�辟｡蜉ｹ蛹悶＠縺ｦ縺吶ｊ謚懊￠繧・
            else
            {
                StartCoroutine(TemporaryDisableCollider(collision));
            }
        }
    }

    // 雋ｫ騾壹＠縺溷ｯｾ雎｡縺ｨ縺ｮ迚ｩ逅・噪縺ｪ陦晉ｪ∝愛螳壹ｒ辟｡隕悶☆繧九Μ繧ｹ繝・
    private HashSet<Collider2D> ignoredColliders = new HashSet<Collider2D>();

    protected IEnumerator TemporaryDisableCollider(Collider2D targetCollider)
    {
        if (bulletCollider != null && targetCollider != null)
        {
            // 蜷後§謨ｵ縺ｫ菴募ｺｦ繧ょｽ薙◆繧峨↑縺・ｈ縺・↓縲√°縺､迚ｩ逅・噪縺ｫ蠑輔▲縺九°縺｣縺ｦ貂幃溘＠縺ｪ縺・ｈ縺・↓繧ｳ繝ｪ繧ｸ繝ｧ繝ｳ繧堤┌隕悶☆繧・
            Physics2D.IgnoreCollision(bulletCollider, targetCollider, true);
            ignoredColliders.Add(targetCollider);
            
            // 雋ｫ騾壼ｾ後↓蜀阪・蜷後§謨ｵ縺ｫ蠖薙◆繧九％縺ｨ繧定ｨｱ蜿ｯ縺吶ｋ縺九←縺・°縺ｯ繧ｲ繝ｼ繝�縺ｮ莉墓ｧ倥↓繧医ｋ縺後・
            // 莉雁屓縺ｯ縲碁壹ｊ謚懊￠繧矩俣縲阪□縺醍┌隕悶＠縲∽ｸ螳壽凾髢難ｼ井ｾ九∴縺ｰ0.5遘抵ｼ牙ｾ後↓辟｡隕悶ｒ隗｣髯､縺吶ｋ
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
        if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
        {
            Alpha.Core.ProceduralJuiceManager_Alpha.Instance.SpawnRipple(transform.position, isEnemyBullet ? Color.white : new Color(0.8f, 0.4f, 0.4f), 0.2f, 0.8f, 0.15f);
        }
        Destroy(this.gameObject);
    }
    public void GenerateAnotherChildBullet() { }

    public void OnBombDestruct()
    {
        // プレイヤーの弾（isEnemyBullet == false）はボムで消えない
        if (!isEnemyBullet) return;

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

