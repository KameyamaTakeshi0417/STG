using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Environment
{
    /// <summary>
    /// 木などの障害物にアタッチし、トリガー判定エリアにプレイヤーが入った際に
    /// 前後表示（SortingOrder）を切り替え、半透明フェードを行うコンポーネント。
    /// 
    /// 【特徴】
    /// - Rigidbody2D (Kinematic) を自動追加・設定するため、静的コライダーによるトリガー不発を防ぎます。
    /// - 複数コライダーによる Enter/Exit のカウントズレを防ぐため、HashSetで接触中のコライダーを厳密に管理します。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TreeDepthTrigger_Alpha : MonoBehaviour
    {
        [Header("Sorting Order Settings")]
        [Tooltip("普段（プレイヤーが外にいる時）のSortingOrder。プレイヤーより奥なら低く設定")]
        [SerializeField] private int defaultSortingOrder = -10;

        [Tooltip("プレイヤーが当たり判定内にいる時のSortingOrder。プレイヤー(通常0前後)より手前にするために大きく設定")]
        [SerializeField] private int foregroundSortingOrder = 10;

        [Header("Target Detection")]
        [Tooltip("プレイヤー判定に使用するTag（通常は 'Player'）")]
        [SerializeField] private string playerTag = "Player";

        [Header("Transparency Effect")]
        [Tooltip("プレイヤーが隠れた時に木を少し半透明にするかどうか")]
        [SerializeField] private bool enableFadeWhenBehind = true;

        [Tooltip("隠れた時の木の不透明度（0: 完全透明 〜 1: 通常）")]
        [Range(0.1f, 1f)]
        [SerializeField] private float fadeAlpha = 0.5f;

        [Tooltip("フェードの速度")]
        [SerializeField] private float fadeSpeed = 8f;

        [Header("Debug")]
        [Tooltip("コンソールにトリガー検出ログを出力するかどうか")]
        [SerializeField] private bool showDebugLog = true;

        private SpriteRenderer spriteRenderer;
        private Color baseColor;
        private float currentAlpha = 1f;
        private float targetAlpha = 1f;

        // トリガー内に現在滞在しているプレイヤー関連コライダーの管理リスト
        private readonly HashSet<Collider2D> activePlayerColliders = new HashSet<Collider2D>();

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseColor = spriteRenderer.color;

            // 初期状態は奥に設定
            spriteRenderer.sortingOrder = defaultSortingOrder;
            currentAlpha = 1f;
            targetAlpha = 1f;

            // Collider2D の IsTrigger 確認
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }

            // 【重要】Unity 2Dでトリガーを確実に発火させるため、Kinematic Rigidbody2D を設定
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true; // Kinematicでも全トリガーイベントを拾う設定
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayer(other))
            {
                activePlayerColliders.Add(other);
                UpdateDepthState();
                if (showDebugLog)
                {
                    Debug.Log($"[TreeDepth] プレイヤー進入検知: {other.name} (接触数: {activePlayerColliders.Count})", this);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (activePlayerColliders.Contains(other))
            {
                activePlayerColliders.Remove(other);
                UpdateDepthState();
                if (showDebugLog)
                {
                    Debug.Log($"[TreeDepth] プレイヤー退出検知: {other.name} (接触数: {activePlayerColliders.Count})", this);
                }
            }
        }

        private bool IsPlayer(Collider2D other)
        {
            if (other == null) return false;

            // 1. Tagチェック
            if (other.CompareTag(playerTag)) return true;
            if (other.transform.root.CompareTag(playerTag)) return true;

            // 2. コンポーネントチェック
            if (other.GetComponentInParent<global::Player_Control_Alpha>() != null) return true;

            return false;
        }

        private void UpdateDepthState()
        {
            // 無効になったコライダーを掃除
            activePlayerColliders.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

            if (activePlayerColliders.Count > 0)
            {
                // プレイヤーが潜り込んだ：木を手前にし、半透明化
                spriteRenderer.sortingOrder = foregroundSortingOrder;
                targetAlpha = enableFadeWhenBehind ? fadeAlpha : 1f;
            }
            else
            {
                // プレイヤーが外に出た：木を奥に戻し、不透明化
                spriteRenderer.sortingOrder = defaultSortingOrder;
                targetAlpha = 1f;
            }
        }

        private void Update()
        {
            // アルファ値のスムーズなフェード処理
            if (enableFadeWhenBehind && !Mathf.Approximately(currentAlpha, targetAlpha))
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
                Color c = baseColor;
                c.a = currentAlpha;
                spriteRenderer.color = c;
            }
        }

        private void OnDisable()
        {
            activePlayerColliders.Clear();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = defaultSortingOrder;
                Color c = baseColor;
                c.a = 1f;
                spriteRenderer.color = c;
            }
        }
    }
}
