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
        // プレイヤーに対するカメラの相対位置を設定
        targetPosition = player.transform.position + new Vector3(0, 0, -10);

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
