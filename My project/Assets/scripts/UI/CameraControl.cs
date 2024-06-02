using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Vector3 cameraPositionOffset;
    public GameObject player;
    public float range = 5.0f;
    public float followSpeed = 2.0f;

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        ObeyPlayer();
    }

    void ObeyPlayer()
    {
        // プレイヤーの回転角度を取得
        float rotationZ = player.transform.eulerAngles.z;

        // 角度をラジアンに変換
        float radians = rotationZ * Mathf.Deg2Rad;

        // 向きベクトルを計算
        Vector3 direction = new Vector3(Mathf.Cos(radians) * range, Mathf.Sin(radians) * range, -10f);

        // カメラの目標位置を計算
        Vector3 targetPosition = player.transform.position + cameraPositionOffset + direction;

        // カメラの現在位置を目標位置に向かってスムーズに移動
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, followSpeed * Time.fixedDeltaTime);
    }
}
