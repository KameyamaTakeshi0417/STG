using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Health enemy; // 参照する敵オブジェクト
    public Image healthBarImage; // HPバーのImageコンポーネント
    public Vector3 offset; // HPバーの位置オフセット

    void Update()
    {
        if (enemy != null)
        {
            // HPバーを更新
            healthBarImage.fillAmount = enemy.getCurrentHP() / enemy.getHP();

            // HPバーの位置を敵の上に設定
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(enemy.transform.position + offset);
            transform.position = screenPosition;
        }
    }
}
