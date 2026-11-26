using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// Phase 2: 『多層・零落の椿檻』の構成パーツ（八重椿の三角錐弾）
    /// - 通常弾（isCracked = false）：硬い壁。深紅。
    /// - ひび入り弱点弾（isCracked = true）：1層目のみに配置されるストーリー限定弱点弾。低HPでプレイヤーの攻撃で破壊可能。
    /// </summary>
    public class TsubakiYaeConeBullet : MonoBehaviour
    {
        [Header("Cone Settings")]
        [SerializeField] private bool isCracked = false;
        [SerializeField] private float maxHP = 60f;
        [SerializeField] private float contactDamage = 20f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite crackedSprite;
        [SerializeField] private GameObject breakVfxPrefab;

        private float currentHP;
        private bool isDestroyed = false;
        private Collider2D col;

        public bool IsCracked => isCracked;
        public bool IsDestroyed => isDestroyed;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            col = GetComponent<Collider2D>();
        }

        public void Setup(bool cracked, float hp = 60f, float damage = 20f)
        {
            isCracked = cracked;
            maxHP = hp;
            currentHP = maxHP;
            contactDamage = damage;
            isDestroyed = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = isCracked && crackedSprite != null ? crackedSprite : normalSprite;
            }

            if (col != null) col.enabled = true;

            if (isCracked)
            {
                StartCoroutine(GlitchBlinkRoutine());
            }
        }

        private IEnumerator GlitchBlinkRoutine()
        {
            // ひび入り弱点弾はかすかに明滅して視認性を高める
            while (!isDestroyed)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(1f, 0.85f, 0.9f, 1f);
                }
                yield return new WaitForSeconds(0.12f);
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }
                yield return new WaitForSeconds(0.25f);
            }
        }

        public void TakeDamage(float damage)
        {
            if (isDestroyed) return;

            // ひび入り弾のみ破壊可能（通常弾はストーリーモードでは破壊不可の防壁）
            if (!isCracked)
            {
                // ヒットフィードバック（白く一瞬フラッシュ）
                StartCoroutine(HitFlashRoutine());
                return;
            }

            currentHP -= damage;
            StartCoroutine(HitFlashRoutine());

            if (currentHP <= 0)
            {
                BreakAndDestroy();
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer != null)
            {
                Color prev = spriteRenderer.color;
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.05f);
                if (spriteRenderer != null) spriteRenderer.color = prev;
            }
        }

        /// <summary>
        /// 弱点弾が破壊されたときの爽快な砕け散り演出
        /// </summary>
        private void BreakAndDestroy()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            if (col != null) col.enabled = false;

            if (breakVfxPrefab != null)
            {
                Instantiate(breakVfxPrefab, transform.position, Quaternion.identity);
            }

            StartCoroutine(BreakShatterRoutine());
        }

        private IEnumerator BreakShatterRoutine()
        {
            float elapsed = 0f;
            float duration = 0.25f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, startScale * 1.4f, t);
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(1f, 0.3f, 0.4f, 1f - t);
                }
                yield return null;
            }

            gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isDestroyed) return;

            // プレイヤーの攻撃弾に当たった場合
            if (collision.CompareTag("Bullet"))
            {
                var bullet = collision.GetComponent<Bullet_Base>();
                if (bullet != null && !bullet.isEnemyBullet)
                {
                    TakeDamage(25f); // 弾からのダメージ
                }
            }
            // プレイヤー本体に接触した場合
            else if (collision.CompareTag("Player"))
            {
                var health = collision.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(contactDamage);
                }
            }
        }

        /// <summary>
        /// 外部（檻の消滅や決着）からの安全な消滅
        /// </summary>
        public void ForceStopAndVanish()
        {
            if (isDestroyed) return;
            isDestroyed = true;
            StopAllCoroutines();

            if (col != null) col.enabled = false;
            StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeOutRoutine()
        {
            float elapsed = 0f;
            float duration = 0.4f;
            Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                }
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
