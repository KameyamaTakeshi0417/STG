using System;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Result
{
    [System.Serializable]
    public class ResultPanelState
    {
        public int index;
        public int row;
        public int col;
        public ResultMission mission;
        public bool isCompleted;
        public bool alreadyBroken;   // 過去のステージで既に割れているか
        public bool justBroken;      // 今回のステージで新しく割れたか
    }

    [System.Serializable]
    public class LineDefinition
    {
        public string lineName;
        public List<int> cellIndices = new List<int>();
        public bool isComplete;
        public bool alreadyComplete; // 過去のステージで既に完成していたか
        public bool justComplete;    // 今回新しく完成したか
    }

    public class ResultMatrixManager_Alpha : MonoBehaviour
    {
        private static ResultMatrixManager_Alpha _instance;
        public static ResultMatrixManager_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ResultMatrixManager_Alpha>();
                    if (_instance == null)
                    {
                        var go = new GameObject("ResultMatrixManager_Alpha");
                        _instance = go.AddComponent<ResultMatrixManager_Alpha>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Grid Settings")]
        [Tooltip("マトリクスのグリッドサイズ（今回は3、全6ステージで5に拡張可能）")]
        public int gridSize = 3;

        [Header("State (Read Only)")]
        public ResultPanelState[] panels;
        public List<LineDefinition> lines = new List<LineDefinition>();
        public int completedLinesCount = 0;
        public int newlyCompletedLinesCount = 0;
        public int newlyCompletedPanelsCount = 0;
        public int totalAccumulatedScore = 0;
        public string rankGrade = "C";

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
        }

        private void Start()
        {
            if (panels == null || panels.Length != gridSize * gridSize)
            {
                InitializeGrid(gridSize);
            }
        }

        /// <summary>
        /// 1ラン（ゲーム全体）の開始時に盤面を新規生成・初期化する
        /// </summary>
        public void InitializeGrid(int size = 3)
        {
            gridSize = size;
            int totalCells = gridSize * gridSize;
            panels = new ResultPanelState[totalCells];

            var missions = ResultMissionFactory_Alpha.GenerateGridMissions(gridSize);

            for (int i = 0; i < totalCells; i++)
            {
                panels[i] = new ResultPanelState
                {
                    index = i,
                    row = i / gridSize,
                    col = i % gridSize,
                    mission = missions[i],
                    isCompleted = false,
                    alreadyBroken = false,
                    justBroken = false
                };
            }

            BuildLineDefinitions();
            completedLinesCount = 0;
            newlyCompletedLinesCount = 0;
            newlyCompletedPanelsCount = 0;
            totalAccumulatedScore = 0;
            rankGrade = "C";

            Debug.Log($"[ResultMatrixManager] Initialized persistent {gridSize}x{gridSize} grid for NEW GAME RUN.");
        }

        private void BuildLineDefinitions()
        {
            lines.Clear();

            // 1. 各行（ヨコ列）
            for (int r = 0; r < gridSize; r++)
            {
                var line = new LineDefinition { lineName = $"Row {r + 1}" };
                for (int c = 0; c < gridSize; c++)
                {
                    line.cellIndices.Add(r * gridSize + c);
                }
                lines.Add(line);
            }

            // 2. 各列（タテ列）
            for (int c = 0; c < gridSize; c++)
            {
                var line = new LineDefinition { lineName = $"Col {c + 1}" };
                for (int r = 0; r < gridSize; r++)
                {
                    line.cellIndices.Add(r * gridSize + c);
                }
                lines.Add(line);
            }

            // 3. 左上 -> 右下（主対角線）
            var diag1 = new LineDefinition { lineName = "Diagonal Main" };
            for (int i = 0; i < gridSize; i++)
            {
                diag1.cellIndices.Add(i * gridSize + i);
            }
            lines.Add(diag1);

            // 4. 右上 -> 左下（副対角線）
            var diag2 = new LineDefinition { lineName = "Diagonal Anti" };
            for (int i = 0; i < gridSize; i++)
            {
                diag2.cellIndices.Add(i * gridSize + (gridSize - 1 - i));
            }
            lines.Add(diag2);
        }

        /// <summary>
        /// ステージボス撃破時、1ゲーム通算の実績をもとに新たに達成されたマスとラインを判定する
        /// </summary>
        public void EvaluateAllPanels(RunStatsTracker_Alpha stats)
        {
            if (panels == null || stats == null) return;

            newlyCompletedPanelsCount = 0;
            newlyCompletedLinesCount = 0;

            // 各マスの達成判定
            for (int i = 0; i < panels.Length; i++)
            {
                var p = panels[i];
                p.justBroken = false;

                if (p.mission != null)
                {
                    bool completedNow = p.mission.IsCompleted(stats);
                    if (completedNow)
                    {
                        p.isCompleted = true;
                        if (!p.alreadyBroken)
                        {
                            p.justBroken = true;
                            newlyCompletedPanelsCount++;
                            totalAccumulatedScore += p.mission.scorePoints;
                        }
                    }
                }
            }

            // 各ラインの揃い判定
            completedLinesCount = 0;
            foreach (var line in lines)
            {
                line.justComplete = false;
                bool lineDone = true;
                foreach (int idx in line.cellIndices)
                {
                    if (idx >= 0 && idx < panels.Length && !panels[idx].isCompleted)
                    {
                        lineDone = false;
                        break;
                    }
                }

                line.isComplete = lineDone;
                if (lineDone)
                {
                    completedLinesCount++;
                    if (!line.alreadyComplete)
                    {
                        line.justComplete = true;
                        newlyCompletedLinesCount++;
                        totalAccumulatedScore += 1000; // 1ラインにつき+1000pt
                    }
                }
            }

            // 全消し判定
            bool isAllCleared = true;
            foreach (var p in panels)
            {
                if (!p.isCompleted) { isAllCleared = false; break; }
            }
            if (isAllCleared && stats.isGameCleared)
            {
                totalAccumulatedScore += 5000;
            }

            // ランク評価
            if (isAllCleared) rankGrade = "S+";
            else if (completedLinesCount >= 3 || totalAccumulatedScore >= 4000) rankGrade = "S";
            else if (completedLinesCount >= 2 || totalAccumulatedScore >= 2500) rankGrade = "A";
            else if (completedLinesCount >= 1 || totalAccumulatedScore >= 1200) rankGrade = "B";
            else rankGrade = "C";

            Debug.Log($"[ResultMatrixManager] Evaluation: NewlyBroken={newlyCompletedPanelsCount}, NewlyLines={newlyCompletedLinesCount}, TotalLines={completedLinesCount}, TotalScore={totalAccumulatedScore}, Rank={rankGrade}");
        }

        /// <summary>
        /// 今回割れたマス・ラインを「達成済み」として確定する（次ステージ以降で二重割りしないため）
        /// </summary>
        public void FinalizeEvaluation()
        {
            if (panels != null)
            {
                foreach (var p in panels)
                {
                    if (p.justBroken)
                    {
                        p.alreadyBroken = true;
                        p.justBroken = false;
                    }
                }
            }
            if (lines != null)
            {
                foreach (var l in lines)
                {
                    if (l.justComplete)
                    {
                        l.alreadyComplete = true;
                        l.justComplete = false;
                    }
                }
            }
        }
    }
}
