using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Alpha.Result
{
    public class ResultMatrixUI_Alpha : MonoBehaviour
    {
        private static ResultMatrixUI_Alpha _instance;
        public static ResultMatrixUI_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ResultMatrixUI_Alpha>();
                    if (_instance == null)
                    {
                        var go = new GameObject("ResultMatrixUI_Alpha");
                        _instance = go.AddComponent<ResultMatrixUI_Alpha>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Audio Settings")]
        [Tooltip("パネル破壊音（未設定時は Resources/SE/SE_Result_GlassBreak を自動ロード）")]
        public AudioClip glassBreakClip;
        [Tooltip("ライン完成音（未設定時は Resources/SE/SE_Result_LineComplete を自動ロード）")]
        public AudioClip lineCompleteClip;
        [Range(0f, 1f)] public float seVolume = 0.85f;

        [Header("Font Settings")]
        [Tooltip("リザルト画面で使用するフォント（未指定時はSoukouMincho SDFを自動ロード）")]
        public TMP_FontAsset customFontAsset;

        [Header("UI Root References")]
        public Canvas resultCanvas;
        public GameObject mainContainer;
        public Transform gridContainer;
        public TextMeshProUGUI stageTitleText;
        public TextMeshProUGUI totalScoreText;
        public TextMeshProUGUI rankGradeText;
        public TextMeshProUGUI linesCountText;
        public Button nextStageButton;

        private Action onResultFinished;
        private List<GameObject> panelUIObjects = new List<GameObject>();
        private bool isAnimating = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            LoadFontAssetIfNeeded();

            if (glassBreakClip == null) glassBreakClip = Resources.Load<AudioClip>("SE/SE_Result_GlassBreak");
            if (lineCompleteClip == null) lineCompleteClip = Resources.Load<AudioClip>("SE/SE_Result_LineComplete");

            EnsureUIHierarchy();
            ApplyFontToAllUI();
            if (mainContainer != null) mainContainer.SetActive(false);
        }

        public void LoadFontAssetIfNeeded()
        {
#if UNITY_EDITOR
            if (customFontAsset == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("SoukouMincho SDF t:TMP_FontAsset");
                if (guids.Length > 0)
                {
                    string p = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    customFontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
                    if (customFontAsset != null)
                    {
                        Debug.Log("[ResultMatrixUI] Auto-loaded SoukouMincho SDF from: " + p);
                    }
                }
            }
#endif
            if (customFontAsset == null)
            {
                customFontAsset = Resources.Load<TMP_FontAsset>("Fonts/SoukouMincho SDF");
            }
            if (customFontAsset == null)
            {
                customFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/SoukouMincho SDF");
            }
        }

        public void ApplyFontToAllUI()
        {
            LoadFontAssetIfNeeded();
            if (customFontAsset == null) return;

            Transform targetRoot = (mainContainer != null) ? mainContainer.transform : transform;
            var allTexts = targetRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (t != null) t.font = customFontAsset;
            }
        }

        private void EnsureUIHierarchy()
        {
            if (resultCanvas != null && mainContainer != null) return;

            // Canvas
            var existingCanvas = GetComponentInChildren<Canvas>(true);
            if (existingCanvas != null)
            {
                resultCanvas = existingCanvas;
            }
            else
            {
                var canvasObj = new GameObject("ResultMatrixCanvas");
                canvasObj.transform.SetParent(transform);
                resultCanvas = canvasObj.AddComponent<Canvas>();
                resultCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                resultCanvas.sortingOrder = 999;
                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Main Container (暗転背景: 深森墨黒・すりガラス調)
            var bgObj = new GameObject("MainContainer");
            bgObj.transform.SetParent(resultCanvas.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.040f, 0.055f, 0.048f, 0.96f);
            mainContainer = bgObj;

            // 全体四隅に真鍮幾何学L字コーナー
            AddCornerAccents(bgRect, new Color(0.55f, 0.46f, 0.32f, 0.65f), 24f, 1.5f);

            // Header Title (装甲明朝 + アンティーク真鍮)
            var titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(mainContainer.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -60);
            titleRect.sizeDelta = new Vector2(900, 80);
            stageTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) stageTitleText.font = customFontAsset;
            stageTitleText.text = "<size=65%><color=#8c7552>HERBARIUM MATRIX // </color></size>神格条項評価";
            stageTitleText.fontSize = 36;
            stageTitleText.alignment = TextAlignmentOptions.Center;
            stageTitleText.color = new Color(0.80f, 0.68f, 0.44f, 1.0f);

            // 区切り線（真鍮ゴールド極細線）
            var sepObj = new GameObject("TitleLine");
            sepObj.transform.SetParent(mainContainer.transform, false);
            var sepRect = sepObj.AddComponent<RectTransform>();
            sepRect.anchorMin = new Vector2(0.5f, 1f);
            sepRect.anchorMax = new Vector2(0.5f, 1f);
            sepRect.anchoredPosition = new Vector2(0, -105);
            sepRect.sizeDelta = new Vector2(650, 1.5f);
            var sepImg = sepObj.AddComponent<Image>();
            sepImg.color = new Color(0.55f, 0.46f, 0.32f, 0.65f);

            // Grid Container (中央3x3 標本プレート盤面)
            var gridObj = new GameObject("GridContainer");
            gridObj.transform.SetParent(mainContainer.transform, false);
            var gRect = gridObj.AddComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0.5f, 0.5f);
            gRect.anchorMax = new Vector2(0.5f, 0.5f);
            gRect.anchoredPosition = new Vector2(-150, -20);
            gRect.sizeDelta = new Vector2(620, 620);
            gridContainer = gridObj.transform;

            // 盤面四隅の真鍮コーナー
            AddCornerAccents(gRect, new Color(0.80f, 0.68f, 0.44f, 0.8f), 18f, 1.5f);

            // Side Panel (右側: 標本鑑定枠)
            var sideObj = new GameObject("SidePanel");
            sideObj.transform.SetParent(mainContainer.transform, false);
            var sRect = sideObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 0.5f);
            sRect.anchorMax = new Vector2(0.5f, 0.5f);
            sRect.anchoredPosition = new Vector2(400, -20);
            sRect.sizeDelta = new Vector2(400, 620);

            var sBg = sideObj.AddComponent<Image>();
            sBg.color = new Color(0.048f, 0.070f, 0.060f, 0.95f);
            AddCornerAccents(sRect, new Color(0.55f, 0.46f, 0.32f, 0.65f), 16f, 1.5f);

            // Total Score Text (真鍮ゴールド)
            var scoreObj = new GameObject("TotalScoreText");
            scoreObj.transform.SetParent(sideObj.transform, false);
            var scRect = scoreObj.AddComponent<RectTransform>();
            scRect.anchoredPosition = new Vector2(0, 180);
            scRect.sizeDelta = new Vector2(380, 80);
            totalScoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) totalScoreText.font = customFontAsset;
            totalScoreText.text = "SCORE: 0";
            totalScoreText.fontSize = 36;
            totalScoreText.alignment = TextAlignmentOptions.Center;
            totalScoreText.color = new Color(0.80f, 0.68f, 0.44f, 1.0f);

            // Lines Text (新緑若葉色)
            var linesObj = new GameObject("LinesText");
            linesObj.transform.SetParent(sideObj.transform, false);
            var lRect = linesObj.AddComponent<RectTransform>();
            lRect.anchoredPosition = new Vector2(0, 100);
            lRect.sizeDelta = new Vector2(380, 60);
            linesCountText = linesObj.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) linesCountText.font = customFontAsset;
            linesCountText.text = "LINES: 0";
            linesCountText.fontSize = 26;
            linesCountText.alignment = TextAlignmentOptions.Center;
            linesCountText.color = new Color(0.48f, 0.78f, 0.56f, 1.0f);

            // Rank Stamp Text (椿の深紅)
            var rankObj = new GameObject("RankGradeText");
            rankObj.transform.SetParent(sideObj.transform, false);
            var rRect = rankObj.AddComponent<RectTransform>();
            rRect.anchoredPosition = new Vector2(0, -20);
            rRect.sizeDelta = new Vector2(250, 150);
            rankGradeText = rankObj.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) rankGradeText.font = customFontAsset;
            rankGradeText.text = "-";
            rankGradeText.fontSize = 100;
            rankGradeText.fontStyle = FontStyles.Bold;
            rankGradeText.alignment = TextAlignmentOptions.Center;
            rankGradeText.color = new Color(0.72f, 0.22f, 0.24f, 1.0f);

            // Next Stage Button (深苔色プレート + 真鍮枠)
            var btnObj = new GameObject("NextStageButton");
            btnObj.transform.SetParent(sideObj.transform, false);
            var bRect = btnObj.AddComponent<RectTransform>();
            bRect.anchoredPosition = new Vector2(0, -180);
            bRect.sizeDelta = new Vector2(280, 64);
            var bImg = btnObj.AddComponent<Image>();
            bImg.color = new Color(0.14f, 0.21f, 0.17f, 0.95f);
            AddCornerAccents(bRect, new Color(0.80f, 0.68f, 0.44f, 0.9f), 10f, 1.5f);
            nextStageButton = btnObj.AddComponent<Button>();
            
            var btnTextObj = new GameObject("BtnText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            var btRect = btnTextObj.AddComponent<RectTransform>();
            btRect.sizeDelta = bRect.sizeDelta;
            var btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) btnTmp.font = customFontAsset;
            btnTmp.text = "次期条項へ進む ▶";
            btnTmp.fontSize = 24;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = new Color(0.93f, 0.91f, 0.86f, 1.0f);

            nextStageButton.onClick.AddListener(OnNextStageClicked);
        }

        /// <summary>
        /// ステージボス撃破時にリザルト画面を開き、新しく達成されたマスを追加破壊する
        /// </summary>
        public void ShowResultSequence(Action onComplete)
        {
            onResultFinished = onComplete;
            EnsureUIHierarchy();

            var mgr = ResultMatrixManager_Alpha.Instance;
            var stats = RunStatsTracker_Alpha.Instance;

            // 1ゲーム通算の実績に基づいて新たに達成されたマスを判定
            mgr.EvaluateAllPanels(stats);

            mainContainer.SetActive(true);
            nextStageButton.gameObject.SetActive(false);
            rankGradeText.text = "";

            // 過去に割れていたマスは最初から割れた状態でグリッドを描画
            BuildGridUI(mgr);

            StartCoroutine(PlayBreakSequenceRoutine(mgr));
        }

        private void BuildGridUI(ResultMatrixManager_Alpha mgr)
        {
            foreach (var obj in panelUIObjects)
            {
                if (obj != null) Destroy(obj);
            }
            panelUIObjects.Clear();

            int size = mgr.gridSize;
            float totalWidth = 620f;
            float spacing = 12f;
            float cellWidth = (totalWidth - spacing * (size - 1)) / size;

            for (int i = 0; i < mgr.panels.Length; i++)
            {
                var panelState = mgr.panels[i];
                int r = panelState.row;
                int c = panelState.col;

                var cellObj = new GameObject($"Cell_{i}");
                cellObj.transform.SetParent(gridContainer, false);
                var rt = cellObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);

                float posX = c * (cellWidth + spacing) + cellWidth * 0.5f;
                float posY = -(r * (cellWidth + spacing) + cellWidth * 0.5f);
                rt.anchoredPosition = new Vector2(posX, posY);
                rt.sizeDelta = new Vector2(cellWidth, cellWidth);

                var bg = cellObj.AddComponent<Image>();
                // 過去ステージで既に割れていた場合は割れた色、そうでなければ未破壊色
                bg.color = GetDifficultyColor(panelState.mission.difficulty, panelState.alreadyBroken);
                AddCornerAccents(rt, new Color(0.55f, 0.46f, 0.32f, 0.4f), 10f, 1f);

                // テキスト
                var textObj = new GameObject("Text");
                textObj.transform.SetParent(cellObj.transform, false);
                var tRect = textObj.AddComponent<RectTransform>();
                tRect.sizeDelta = new Vector2(cellWidth - 16, cellWidth - 16);
                var tmp = textObj.AddComponent<TextMeshProUGUI>();
                if (customFontAsset != null) tmp.font = customFontAsset;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = (size == 3) ? 16 : 12;
                tmp.enableWordWrapping = true;
                tmp.color = new Color(0.93f, 0.91f, 0.86f, 1.0f);
                tmp.text = $"<b>{panelState.mission.title}</b>\n<size=75%><color=#adada3>{panelState.mission.description}</color></size>\n<color=#ccad70>+{panelState.mission.scorePoints}pt</color>";

                // 過去ステージで既に割れていた場合は最初から「[ 条項承認 - RATIFIED ]」スタンプをつける
                if (panelState.alreadyBroken)
                {
                    AttachClearStamp(cellObj);
                }

                panelUIObjects.Add(cellObj);
            }

            int prevLines = mgr.completedLinesCount - mgr.newlyCompletedLinesCount;
            linesCountText.text = $"LINES: {prevLines}";
            int prevScore = mgr.totalAccumulatedScore;
            totalScoreText.text = $"SCORE: {prevScore}";

            ApplyFontToAllUI();
        }

        private Color GetDifficultyColor(int diff, bool isBroken)
        {
            if (isBroken)
            {
                // 達成時: 植物標本が澄んだ光とともに結晶化・承認された深色
                switch (diff)
                {
                    case 1: return new Color(0.14f, 0.28f, 0.20f, 0.96f); // 蓮・翡翠苔
                    case 2: return new Color(0.16f, 0.24f, 0.32f, 0.96f); // 深藍・真鍮
                    case 3: return new Color(0.32f, 0.16f, 0.18f, 0.96f); // 椿・深紅
                    default: return new Color(0.35f, 0.28f, 0.14f, 0.98f); // 最終条項・黄金
                }
            }
            else
            {
                // 未達成時: 深森の墨色・すりガラスプレート
                switch (diff)
                {
                    case 1: return new Color(0.06f, 0.09f, 0.08f, 0.90f);
                    case 2: return new Color(0.06f, 0.08f, 0.10f, 0.90f);
                    case 3: return new Color(0.09f, 0.07f, 0.06f, 0.90f);
                    default: return new Color(0.08f, 0.07f, 0.09f, 0.90f);
                }
            }
        }

        private IEnumerator PlayBreakSequenceRoutine(ResultMatrixManager_Alpha mgr)
        {
            isAnimating = true;
            yield return new WaitForSecondsRealtime(0.4f);

            int centerIdx = (mgr.gridSize / 2) * mgr.gridSize + (mgr.gridSize / 2);

            // 1. 今回新しく達成されたマス（justBroken == true）を順次破壊演出
            for (int i = 0; i < mgr.panels.Length; i++)
            {
                if (i == centerIdx) continue;

                var panel = mgr.panels[i];
                if (panel.justBroken)
                {
                    yield return StartCoroutine(BreakPanelAnimation(i, panel));
                    yield return new WaitForSecondsRealtime(0.18f);
                }
            }

            // 2. 中央マス（ゲームクリア達成時）
            if (centerIdx >= 0 && centerIdx < mgr.panels.Length && mgr.panels[centerIdx].justBroken)
            {
                yield return new WaitForSecondsRealtime(0.3f);
                yield return StartCoroutine(BreakPanelAnimation(centerIdx, mgr.panels[centerIdx], true));
            }

            // 3. 新規ライン完成演出
            yield return new WaitForSecondsRealtime(0.4f);
            if (mgr.newlyCompletedLinesCount > 0)
            {
                PlaySE(lineCompleteClip, 1.0f);
                linesCountText.text = $"LINES: {mgr.completedLinesCount} (+{mgr.newlyCompletedLinesCount} NEW!!)";
                linesCountText.transform.DOPunchScale(Vector3.one * 0.35f, 0.4f).SetUpdate(true);
            }
            else
            {
                linesCountText.text = $"LINES: {mgr.completedLinesCount}";
            }

            // 4. トータルスコア更新 & ランク捺印
            yield return new WaitForSecondsRealtime(0.5f);
            totalScoreText.text = $"SCORE: {mgr.totalAccumulatedScore}";
            totalScoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetUpdate(true);

            rankGradeText.text = mgr.rankGrade;
            rankGradeText.transform.localScale = Vector3.one * 2.5f;
            rankGradeText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBounce).SetUpdate(true);
            PlaySE(glassBreakClip, 1.1f);

            // 5. 評価状態を確定（次ステージで二重演出しないため）
            mgr.FinalizeEvaluation();

            // 6. 次へ進むボタン
            yield return new WaitForSecondsRealtime(0.6f);
            nextStageButton.gameObject.SetActive(true);
            nextStageButton.transform.localScale = Vector3.zero;
            nextStageButton.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);

            isAnimating = false;
        }

        private IEnumerator BreakPanelAnimation(int index, ResultPanelState panel, bool isCenter = false)
        {
            if (index < 0 || index >= panelUIObjects.Count) yield break;

            GameObject cellObj = panelUIObjects[index];
            var img = cellObj.GetComponent<Image>();

            // パネル破壊音
            PlaySE(glassBreakClip, isCenter ? 0.95f : UnityEngine.Random.Range(0.98f, 1.15f));
            cellObj.transform.DOPunchScale(Vector3.one * (isCenter ? 0.4f : 0.25f), 0.2f).SetUpdate(true);

            if (img != null)
            {
                img.color = GetDifficultyColor(panel.mission.difficulty, true);
            }

            AttachClearStamp(cellObj);
            yield return null;
        }

        private void AttachClearStamp(GameObject cellObj)
        {
            if (cellObj == null || cellObj.transform.Find("ClearStamp") != null) return;

            var stampObj = new GameObject("ClearStamp");
            stampObj.transform.SetParent(cellObj.transform, false);
            var sRect = stampObj.AddComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0, 36);
            sRect.sizeDelta = new Vector2(160, 26);
            var sBg = stampObj.AddComponent<Image>();
            sBg.color = new Color(0.06f, 0.14f, 0.10f, 0.90f);
            AddCornerAccents(sRect, new Color(0.80f, 0.68f, 0.44f, 0.9f), 6f, 1f);

            // TextMeshProUGUI は Image と同じ GameObject に追加できないため子オブジェクトに分離
            var textObj = new GameObject("StampText");
            textObj.transform.SetParent(stampObj.transform, false);
            var tRect = textObj.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            var stampTmp = textObj.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) stampTmp.font = customFontAsset;
            stampTmp.text = "[ 条項承認 - RATIFIED ]";
            stampTmp.fontSize = 11;
            stampTmp.alignment = TextAlignmentOptions.Center;
            stampTmp.color = new Color(0.32f, 0.72f, 0.53f, 1.0f);
        }

        private void PlaySE(AudioClip clip, float pitch = 1.0f)
        {
            if (clip != null && Alpha.Audio.SoundManager_Alpha.Instance != null)
            {
                Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(clip, seVolume, pitch);
            }
        }

        private void OnNextStageClicked()
        {
            if (isAnimating) return;

            if (mainContainer != null)
            {
                mainContainer.SetActive(false);
            }

            onResultFinished?.Invoke();
        }

        private static void AddCornerAccents(RectTransform targetParent, Color color, float size = 14f, float thickness = 1.5f)
        {
            if (targetParent == null) return;

            for (int i = targetParent.childCount - 1; i >= 0; i--)
            {
                var c = targetParent.GetChild(i);
                if (c.name.StartsWith("Corner_"))
                {
                    Destroy(c.gameObject);
                }
            }

            CreateSingleCorner(targetParent, "Corner_TL", new Vector2(0f, 1f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_TR", new Vector2(1f, 1f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_BL", new Vector2(0f, 0f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_BR", new Vector2(1f, 0f), color, size, thickness);
        }

        private static void CreateSingleCorner(RectTransform parent, string name, Vector2 anchor, Color color, float size, float thickness)
        {
            GameObject cornerGO = new GameObject(name, typeof(RectTransform));
            cornerGO.transform.SetParent(parent, false);
            RectTransform crt = cornerGO.GetComponent<RectTransform>();
            crt.anchorMin = anchor;
            crt.anchorMax = anchor;
            crt.pivot = anchor;
            crt.sizeDelta = new Vector2(size, size);
            crt.anchoredPosition = Vector2.zero;

            GameObject hGO = new GameObject("H", typeof(RectTransform));
            hGO.transform.SetParent(cornerGO.transform, false);
            RectTransform hrt = hGO.GetComponent<RectTransform>();
            hrt.anchorMin = anchor;
            hrt.anchorMax = anchor;
            hrt.pivot = anchor;
            hrt.sizeDelta = new Vector2(size, thickness);
            hrt.anchoredPosition = Vector2.zero;
            Image hImg = hGO.AddComponent<Image>();
            hImg.color = color;
            hImg.raycastTarget = false;

            GameObject vGO = new GameObject("V", typeof(RectTransform));
            vGO.transform.SetParent(cornerGO.transform, false);
            RectTransform vrt = vGO.GetComponent<RectTransform>();
            vrt.anchorMin = anchor;
            vrt.anchorMax = anchor;
            vrt.pivot = anchor;
            vrt.sizeDelta = new Vector2(thickness, size);
            vrt.anchoredPosition = Vector2.zero;
            Image vImg = vGO.AddComponent<Image>();
            vImg.color = color;
            vImg.raycastTarget = false;
        }
    }
}
