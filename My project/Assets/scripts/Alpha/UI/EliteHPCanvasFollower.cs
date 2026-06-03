using UnityEngine;

namespace Alpha.UI
{
    /// <summary>
    /// World‑Space Canvas 用のフォロワーコンポーネント。
    /// EliteEnemyHPCanvas のインスタンスに自動で付与され、対象の敵 Transform を追従します。
    /// </summary>
    public class EliteHPCanvasFollower : MonoBehaviour
    {
        /// <summary>追従対象の Transform（敵）</summary>
        public Transform target;

        /// <summary>表示位置の微調整オフセット（任意）</summary>
        public Vector3 offset = new Vector3(0f, 2f, 0f);

        private void LateUpdate()
        {
            if (target != null)
            {
                // 敵のワールド座標にオフセットを加えて Canvas を配置
                transform.position = target.position + offset;
                // カメラに正面を向かせる（2D 用に Z 軸だけ回転）
                var cam = Camera.main;
                if (cam != null)
                {
                    // カメラの方向を見るように回転（LookAt の代わりに平面回転）
                    Vector3 dir = cam.transform.position - transform.position;
                    dir.z = 0f; // Z 軸は固定
                    transform.rotation = Quaternion.LookRotation(Vector3.forward, dir);
                }
            }
        }
    }
}
