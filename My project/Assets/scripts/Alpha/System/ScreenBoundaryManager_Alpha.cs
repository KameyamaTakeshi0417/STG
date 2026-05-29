using UnityEngine;

namespace Alpha.Core
{
    public class ScreenBoundaryManager_Alpha : MonoBehaviour
    {
        public static ScreenBoundaryManager_Alpha Instance { get; private set; }

        [Header("Boundary Settings")]
        [Tooltip("画面サイズに関係なく、手動で指定する領域のサイズ（幅と高さ）")]
        public Vector2 boundarySize = new Vector2(30f, 20f);

        [Tooltip("生成される壁の厚み（プレイヤーがすり抜けないように十分に分厚くします）")]
        public float wallThickness = 5f;

        [Tooltip("壁に設定するタグ（弾が壁に当たって消えるようになります）")]
        public string wallTag = "wall";

        // ワールド座標での画面の端
        public float MinX { get; private set; }
        public float MaxX { get; private set; }
        public float MinY { get; private set; }
        public float MaxY { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            UpdateBoundaries();
            GeneratePhysicalWalls();
        }

        private void UpdateBoundaries()
        {
            Vector3 centerPos = Vector3.zero;
            float halfWidth = boundarySize.x / 2f;
            float halfHeight = boundarySize.y / 2f;

            MinX = centerPos.x - halfWidth;
            MaxX = centerPos.x + halfWidth;
            MinY = centerPos.y - halfHeight;
            MaxY = centerPos.y + halfHeight;
        }

        private void GeneratePhysicalWalls()
        {
            // 既存の壁があれば削除（エディタでの再生成用）
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            Vector3 center = Vector3.zero;

            // 上の壁
            CreateWall("Wall_Top", new Vector2(center.x, MaxY + wallThickness / 2f), new Vector2(boundarySize.x + wallThickness * 2, wallThickness));
            // 下の壁
            CreateWall("Wall_Bottom", new Vector2(center.x, MinY - wallThickness / 2f), new Vector2(boundarySize.x + wallThickness * 2, wallThickness));
            // 左の壁
            CreateWall("Wall_Left", new Vector2(MinX - wallThickness / 2f, center.y), new Vector2(wallThickness, boundarySize.y));
            // 右の壁
            CreateWall("Wall_Right", new Vector2(MaxX + wallThickness / 2f, center.y), new Vector2(wallThickness, boundarySize.y));
        }

        private void CreateWall(string wallName, Vector2 position, Vector2 size)
        {
            GameObject wallObj = new GameObject(wallName);
            wallObj.transform.parent = transform;
            wallObj.transform.position = position;
            wallObj.tag = wallTag;

            // 物理的な壁としてBoxCollider2Dを追加
            BoxCollider2D col = wallObj.AddComponent<BoxCollider2D>();
            col.size = size;
            
            // 敵やプレイヤーが物理的にぶつかるようにTriggerにはしない
            col.isTrigger = false;
        }

        /// <summary>
        /// 指定した座標を、画面内に制限（Clamp）して返します。
        /// </summary>
        public Vector3 ClampPositionToScreen(Vector3 pos)
        {
            // 壁の厚みを考慮して内側にクランプ（めり込み防止）
            float margin = 0.5f;
            float x = Mathf.Clamp(pos.x, MinX + margin, MaxX - margin);
            float y = Mathf.Clamp(pos.y, MinY + margin, MaxY - margin);
            return new Vector3(x, y, pos.z);
        }

        /// <summary>
        /// 指定した座標が、領域外に出ているか判定します（予備のチェック用）
        /// </summary>
        public bool IsOutOfBounds(Vector3 pos)
        {
            float margin = 1.0f;
            return pos.x < MinX - margin || pos.x > MaxX + margin || pos.y < MinY - margin || pos.y > MaxY + margin;
        }

        private void OnDrawGizmos()
        {
            UpdateBoundaries();

            // 指定された領域を緑で描画（これが実際のプレイヤーが移動できる限界）
            Gizmos.color = Color.green;
            Vector3 center = Vector3.zero;
            Gizmos.DrawWireCube(center, new Vector3(boundarySize.x, boundarySize.y, 0f));

            // 壁の厚みを薄い赤で描画
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            // 上
            Gizmos.DrawCube(new Vector3(center.x, MaxY + wallThickness / 2f, 0f), new Vector3(boundarySize.x + wallThickness * 2, wallThickness, 0f));
            // 下
            Gizmos.DrawCube(new Vector3(center.x, MinY - wallThickness / 2f, 0f), new Vector3(boundarySize.x + wallThickness * 2, wallThickness, 0f));
            // 左
            Gizmos.DrawCube(new Vector3(MinX - wallThickness / 2f, center.y, 0f), new Vector3(wallThickness, boundarySize.y, 0f));
            // 右
            Gizmos.DrawCube(new Vector3(MaxX + wallThickness / 2f, center.y, 0f), new Vector3(wallThickness, boundarySize.y, 0f));
        }
    }
}
