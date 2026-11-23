using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Visual
{
    [ExecuteAlways]
    public class BotanicalCharacterVisual_Alpha : MonoBehaviour
    {
        public enum VisualPreset
        {
            Player,
            Enemy,
            Boss,
            Custom
        }

        [Header("Preset & Material Settings")]
        [SerializeField] private VisualPreset preset = VisualPreset.Enemy;
        [SerializeField] private Material customMaterial;
        [SerializeField] private bool applyToChildren = true;
        [SerializeField] private List<SpriteRenderer> targetRenderers = new List<SpriteRenderer>();

        [Header("Lighting Overrides (Custom only or dynamic)")]
        [SerializeField] private bool overridePresetColors = false;
        [SerializeField] private Color topColor = new Color(1.05f, 1.04f, 1.02f, 1f);
        [SerializeField] private Color bottomColor = new Color(0.92f, 0.94f, 0.96f, 1f);
        [SerializeField] private Color rimColor = new Color(1.0f, 0.96f, 0.88f, 1f);
        [Range(0f, 5f)] [SerializeField] private float rimIntensity = 0.45f;
        [Range(0.5f, 4f)] [SerializeField] private float rimWidth = 1.2f;
        [Range(0f, 1f)] [SerializeField] private float komorebiStrength = 0.05f;

        // Cached shader property IDs
        private static readonly int TopColorId = Shader.PropertyToID("_TopColor");
        private static readonly int BottomColorId = Shader.PropertyToID("_BottomColor");
        private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
        private static readonly int RimIntensityId = Shader.PropertyToID("_RimIntensity");
        private static readonly int RimWidthId = Shader.PropertyToID("_RimWidth");
        private static readonly int KomorebiStrengthId = Shader.PropertyToID("_KomorebiStrength");
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
        private static readonly int DissolveEdgeColorId = Shader.PropertyToID("_DissolveEdgeColor");

        private MaterialPropertyBlock propertyBlock;
        private Coroutine flashCoroutine;
        private Coroutine dissolveCoroutine;
        private float currentFlashAmount = 0f;
        private float currentDissolveAmount = 0f;
        private bool isDissolving = false;

        public VisualPreset Preset
        {
            get => preset;
            set
            {
                preset = value;
                ApplyMaterialAndPreset();
            }
        }

        public bool IsDissolving => isDissolving;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            InitializeRenderers();
        }

        private void Start()
        {
            ApplyMaterialAndPreset();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            InitializeRenderers();
            ApplyPresetProperties();
        }
#endif

        public void InitializeRenderers()
        {
            if (targetRenderers == null)
            {
                targetRenderers = new List<SpriteRenderer>();
            }

            if (targetRenderers.Count == 0)
            {
                if (applyToChildren)
                {
                    var allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
                    for (int i = 0; i < allRenderers.Length; i++)
                    {
                        var sr = allRenderers[i];
                        if (sr == null) continue;

                        string goName = sr.gameObject.name.ToLower();
                        // 影オブジェクトや当たり判定マーカーを除外
                        if (goName.Contains("shadow") || goName.Contains("circle") || goName.Contains("hitbox") || goName.Contains("marker"))
                        {
                            continue;
                        }

                        // 半透明の真っ黒なスプライト（影）を除外
                        if (sr.color.r < 0.05f && sr.color.g < 0.05f && sr.color.b < 0.05f && sr.color.a < 0.95f)
                        {
                            continue;
                        }

                        targetRenderers.Add(sr);
                    }
                }
                else
                {
                    var sr = GetComponent<SpriteRenderer>();
                    if (sr != null) targetRenderers.Add(sr);
                }
            }
        }

        public void ApplyMaterialAndPreset()
        {
            if (targetRenderers == null || targetRenderers.Count == 0)
            {
                InitializeRenderers();
            }

            Material baseMat = GetBaseMaterialForPreset();
            if (baseMat != null)
            {
                for (int i = 0; i < targetRenderers.Count; i++)
                {
                    if (targetRenderers[i] != null)
                    {
                        targetRenderers[i].sharedMaterial = baseMat;
                    }
                }
            }

            ApplyPresetProperties();
        }

        private Material GetBaseMaterialForPreset()
        {
            if (customMaterial != null) return customMaterial;

            string matPath = preset switch
            {
                VisualPreset.Player => "Materials/M_PlayerCharacter_Alpha",
                VisualPreset.Boss => "Materials/M_BossCharacter_Alpha",
                _ => "Materials/M_EnemyCharacter_Alpha"
            };

            Material mat = Resources.Load<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Custom/BotanicalCharacter2D");
                if (shader != null)
                {
                    mat = new Material(shader);
                }
            }
            return mat;
        }

        private void ApplyPresetProperties()
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

            if (overridePresetColors)
            {
                propertyBlock.SetColor(TopColorId, topColor);
                propertyBlock.SetColor(BottomColorId, bottomColor);
                propertyBlock.SetColor(RimColorId, rimColor);
                propertyBlock.SetFloat(RimIntensityId, rimIntensity);
                propertyBlock.SetFloat(RimWidthId, rimWidth);
                propertyBlock.SetFloat(KomorebiStrengthId, komorebiStrength);
            }

            propertyBlock.SetFloat(FlashAmountId, currentFlashAmount);
            propertyBlock.SetFloat(DissolveAmountId, currentDissolveAmount);

            UpdatePropertyBlock();
        }

        private void UpdatePropertyBlock()
        {
            if (targetRenderers == null) return;
            for (int i = 0; i < targetRenderers.Count; i++)
            {
                if (targetRenderers[i] != null)
                {
                    targetRenderers[i].SetPropertyBlock(propertyBlock);
                }
            }
        }

        /// <summary>
        /// 被弾時のヒットフラッシュ演出を発火
        /// </summary>
        public void TriggerHitFlash(float duration = 0.08f, float peakAmount = 0.85f, Color? flashColor = null)
        {
            if (!Application.isPlaying || isDissolving) return;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            if (gameObject.activeInHierarchy)
            {
                flashCoroutine = StartCoroutine(HitFlashRoutine(duration, peakAmount, flashColor));
            }
        }

        private IEnumerator HitFlashRoutine(float duration, float peakAmount, Color? flashColor)
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

            if (flashColor.HasValue)
            {
                propertyBlock.SetColor(FlashColorId, flashColor.Value);
            }
            else
            {
                propertyBlock.SetColor(FlashColorId, Color.white);
            }

            float elapsed = 0f;
            currentFlashAmount = peakAmount;
            propertyBlock.SetFloat(FlashAmountId, currentFlashAmount);
            UpdatePropertyBlock();

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                currentFlashAmount = Mathf.Lerp(peakAmount, 0f, t);
                propertyBlock.SetFloat(FlashAmountId, currentFlashAmount);
                UpdatePropertyBlock();
                yield return null;
            }

            currentFlashAmount = 0f;
            propertyBlock.SetFloat(FlashAmountId, 0f);
            UpdatePropertyBlock();
            flashCoroutine = null;
        }

        /// <summary>
        /// 撃破時の花弁ディゾルブ演出を発火
        /// </summary>
        public void TriggerDissolve(float duration = 0.35f, Action onComplete = null)
        {
            if (!Application.isPlaying)
            {
                onComplete?.Invoke();
                return;
            }

            if (isDissolving) return;
            isDissolving = true;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                currentFlashAmount = 0f;
                if (propertyBlock != null)
                {
                    propertyBlock.SetFloat(FlashAmountId, 0f);
                }
            }

            if (dissolveCoroutine != null)
            {
                StopCoroutine(dissolveCoroutine);
            }
            if (gameObject.activeInHierarchy)
            {
                dissolveCoroutine = StartCoroutine(DissolveRoutine(duration, onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private IEnumerator DissolveRoutine(float duration, Action onComplete)
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                currentDissolveAmount = t;
                propertyBlock.SetFloat(DissolveAmountId, currentDissolveAmount);
                UpdatePropertyBlock();
                yield return null;
            }

            currentDissolveAmount = 1f;
            propertyBlock.SetFloat(DissolveAmountId, 1f);
            UpdatePropertyBlock();

            onComplete?.Invoke();
            dissolveCoroutine = null;
        }

        /// <summary>
        /// リムライトの色と強度を動的に変更
        /// </summary>
        public void SetRimLight(Color color, float intensity, float width = -1f)
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor(RimColorId, color);
            propertyBlock.SetFloat(RimIntensityId, intensity);
            if (width > 0f)
            {
                propertyBlock.SetFloat(RimWidthId, width);
            }
            UpdatePropertyBlock();
        }
    }
}
