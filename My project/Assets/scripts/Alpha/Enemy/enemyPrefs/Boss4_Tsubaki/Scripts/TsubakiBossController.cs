using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// 長女・ツバキのボス統括コントローラー
    /// - 3フェーズの進行監視
    /// - Phase 3 終盤（HP 20〜30%以下）での「限界演出」（吹雪途切れ＆よろめく隙）
    /// - 決着時（HP 0）：
    ///   爆破ではなく膝をつき、画面の全弾・トラップ・檻・吹雪の判定が即座にオフになり、
    ///   ハラハラと静かに落ちて消える余韻ある演出を行う。
    /// </summary>
    public class TsubakiBossController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Alpha_EliteHealth eliteHealth;
        [SerializeField] private Alpha_EliteEnemyAI eliteAI;
        [SerializeField] private SpriteRenderer visualRenderer;
        [SerializeField] private TsubakiSnowBlizzard snowBlizzard;

        [Header("Stall / Exhaustion Event")]
        [SerializeField] private float exhaustionHpRatio = 0.25f; // 残り25%で隙が発生
        [SerializeField] private float stallDuration = 2.2f;

        private bool hasTriggeredExhaustion = false;
        private bool hasTriggeredDefeat = false;

        private void Awake()
        {
            if (eliteHealth == null) eliteHealth = GetComponent<Alpha_EliteHealth>();
            if (eliteAI == null) eliteAI = GetComponent<Alpha_EliteEnemyAI>();
            if (visualRenderer == null) visualRenderer = GetComponentInChildren<SpriteRenderer>();
            if (snowBlizzard == null) snowBlizzard = GetComponentInChildren<TsubakiSnowBlizzard>();
        }

        private void Update()
        {
            if (hasTriggeredDefeat) return;

            // 決着検知
            if (eliteHealth != null && eliteHealth.isDead)
            {
                TriggerDefeatSequence();
                return;
            }

            // Phase 3 終盤の限界・よろめき演出検知（最終フェーズ時）
            if (!hasTriggeredExhaustion && eliteHealth != null && eliteHealth.CurrentPhaseIndex >= 2)
            {
                float currentHp = eliteHealth.currentHP;
                float maxHp = eliteHealth.HP;
                if (maxHp > 0 && (currentHp / maxHp) <= exhaustionHpRatio)
                {
                    TriggerExhaustionEvent();
                }
            }
        }

        /// <summary>
        /// 満身創痍のツバキが傷の限界で一瞬体勢を崩す隙
        /// </summary>
        public void TriggerExhaustionEvent()
        {
            if (hasTriggeredExhaustion) return;
            hasTriggeredExhaustion = true;

            StartCoroutine(ExhaustionSequenceRoutine());
        }

        private IEnumerator ExhaustionSequenceRoutine()
        {
            // 吹雪を一瞬途切れさせる
            if (snowBlizzard != null)
            {
                snowBlizzard.TriggerExhaustionStall(stallDuration);
            }

            // ツバキ本体が体勢を崩してよろめく（少し傾く・赤ノイズ点滅）
            if (visualRenderer != null)
            {
                Quaternion startRot = visualRenderer.transform.localRotation;
                visualRenderer.transform.localRotation = Quaternion.Euler(0, 0, 15f); // よろめき

                float elapsed = 0f;
                Color baseColor = visualRenderer.color;

                while (elapsed < stallDuration)
                {
                    elapsed += Time.deltaTime;
                    // 赤い傷の明滅
                    visualRenderer.color = (Random.value > 0.6f) ? new Color(1f, 0.4f, 0.5f, 1f) : baseColor;
                    yield return null;
                }

                visualRenderer.transform.localRotation = startRot;
                visualRenderer.color = baseColor;
            }
        }

        /// <summary>
        /// 決着演出（HP 0）：爆破ではなく膝をつき、吹雪と全弾がハラハラと静かに消滅
        /// </summary>
        public void TriggerDefeatSequence()
        {
            if (hasTriggeredDefeat) return;
            hasTriggeredDefeat = true;

            StartCoroutine(DefeatSequenceRoutine());
        }

        private IEnumerator DefeatSequenceRoutine()
        {
            // 1. ボスAIの停止
            if (eliteAI != null)
            {
                eliteAI.StopAllBehaviors();
            }

            // 2. フィールド上の全攻撃オブジェクトの当たり判定を即座にOFFにし、静かに消滅
            // a. 落ち椿トラップ
            TsubakiTrapCamellia[] traps = FindObjectsOfType<TsubakiTrapCamellia>();
            foreach (var trap in traps)
            {
                if (trap != null) trap.ForceStopAndVanish();
            }

            // b. 椿檻
            TsubakiCageController[] cages = FindObjectsOfType<TsubakiCageController>();
            foreach (var cage in cages)
            {
                if (cage != null) cage.ForceStopAndVanish();
            }

            // c. 椿吹雪
            if (snowBlizzard != null)
            {
                snowBlizzard.ForceStopAndVanish();
            }
            TsubakiPetalBullet[] petals = FindObjectsOfType<TsubakiPetalBullet>();
            foreach (var petal in petals)
            {
                if (petal != null) petal.ForceStopAndVanish();
            }

            // d. 細弾など全敵弾
            Bullet_Base[] bullets = FindObjectsOfType<Bullet_Base>();
            foreach (var bullet in bullets)
            {
                if (bullet != null && bullet.isEnemyBullet && bullet.gameObject.activeInHierarchy)
                {
                    Collider2D col = bullet.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;

                    var needle = bullet.GetComponent<TsubakiNeedleBullet>();
                    if (needle != null)
                    {
                        needle.ForceStopAndVanish();
                    }
                    else
                    {
                        Destroy(bullet.gameObject, 0.5f);
                    }
                }
            }

            // 3. ツバキ本体が静かに力を抜いて膝をつく（または下降）
            if (visualRenderer != null)
            {
                float duration = 2.0f;
                float elapsed = 0f;
                Vector3 startPos = visualRenderer.transform.localPosition;
                Vector3 kneePos = startPos + new Vector3(0, -0.4f, 0);

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    // 静かに沈み込む
                    visualRenderer.transform.localPosition = Vector3.Lerp(startPos, kneePos, t);
                    // 余韻を持ってフェード
                    visualRenderer.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.3f, t));
                    yield return null;
                }
            }
        }
    }
}
