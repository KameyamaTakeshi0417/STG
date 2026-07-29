using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Weapons
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(LineRenderer))]
    public class Alpha_LaserBeam : MonoBehaviour
    {
        [Header("Laser Settings")]
        public float length = 20f;
        public float targetThickness = 1f;
        public float expandDuration = 0.5f;
        
        [Header("Damage Settings")]
        public float damage = 1f;

        private BoxCollider2D boxCollider;
        private LineRenderer lineRenderer;
        
        private float currentThickness = 0f;
        private float expandTimer = 0f;
        private bool isExpanding = true;

        private HashSet<PlayerHealth> damagedThisTick = new HashSet<PlayerHealth>();

        void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            lineRenderer = GetComponent<LineRenderer>();
            
            boxCollider.isTrigger = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = 2;
        }

        void Start()
        {
            ApplyLaserSettings();
            SetThickness(0f); // 初回は太さ0
        }

        public void Setup(float newLength, float newThickness, float newExpandTime, float newDamage)
        {
            this.length = newLength;
            this.targetThickness = newThickness;
            this.expandDuration = newExpandTime;
            this.damage = newDamage;

            if (lineRenderer != null)
            {
                ApplyLaserSettings();
            }
        }

        private void ApplyLaserSettings()
        {
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, new Vector3(0, length, 0));
        }

        void OnEnable()
        {
            expandTimer = 0f;
            isExpanding = true;
            SetThickness(0f);
            damagedThisTick.Clear();

            if (Alpha.Core.Utils.Alpha_TickManager.Instance != null)
            {
                Alpha.Core.Utils.Alpha_TickManager.Instance.OnTick += HandleTick;
            }
        }

        void OnDisable()
        {
            if (Alpha.Core.Utils.Alpha_TickManager.Instance != null)
            {
                Alpha.Core.Utils.Alpha_TickManager.Instance.OnTick -= HandleTick;
            }
        }

        private void HandleTick()
        {
            damagedThisTick.Clear();
        }

        void Update()
        {
            if (isExpanding)
            {
                expandTimer += Time.deltaTime;
                float t = Mathf.Clamp01(expandTimer / expandDuration);
                
                // アニメーションカーブなどを使いたい場合はここで調整
                currentThickness = Mathf.Lerp(0f, targetThickness, t);
                SetThickness(currentThickness);

                if (t >= 1f)
                {
                    isExpanding = false;
                }
            }
            
            // Cleanup invalid entries
            damagedThisTick.RemoveWhere(h => h == null || !h.gameObject.activeInHierarchy);
        }

        private void SetThickness(float thickness)
        {
            lineRenderer.startWidth = thickness;
            lineRenderer.endWidth = thickness;

            // BoxCollider2DのサイズとオフセットをLineRendererに合わせる
            // レーザーは Y軸(上)方向 に伸びると仮定
            boxCollider.size = new Vector2(thickness, length);
            boxCollider.offset = new Vector2(0, length / 2f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                ApplyDamage(collision);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                var health = collision.GetComponentInParent<PlayerHealth>();
                if (health != null && !damagedThisTick.Contains(health))
                {
                    health.TakeDamage(damage);
                    damagedThisTick.Add(health);
                }
            }
        }

        private void ApplyDamage(Collider2D collision)
        {
            var health = collision.GetComponentInParent<PlayerHealth>();
            if (health != null && !damagedThisTick.Contains(health))
            {
                health.TakeDamage(damage);
                damagedThisTick.Add(health);
            }
        }
        
        // 外部から長さを変える場合
        public void SetLength(float newLength)
        {
            length = newLength;
            lineRenderer.SetPosition(1, new Vector3(0, length, 0));
            SetThickness(currentThickness); // コライダー再計算
        }
    }
}
