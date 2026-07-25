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
        public float tickRate = 0.2f; // Seconds between ticks

        private BoxCollider2D boxCollider;
        private LineRenderer lineRenderer;
        
        private float currentThickness = 0f;
        private float expandTimer = 0f;
        private bool isExpanding = true;

        // プレイヤーごとの最終被弾時間（マルチプレイ対応も考慮してDictionary）
        private Dictionary<Collider2D, float> lastDamageTimes = new Dictionary<Collider2D, float>();

        void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            lineRenderer = GetComponent<LineRenderer>();
            
            boxCollider.isTrigger = true;
            // レーザーの始点はローカル0,0で、上（Y軸方向）に伸びる想定
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, new Vector3(0, length, 0));
            
            // 初回は太さ0
            SetThickness(0f);
        }

        void OnEnable()
        {
            expandTimer = 0f;
            isExpanding = true;
            SetThickness(0f);
            lastDamageTimes.Clear();
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
            List<Collider2D> keysToRemove = null;
            foreach(var key in lastDamageTimes.Keys)
            {
                if (key == null || !key.gameObject.activeInHierarchy)
                {
                    if (keysToRemove == null) keysToRemove = new List<Collider2D>();
                    keysToRemove.Add(key);
                }
            }
            if (keysToRemove != null)
            {
                foreach(var key in keysToRemove) lastDamageTimes.Remove(key);
            }
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
                if (!lastDamageTimes.ContainsKey(collision) || Time.time >= lastDamageTimes[collision] + tickRate)
                {
                    ApplyDamage(collision);
                }
            }
        }

        private void ApplyDamage(Collider2D collision)
        {
            var health = collision.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                lastDamageTimes[collision] = Time.time;
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
