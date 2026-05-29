using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    [Header("Follow")]
    public Vector3 cameraPositionOffset = Vector3.zero;
    public GameObject player;
    public float range = 0.0f;          // プレイヤー向き方向へのオフセット量
    public float followSmoothTime = 0.15f;

    [Header("Zoom (Orthographic)")]
    [Tooltip("ズームイン最大（小さいほど拡大）")]
    public float minOrthoSize = 4.0f;

    [Tooltip("ズームアウト最大（大きいほど縮小）")]
    public float maxOrthoSize = 12.0f;

    [Tooltip("フィールド中央（ズームアウト最大時に画角中心になる点）")]
    public Vector3 fieldCenter = Vector3.zero;

    [Tooltip("ホイールの感度")]
    public float zoomSpeed = 2.0f;

    [Tooltip("ズームのホイール方向を反転させる場合はON")]
    public bool invertZoomDirection = false;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;

    // 0=ズームアウト最大（fieldCenter中心）, 1=ズームイン最大（player中心）
    private float zoomT = 1f;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (player == null)
            player = GameObject.Find("Player"); // Tag運用なら FindWithTag 推奨
    }

    void Start()
    {
        // 初期orthographicSizeからzoomTを推定（設定を変えても自然に追従）
        float size = cam.orthographicSize;
        zoomT = Mathf.InverseLerp(maxOrthoSize, minOrthoSize, size); // size=min→1, size=max→0
        zoomT = Mathf.Clamp01(zoomT);
    }

    void Update()
    {
        HandleZoomInput(Time.deltaTime);
    }

    void LateUpdate()
    {
        ObeyPlayer(Time.deltaTime);
    }

    void HandleZoomInput(float dt)
    {
        if (cam == null) return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.0001f) return;

        // 変数名を変更したことでインスペクタの古い設定がリセットされ、確実に新しい挙動が適用されます。
        // デフォルト(invertZoomDirection=false)では、奥スクロール(+)でズームイン(sign=1)になります。
        float sign = invertZoomDirection ? -1f : 1f; 
        float delta = scroll * sign;

        // zoomT: 1がズームイン、0がズームアウト
        zoomT = Mathf.Clamp01(zoomT + delta * zoomSpeed * dt);

        // orthographicSize: ズームインほど小、ズームアウトほど大
        cam.orthographicSize = Mathf.Lerp(maxOrthoSize, minOrthoSize, zoomT);
    }

    void ObeyPlayer(float dt)
    {
        if (player == null) return;

        // プレイヤーの回転角度（Z）を向きとして使う
        float rotationZ = player.transform.eulerAngles.z;
        float radians = rotationZ * Mathf.Deg2Rad;

        // 向き方向へrange分だけカメラをずらす（既存仕様）
        Vector3 direction = new Vector3(
            Mathf.Cos(radians) * range,
            Mathf.Sin(radians) * range,
            0f
        );

        // フォーカス点：ズームアウトほどfieldCenter寄り、ズームインほどplayer寄り
        // zoomT=0 → fieldCenter、zoomT=1 → player
        Vector3 focusPoint = Vector3.Lerp(fieldCenter, player.transform.position, zoomT);

        // 目標位置（Zは固定）
        Vector3 targetPosition = focusPoint + cameraPositionOffset + direction;
        targetPosition.z = -10f;

        // SmoothDampでカメラを滑らかに移動
        Vector3 newPos = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            followSmoothTime
        );

        // 【追加】カメラが壁の外を絶対に映さないように、現在の位置を厳密にクランプする
        if (Alpha.Core.ScreenBoundaryManager_Alpha.Instance != null)
        {
            var bounds = Alpha.Core.ScreenBoundaryManager_Alpha.Instance;
            float camHalfHeight = cam.orthographicSize;
            float camHalfWidth = camHalfHeight * cam.aspect;

            float minX = bounds.MinX + camHalfWidth;
            float maxX = bounds.MaxX - camHalfWidth;
            float minY = bounds.MinY + camHalfHeight;
            float maxY = bounds.MaxY - camHalfHeight;

            // X軸の制限（広すぎる場合は中央に固定）
            if (minX > maxX)
            {
                newPos.x = (bounds.MinX + bounds.MaxX) / 2f;
            }
            else
            {
                newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            }

            // Y軸の制限（広すぎる場合は中央に固定）
            if (minY > maxY)
            {
                newPos.y = (bounds.MinY + bounds.MaxY) / 2f;
            }
            else
            {
                newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            }
        }

        transform.position = newPos;
    }
}