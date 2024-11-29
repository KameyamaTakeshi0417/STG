using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject player; // プレイヤーの参照
    public float followSpeed = 2.0f; // カメラがプレイヤーを追従する速度

    public float noInputTimeThreshold = 3.0f; // 入力がない時間のしきい値
    public float moveToPlayerDuration = 2.0f; // プレイヤーに近づくまでの時間（ズームイン時間）
    public float moveAwayFromPlayerDuration = 0.5f; // 元の位置に戻るまでの時間（ズームアウト時間）
    public float targetZoomInSize = 3.0f; // ズームインしたときのカメラサイズ
    public float originalZoomSize = 5.0f; // 通常のカメラサイズ

    private Camera cam; // カメラコンポーネント
    private float noInputTimer = 0.0f; // 入力がない時間を記録するタイマー
    private bool isZoomingIn = false; // カメラがズームインしているかどうかのフラグ
    private Vector3 targetPosition; // カメラの目標位置

    void Awake()
    {
        // プレイヤーとカメラの取得
        player = GameObject.Find("Player");
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // プレイヤーの入力を監視
        if (Input.anyKey)
        {
            noInputTimer = 0.0f; // 入力があればタイマーをリセット
            isZoomingIn = false; // 入力があったのでズームアウト
        }
        else
        {
            noInputTimer += Time.deltaTime; // 入力がない時間を計測
        }

        // 入力が一定時間ない場合、カメラをズームインする
        if (noInputTimer >= noInputTimeThreshold)
        {
            isZoomingIn = true; // しきい値を超えたらズームイン開始
        }

        AdjustCameraSize();
    }

    void FixedUpdate()
    {
        ObeyPlayer();
    }

void ObeyPlayer()
{
    // マウスのスクリーン座標を取得
    Vector3 mousePosition = Input.mousePosition;

    // マウス位置をビューポート座標に変換し、0から1の範囲に制限（範囲外なら0または1に制限）
    float clampedViewportX = Mathf.Clamp(mousePosition.x / Screen.width, 0f, 1f);
    float clampedViewportY = Mathf.Clamp(mousePosition.y / Screen.height, 0f, 1f);

    // ビューポート座標をワールド座標に変換
    Vector3 clampedWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(clampedViewportX, clampedViewportY, -Camera.main.transform.position.z));
    clampedWorldPosition.z = -10f; // カメラのZ位置を維持

    // プレイヤーの位置とクランプしたマウス位置の中間を計算
    targetPosition = (player.transform.position + clampedWorldPosition) / 2;

    // カメラの現在位置を目標位置に向かってスムーズに移動
    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);
}



    void AdjustCameraSize()
    {
        if (isZoomingIn)
        {
            // カメラをズームイン
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoomInSize, Time.deltaTime / moveToPlayerDuration);
        }
        else
        {
            // カメラを元のサイズに戻す（ズームアウト）
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, originalZoomSize, Time.deltaTime / moveAwayFromPlayerDuration);
        }
    }
}
