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

    [Tooltip("ホイールの感度")]
    public float zoomSpeed = 2.0f;

    [Tooltip("ズームの滑らかさ")]
    public float zoomSmoothTime = 0.1f;

    [Tooltip("ズームのホイール方向を反転させる場合はON")]
    public bool invertZoomDirection = false;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private float targetOrthoSize;
    private float zoomVelocity;

    [HideInInspector] public Vector3 shakeOffset = Vector3.zero; // JuiceManager用

    [Header("Tilt (Juice)")]
    public float maxTiltAngle = 2.0f;
    public float tiltSmoothTime = 0.1f;
    private float currentTiltVelocity;
    private float currentTilt;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (player == null)
            player = GameObject.Find("Player"); // Tag運用なら FindWithTag 推奨
            
        targetOrthoSize = cam.orthographicSize;
    }

    void Start()
    {
        targetOrthoSize = Mathf.Clamp(targetOrthoSize, minOrthoSize, maxOrthoSize);
    }

    void Update()
    {
        HandleZoomInput();
    }

    void LateUpdate()
    {
        ObeyPlayer(Time.deltaTime);
    }

    void HandleZoomInput()
    {
        if (cam == null) return;
        
        // ポーズ中やチュートリアル中はズームしない
        if (Time.timeScale == 0f) return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            float sign = invertZoomDirection ? -1f : 1f; 
            float delta = scroll * sign;
            
            // 目標のズームサイズを変更 (スクロール量 * 感度)
            // ズームアウト方向(+)はサイズが大きくなる、ズームイン方向(-)はサイズが小さくなる
            // 直感的には手前に引くとズームアウトなので、deltaを加算する
            targetOrthoSize -= delta * zoomSpeed;
            targetOrthoSize = Mathf.Clamp(targetOrthoSize, minOrthoSize, maxOrthoSize);
        }

        // 実際のカメラサイズを滑らかに追従させる
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetOrthoSize, ref zoomVelocity, zoomSmoothTime);
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

        // 以前の fieldCenter に無理やり引っ張るロジックは削除し、
        // 常にプレイヤーをフォーカスするように変更しました。（プレイ支障改善のため）
        Vector3 focusPoint = player.transform.position;

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

        // ScreenShake用のオフセットを加算して適用
        transform.position = newPos + shakeOffset;

        // 【追加】カメラティルト
        float targetTilt = 0f;
        if (player != null && Time.timeScale > 0f)
        {
            float h = Input.GetAxisRaw("Horizontal");
            targetTilt = -h * maxTiltAngle; // 移動方向の逆向きに回転（視覚的には進行方向にカメラが傾く）
        }
        currentTilt = Mathf.SmoothDampAngle(currentTilt, targetTilt, ref currentTiltVelocity, tiltSmoothTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentTilt);
    }
}
