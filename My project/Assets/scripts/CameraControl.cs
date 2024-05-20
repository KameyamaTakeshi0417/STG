using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Vector3 cameraPositionOffset;
    public GameObject player;
public float range=5.0f;
    void Update()
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
        Vector3 direction = new Vector3(Mathf.Cos(radians)*range, Mathf.Sin(radians)*range, -10f);

        // カメラの新しい位置を計算
        transform.position = player.transform.position + cameraPositionOffset + direction;
    }
}
